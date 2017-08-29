using System.Collections.Generic;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using CQRSlite.Config;
using LiteDB;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Owin;
using Quartz;
using Quartz.Impl;
using Swashbuckle.Application;
using Televic.Mycro.Bus;
using Televic.Mycro.Notification;
using Televic.Mycro.Scheduling;

namespace Televic.Mycro.Web
{
    public abstract class Startup
    {
        private IContainer _container;

        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.MapSignalR();
            ConfigureDependencyInjection(appBuilder);
            ConfigureMessageBus();
            ConfigureWebApi(appBuilder);
            ConfigureNancy(appBuilder);
            StartScheduler(appBuilder);
        }

        private void StartScheduler(IAppBuilder appBuilder)
        {
            var scheduler = _container.Resolve<IScheduler>();
            scheduler.JobFactory = new AutofacJobFactory(_container);
            ScheduleJobs(scheduler);
            scheduler.Start();
            appBuilder.OnDisposing(() => scheduler.Shutdown());
        }

        private static void ConfigureNancy(IAppBuilder appBuilder)
        {
            appBuilder.UseNancy(config => config.Bootstrapper = new CustomNancyBootstrapper());
        }

        private void ConfigureDependencyInjection(IAppBuilder appBuilder)
        {
            var builder = new ContainerBuilder();
            RegisterDependencies(builder);
            _container = builder.Build();
            appBuilder.UseAutofacMiddleware(_container);
        }

        private void ConfigureMessageBus()
        {
            var bus = new BusRegistrar(new DependencyResolver(_container));
            bus.Register(ApplicationAssemblyType, typeof(NotificationEventHandler));
        }

        private void RegisterDependencies(ContainerBuilder builder)
        {
            builder.RegisterModule<BusRegistrationModule>();
            builder.RegisterInstance(GlobalHost.ConnectionManager.GetHubContext<NotificationEventHub, INotificationClient>())
                .As<IHubContext<INotificationClient>>();
            builder.RegisterType<Notifier>().InstancePerLifetimeScope();
            builder.RegisterType<NotificationEventHandler>().InstancePerLifetimeScope();
            builder.RegisterInstance(new LiteRepository(ConnectionString));
            builder.RegisterAssemblyTypes(ApplicationAssemblyType.Assembly)
                .Where(t => t.Name.EndsWith("Handler"))
                .AsSelf()
                .InstancePerLifetimeScope();
            builder.RegisterApiControllers(ApplicationAssemblyType.Assembly);
            builder.RegisterAssemblyModules(ApplicationAssemblyType.Assembly);
            builder.RegisterInstance(new StdSchedulerFactory().GetScheduler()).As<IScheduler>();
            builder.RegisterAssemblyTypes(ApplicationAssemblyType.Assembly)
                .Where(t => t.Name.EndsWith("Job"))
                .AsSelf();
        }

        private void ConfigureWebApi(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(_container) };
            ConfigureJsonFormatting(config);
            config.MapHttpAttributeRoutes();
            config.EnableSwagger(c => ConfigureSwagger(c)).EnableSwaggerUi();       
            appBuilder.UseWebApi(config);
        }

        private static void ConfigureJsonFormatting(HttpConfiguration config)
        {
            var defaultSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter {CamelCaseText = true},
                }
            };

            JsonConvert.DefaultSettings = () => defaultSettings;

            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings = defaultSettings;
        }

        protected virtual string ApplicationName => ApplicationAssemblyType.Assembly.GetName().Name;
        protected virtual System.Type ApplicationAssemblyType => GetType();

        protected virtual void ScheduleJobs(IScheduler scheduler) { }

        protected virtual string ConnectionString => ConfigurationManager.ConnectionStrings[$"{ApplicationName}DB"].ConnectionString;
        protected virtual InfoBuilder ConfigureSwagger(SwaggerDocsConfig c)
        {
            return c.SingleApiVersion("v1", $"{ApplicationName} REST API");
        }
    }
}

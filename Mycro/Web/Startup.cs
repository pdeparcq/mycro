using System.Collections.Generic;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Common.Logging.Configuration;
using Common.Logging.NLog;
using CQRSlite.Config;
using LiteDB;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Host.HttpListener;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Config;
using NLog.Targets;
using Owin;
using Quartz;
using Quartz.Impl;
using Swashbuckle.Application;
using Televic.Mycro.Bus;
using Televic.Mycro.Documents;
using Televic.Mycro.Notification;
using Televic.Mycro.Scheduling;

namespace Televic.Mycro.Web
{
    public abstract class Startup
    {
        private IContainer _container;

        public void Configuration(IAppBuilder appBuilder)
        {
            ReferenceDummyTypes();
            ConfigureDefaultLogging();
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
            builder.RegisterApiControllers(typeof(DocumentsController).Assembly);
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
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("API Default", "api/{controller}/{id}", new { id = RouteParameter.Optional });
            ConfigureJsonFormatting(config);
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

        protected virtual string ConnectionString
        {
            get
            {
                var connectionString = ConfigurationManager.ConnectionStrings[$"{ApplicationName}DB"];
                return connectionString != null ? connectionString.ConnectionString : "Filename=database.db";
            }
        }

        protected virtual InfoBuilder ConfigureSwagger(SwaggerDocsConfig c)
        {
            c.OperationFilter<ImportFileParamType>();
            return c.SingleApiVersion("v1", $"{ApplicationName} REST API");
        }

        private void ConfigureDefaultLogging()
        {
            if (LogManager.Configuration == null)
            {
                //NLog
                var config = new LoggingConfiguration();
                var consoleTarget = new ColoredConsoleTarget
                {
                    Layout = @"${longdate} ${uppercase:${level}} ${message} ${exception:format=ToString}"
                };
                config.AddTarget("console", consoleTarget);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));

                LogManager.Configuration = config;

                //Common.Logging
                Common.Logging.LogManager.Adapter = new NLogLoggerFactoryAdapter(new NameValueCollection { ["configType"] = "INLINE" });
            }
        }

        private static void ReferenceDummyTypes()
        {
            //avoid need to reference owin.host.httplistener nuget package from consuming side
            var dummyType = typeof(OwinHttpListener);
        }
    }
}

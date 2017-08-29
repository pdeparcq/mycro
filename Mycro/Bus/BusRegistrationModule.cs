using Autofac;
using CQRSlite.Bus;
using CQRSlite.Cache;
using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using Televic.Mycro.Events;

namespace Televic.Mycro.Bus
{
    public class BusRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CustomMessageBus>().SingleInstance();
            builder.Register(c => c.Resolve<CustomMessageBus>()).As<ICommandSender>();
            builder.Register(c => c.Resolve<CustomMessageBus>()).As<IEventPublisher>();
            builder.Register(c => c.Resolve<CustomMessageBus>()).As<IHandlerRegistrar>();

            builder.RegisterType<Session>().As<ISession>().InstancePerLifetimeScope();
            builder.RegisterType<LiteDbEventStore>().As<IEventStore>();
            builder.RegisterType<MemoryCache>().As<ICache>().InstancePerLifetimeScope();
            builder.Register(
                c =>
                    new CacheRepository(new Repository(c.Resolve<IEventStore>()), c.Resolve<IEventStore>(),
                        c.Resolve<ICache>())).As<IRepository>().InstancePerLifetimeScope();
        }
    }
}

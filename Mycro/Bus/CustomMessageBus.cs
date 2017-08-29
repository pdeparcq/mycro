using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Bus;
using CQRSlite.Commands;
using CQRSlite.Events;
using CQRSlite.Messages;
using MemBus;
using MemBus.Configurators;
using Newtonsoft.Json;
using NLog;
using Televic.Mycro.Notification;

namespace Televic.Mycro.Bus
{
    public class CustomMessageBus : ICommandSender, IEventPublisher, IHandlerRegistrar
    {
        private readonly IBus _bus;
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CustomMessageBus(Notifier notifier)
        {
            _bus = BusSetup.StartWith<Conservative>().Construct();
        }

        public async Task Send<T>(T command, CancellationToken cancellationToken = new CancellationToken()) where T : class, ICommand
        {
            await _bus.PublishAsync(command);
        }

        public async Task Publish<T>(T @event, CancellationToken cancellationToken = new CancellationToken()) where T : class, IEvent
        {   
            await _bus.PublishAsync(@event);
        }

        public void RegisterHandler<T>(Func<T, CancellationToken, Task> handler) where T : class, IMessage
        {
            _bus.Subscribe(async (T message) =>
            {
                try
                {
                    Logger.Info($"Handling {message}: {JsonConvert.SerializeObject(message)}");
                    await handler(message, new CancellationToken());
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            });
        }
    }
}

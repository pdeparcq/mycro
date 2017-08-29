using System.Threading.Tasks;
using CQRSlite.Events;

namespace Televic.Mycro.Notification
{
    public class NotificationEventHandler : IEventHandler<IEvent>
    {
        private readonly Notifier _notifier;

        public NotificationEventHandler(Notifier notifier)
        {
            _notifier = notifier;
        }

        public Task Handle(IEvent message)
        {
            _notifier.Notify(message);

            return Task.CompletedTask;
        }
    }
}

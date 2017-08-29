using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Televic.Mycro.Notification
{
    public interface INotificationClient
    {
        void Notify(Notification e);
    }

    public class NotificationEventHub : Hub<INotificationClient>
    {
        private readonly string AdminModuleName = "admin";

        public async Task Subscribe(string module)
        {
            await Groups.Add(Context.ConnectionId, module);

            Publish(new Notification
            {
                ModuleName = AdminModuleName,
                Name = "ClientSubscribedToModule",
                Data = new
                {
                    ClientConnectionId = Context.ConnectionId,
                    ModuleName = module
                }
            });
        }

        public async Task Unsubscribe(string module)
        {
            await Groups.Remove(Context.ConnectionId, module);

            Publish(new Notification
            {
                ModuleName = AdminModuleName,
                Name = "ClientUnsubscribedFromModule",
                Data = new
                {
                    ClientConnectionId =  Context.ConnectionId,
                    ModuleName = module
                }
            });
        }

        public override Task OnConnected()
        {
            Publish(new Notification
            {
                ModuleName = AdminModuleName,
                Name = "ClientConnected",
                Data = new
                {
                    ClientConnectionId = Context.ConnectionId
                }
            });
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Publish(new Notification
            {
                ModuleName = AdminModuleName,
                Name = "ClientDisconnected",
                Data = new
                {
                    ClientConnectionId = Context.ConnectionId
                }
            });
            return base.OnDisconnected(stopCalled);
        }

        private void Publish(Notification notification)
        {
            Clients.Group(notification.ModuleName).Notify(notification);
        }
    }
}

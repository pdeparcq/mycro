using System;
using System.Collections.Generic;
using CQRSlite.Events;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using NLog;

namespace Televic.Mycro.Notification
{
    public class Notifier
    {
        private readonly IHubContext<INotificationClient> _hubContext;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Notifier(IHubContext<INotificationClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public void Notify(IEvent e)
        {
            var notification = new Notification
            {
                ModuleName = GetModuleName(e.GetType()),
                Name = e.GetType().Name,
                Data = e
            };
            Logger.Info($"Sending notification: {JsonConvert.SerializeObject(notification)}");
            _hubContext.Clients.Group(notification.ModuleName).Notify(notification);
        }

        private string GetModuleName(Type t)
        {
            if (t.Namespace == null) return "default";

            var node = new LinkedList<string>(t.Namespace.ToLower().Split('.')).Last;
            if (node.Value == "events")
                node = node.Previous;

            return node?.Value ?? "default";
        }
    }
}

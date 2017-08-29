using System;
using CQRSlite.Events;
using Newtonsoft.Json;

namespace Televic.Mycro.Events
{
    public class ApplicationEvent : IEvent
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonIgnore]
        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public ApplicationEvent()
        {
            TimeStamp = DateTimeOffset.UtcNow;
        }
    }
}

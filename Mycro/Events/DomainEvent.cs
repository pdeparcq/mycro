using System;
using CQRSlite.Domain;
using CQRSlite.Events;
using LiteDB;
using Newtonsoft.Json;

namespace Televic.Mycro.Events
{
    public abstract class DomainEvent<T> : IEvent where T : AggregateRoot
    {
        [BsonIndex]
        [JsonProperty("AggregateId")]
        public Guid Id { get; set; }

        [BsonIndex]
        [JsonIgnore]
        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        protected DomainEvent() { }

        protected DomainEvent(T entity)
        {
            Id = entity.Id;
            TimeStamp = DateTimeOffset.UtcNow;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Events;
using LiteDB;

namespace Televic.Mycro.Events
{
    public class LiteDbEventStore : IEventStore
    {
        private readonly LiteRepository _repository;
        private readonly IEventPublisher _eventPublisher;

        public LiteDbEventStore(LiteRepository repository, IEventPublisher eventPublisher)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = new CancellationToken())
        {
            var list = events.ToList();
            foreach (var e in list)
            {
                _repository.Insert(new EventDescriptor
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = e
                }, "events");
                await _eventPublisher.Publish(e, cancellationToken);
            }
        }

        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = new CancellationToken())
        {
            return await Task.Run(() => _repository.Query<EventDescriptor>("events").Where(e => e.Value.Id == aggregateId && e.Value.Version > fromVersion).ToList().Select(e => e.Value), cancellationToken);
        }
    }
}

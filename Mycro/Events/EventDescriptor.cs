using CQRSlite.Events;

namespace Televic.Mycro.Events
{
    public class EventDescriptor
    {
        public string Id { get; set; }
        public IEvent Value { get; set; }
    }
}

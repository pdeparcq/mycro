using System;

namespace Televic.Mycro.Notification
{
    public class Notification
    {
        public string Name { get; set; }

        public string ModuleName { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public object Data { get; set; }

        public Notification()
        {
            Timestamp = DateTimeOffset.UtcNow;
        }
    }
}

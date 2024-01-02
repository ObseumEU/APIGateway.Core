using System;

namespace APIGateway.Core.Kafka.Messages
{
    [Obsolete]
    public class WebhookEvent
    {
        public string EventType { get; set; }
        public string JsonData { get; set; }
    }
}

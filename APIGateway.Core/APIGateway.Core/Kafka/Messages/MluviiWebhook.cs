using System.IO;
using APIGateway.Core.Controllers.Webhook;
using Newtonsoft.Json.Linq;

namespace APIGateway.Core.Kafka.Messages
{
    public class WebhookEvent
    {
        public string EventType { get; set; }
        public string JsonData { get; set; }
    }
}

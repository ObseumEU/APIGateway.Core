using APIGateway.Core.Controllers.Webhook;
using Newtonsoft.Json.Linq;

namespace APIGateway.Core.Kafka.Messages
{
    public class WebhookEvent
    {
        public string eventType { get; set; }
        public JObject data { get; set; }
    }
}

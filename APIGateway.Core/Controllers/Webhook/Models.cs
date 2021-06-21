using System;
using Newtonsoft.Json;

namespace APIGateway.Core.Controllers.Webhook
{
    public class SessionEndedPayload
    {
        [JsonProperty("eventType")] public string EventType { get; set; }

        [JsonProperty("data")] public Data Data { get; set; }
    }

    public class SessionStarted
    {
        [JsonProperty("eventType")] public string EventType { get; set; }

        [JsonProperty("data")] public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("channel")] public string Channel { get; set; }

        [JsonProperty("source")] public string Source { get; set; }

        [JsonProperty("started")] public DateTimeOffset? Started { get; set; }

        [JsonProperty("ended")] public DateTimeOffset Ended { get; set; }

        [JsonProperty("tenantId")] public DateTimeOffset TenantId { get; set; }
    }
}
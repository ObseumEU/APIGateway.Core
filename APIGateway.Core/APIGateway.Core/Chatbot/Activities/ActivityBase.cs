using System;
using System.Collections.Generic;
using System.Text.Json;

namespace APIGateway.Core.Chatbot.Activities
{
    public class ActivityBase
    {
        public string Activity { get; set; }
        public DateTime? timestamp { get; set; }

        public string? Text { get; set; }

        public long? sessionId { get; set; }

        public string? Language { get; set; }
        public string? Source { get; set; }
        public string? type { get; set; }

        public Dictionary<string, string>? callParams { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
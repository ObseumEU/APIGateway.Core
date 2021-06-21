﻿using System;
using System.Collections.Generic;
using APIGateway.Core.Chatbot.Activities;
using Newtonsoft.Json;

namespace APIGateway.Core.Chatbot.Elements
{
    public class SendActivity
    {
        [JsonProperty("sessionId")] public string sessionId { get; set; }

        [JsonProperty("type")] public string type { get; set; }

        public string text { get; set; }

        [JsonProperty("timestamp")] public string timestamp { get; set; }

        [JsonProperty("attachments")] public List<Attachment> attachments { get; set; }

        [JsonProperty("activity")] public string activity { get; set; }

        public SuggestedAction suggestedActions { get; set; }

        public Dictionary<string,string> callParams { get; set; }

        public static SendActivity Create(ActivityBase baseActivity)
        {
            return Create(baseActivity.sessionId.Value);
        }

        public static SendActivity CreateTextActivity(ActivityBase baseActivity, string text)
        {
            var msg = Create(baseActivity);
            msg.attachments = new List<Attachment>();
            msg.text = text;
            return msg;
        }

        public static SendActivity Create(long sessionId)
        {
            return new SendActivity
            {
                timestamp = DateTime.Now.AddMilliseconds(2000).ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"),
                type = "message",
                attachments = new List<Attachment>(),
                sessionId = sessionId.ToString()
            };
        }
    }

    public class SuggestedAction
    {
        public List<Action> actions { get; set; }
    }

    public class Action
    {
        public string type { get; set; }
        public string title { get; set; }
        public string value { get; set; }
        public string image { get; set; }
    }

    public class Attachment
    {
        [JsonProperty("contentType")] public string contentType => "application/vnd.microsoft.card.hero";

        [JsonProperty("content")] public Content content { get; set; }
    }

    public class Content
    {
        [JsonProperty("title")] public string title { get; set; }

        [JsonProperty("buttons")] public List<Button> buttons { get; set; }
    }

    public class Button
    {
        [JsonProperty("type")] public string type { get; set; }

        [JsonProperty("title")] public string title { get; set; }

        [JsonProperty("value")] public string value { get; set; }
    }
}
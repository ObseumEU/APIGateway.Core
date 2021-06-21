namespace APIGateway.Core.Chatbot
{
    public class ChatbotOptions
    {
        public int ChatbotID { get; set; }
        public string CallBackURL { get; set; }
        public string ChatbotWebhookSecret { get; set; }
        public string MluviiAPIUsername { get; set; }
        public string MluviiAPIPassword { get; set; }
    }
}
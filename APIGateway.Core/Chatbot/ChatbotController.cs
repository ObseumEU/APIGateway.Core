using System.Threading.Tasks;
using APIGateway.Core.Chatbot.Activities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace APIGateway.Core.Chatbot
{
    [ApiController]
    [Route("chatbot/message")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotBase _chatbot;
        private readonly ILogger<ChatbotController> _log;

        public ChatbotController(IChatbotBase chatbot, ILogger<ChatbotController> log)
        {
            _chatbot = chatbot;
            _log = log;
        }

        // TODO check secret
        [HttpPost]
        public async Task<ActionResult> WebhookPost([FromBody] ActivityBase activity, string secret)
        {
            _log.LogInformation($"Receive webhook from mluvii to chatbot: {JsonConvert.SerializeObject(activity)}");

            if (activity.Activity.ToLower().Trim().Equals("ping"))
                return Ok();

            await _chatbot.OnReceiveActivity(activity);
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            _log.LogInformation("Check health");
            return Ok();
        }
    }
}
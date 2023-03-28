using System;
using System.Threading;
using System.Threading.Tasks;
using APIGateway.Core.Chatbot.Activities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace APIGateway.Core.Chatbot
{
    [ApiController]
    [Route("chatbot/message")]
    public class ChatbotController : ControllerBase
    {
        private readonly ILogger<ChatbotController> _log;
        private readonly IServiceScopeFactory _provide;

        public ChatbotController(ILogger<ChatbotController> log, IServiceScopeFactory _provide)
        {
            _log = log;
            this._provide = _provide;
        }

        // TODO check secret
        [HttpPost]
        public async Task<ActionResult> WebhookPost([FromBody] ActivityBase activity, string secret)
        {
            if (activity.Activity.ToLower().Trim().Equals("ping"))
                return Ok();

            _log.LogInformation($"Receive webhook from mluvii to chatbot: {JsonConvert.SerializeObject(activity)}");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                using (var scope = _provide.CreateScope())
                {
                    var localLog = scope.ServiceProvider.GetService<ILogger<ChatbotController>>();

                    try
                    {
                        var _chatbot = scope.ServiceProvider.GetService<IChatbotBase>();
                        await _chatbot.OnReceiveActivity(new ActivityBase()
                        {
                            Activity = activity.Activity,
                            callParams = activity.callParams,
                            Language = activity.Language,
                            sessionId = activity.sessionId,
                            Source = activity.Source,
                            Text = activity.Text,
                            timestamp = activity.timestamp,
                            type = activity.type
                        });
                    }
                    catch (Exception ex)
                    {
                        localLog.LogError(ex, "Cannot process incoming chatbot message");
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
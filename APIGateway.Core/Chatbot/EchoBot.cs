using System;
using System.Threading.Tasks;
using APIGateway.Core.Cache;
using APIGateway.Core.Chatbot.Activities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace APIGateway.Core.Chatbot
{
    public class EchoTextBot : ChatbotBase
    {
        public EchoTextBot(
            ILogger<EchoTextBot> log,
            IOptions<ChatbotOptions> options,
            MluviiClient.MluviiClient mluviiClient,
            IOptions<ApiGatewayCoreOptions> coreOptions,
            ICacheService cache) :
            base(log, options, mluviiClient, coreOptions, cache)
        {
        }

        public override async Task OnReceiveActivityBase(ActivityBase activity)
        {
            //Cool staff here!
            await SendActivity(new
            {
                activity.sessionId,
                type = "message",
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz"),
                text = activity.Text
            });
        }

        public override async Task OnReceiveText(ActivityBase activity)
        {
            
        }

        public override async Task OnReceiveConversationStarted(ActivityBase activity)
        {
        }

        public override async Task OnReceiveCallParams(ActivityBase activity)
        {
        }
    }
}
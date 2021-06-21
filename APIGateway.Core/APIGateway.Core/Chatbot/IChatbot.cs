using System.Threading.Tasks;
using APIGateway.Core.Chatbot.Activities;
using RestSharp;

namespace APIGateway.Core.Chatbot
{
    public interface IChatbotBase
    {
        Task OnReceiveActivityBase(ActivityBase activity);
        Task OnReceiveActivity(ActivityBase activity);
        Task<IRestResponse> SetupChatbotInMluvii();
        Task SendActivity(object activity);
        Task<bool> IsHealth();
        Task Forward(long sessionId, int userId);
    }
}
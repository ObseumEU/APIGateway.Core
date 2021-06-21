using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace APIGateway.Core.Controllers
{
    public interface IMluviiWebhookController<EventType>
    {
        [HttpPost]
        Task<ActionResult> WebhookPost([FromBody] EventType sessionEnded, string secret);
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace APIGateway.Core.MluviiClient
{
    public class TokenHolder
    {
        private readonly ILogger log;
        private readonly Func<Task<Token>> obtainToken;
        private string token;
        private DateTime? tokenNextRefresh;

        public TokenHolder(Func<Task<Token>> obtainToken, ILogger log)
        {
            this.obtainToken = obtainToken;
            this.log = log;
        }

        public async Task<string> GetToken()
        {
            if (token != null && tokenNextRefresh.HasValue && tokenNextRefresh.Value > DateTime.Now) return token;

            var tokenResponse = await obtainToken();
            tokenNextRefresh = DateTime.Now + TimeSpan.FromSeconds(tokenResponse.ExpiresIn) -
                               TimeSpan.FromMinutes(5);
            token = tokenResponse.AccessToken;
            log.LogInformation(
                $"Successfully retrieved access token from identity server.  Next refresh: {tokenNextRefresh:O}");

            return token;
        }
    }
}
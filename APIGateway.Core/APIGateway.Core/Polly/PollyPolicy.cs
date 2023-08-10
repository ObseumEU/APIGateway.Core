using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;

namespace APIGateway.Core.Polly
{
    public interface IPollyPolicy
    {
        AsyncRetryPolicy AutoRetryExponencial(int count = 13, string logRetryMessage = null);
    }
    
    public class PollyPolicy : IPollyPolicy
    {
        private readonly ILogger<PollyPolicy> _log;
        public PollyPolicy(ILogger<PollyPolicy> log)
        {
            _log = log;
        }
       

        public AsyncRetryPolicy AutoRetryExponencial(int count = 13, string logRetryMessage = null)
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(count, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, retryCount, context) => _log.LogError($"try: {retryCount}, Message: {logRetryMessage} Exception: {exception.Message} InnerException:{exception.InnerException?.Message}"));
        }
    }
}

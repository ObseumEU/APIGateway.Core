using Microsoft.AspNetCore.SignalR;
using Polly;
using Polly.Retry;
using System;

namespace APIGateway.Core.Polly
{
    public static class PollyPolicy
    {
        public static AsyncRetryPolicy AutoRetryExponencial(int count = 13)
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(count, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, retryCount, context) => Console.WriteLine($"try: {retryCount}, Exception: {exception.Message}"));
        }
    }
}

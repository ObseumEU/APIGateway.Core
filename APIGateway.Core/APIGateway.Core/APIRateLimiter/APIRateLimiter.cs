using System.Collections.Generic;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace APIGateway.Core.APIRateLimiter
{
    public static class APIRateLimiter
    {
        public static IServiceCollection AddAPILimit(IServiceCollection services, List<RateLimitRule> rules = null)
        {
            if (rules == null)
            {
                rules = new List<RateLimitRule>();
                rules.Add(new RateLimitRule
                {
                    Endpoint = "*",
                    Period = "1m",
                    Limit = 500
                });
            }

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.Configure<IpRateLimitOptions>(options => { options.GeneralRules = rules; });

            services.AddOptions();

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            return services;
        }
    }
}
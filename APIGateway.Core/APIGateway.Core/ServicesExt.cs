using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using APIGateway.Core.Auth.BasicAuth;
using APIGateway.Core.Cache;
using APIGateway.Core.Chatbot;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace APIGateway.Core
{
    public static class ServicesExt
    {
        public static IServiceCollection AddApiGatewayCore(this IServiceCollection collection,
            IConfiguration configuration, string mluviiSesttingsName = "APIGatewayCore")
        {
            collection.Configure<ApiGatewayCoreOptions>(configuration.GetSection(mluviiSesttingsName));
            return collection;
        }

        /// <summary>
        ///     Add quartz jobs support, more info how implement
        ///     https://andrewlock.net/creating-a-quartz-net-hosted-service-with-asp-net-core/
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IServiceCollection AddQuartzJobs(this IServiceCollection collection)
        {
            // Add Quartz services
            collection.AddSingleton<IJobFactory, SingletonJobFactory>();
            collection.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            collection.AddHostedService<QuartzHostedService>();
            return collection;
        }

        /// <summary>
        ///     Add quartz job, more info how implement
        ///     https://andrewlock.net/creating-a-quartz-net-hosted-service-with-asp-net-core/
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonJob<T>(this IServiceCollection collection, string cronExpression)
            where T : class, IJob
        {
            if (collection.All(x => x.ServiceType != typeof(IJobFactory)))
                throw new Exception(
                    "If you want use quartz jobs, you must add Quartz in ServiceCollection. eg. services.AddJobs();");

            collection.AddSingleton<T>();
            collection.AddSingleton(new JobSchedule(
                typeof(T),
                cronExpression));
            return collection;
        }

        /// <summary>
        ///     Api documentation
        ///     https://mluvii.app.com/api, https://dots.mluvii.com
        /// </summary>
        public static IServiceCollection AddMluviiClient(this IServiceCollection collection,
            IConfiguration configuration, string mluviiSesttingsName = "MluviiApp")
        {
            collection.Configure<MluviiCredentialOptions>(configuration.GetSection(mluviiSesttingsName));
            collection.AddSingleton<ICacheService, InMemoryCache>();
            collection.AddSingleton<ITokenEndpoint, TokenEndpoint>();
            collection.AddSingleton<MluviiClient.MluviiClient>();

            return collection;
        }

        public static IServiceCollection AddHaproxyStats(this IServiceCollection collection,
            IConfiguration configuration, string sesttingsName = "HAProxy")
        {
            collection.Configure<HaproxyClientOption>(configuration.GetSection(sesttingsName));
            collection.AddSingleton<IHaproxyClient, HaproxyClient>();
            return collection;
        }

        public static IServiceCollection AddBasicAuth(this IServiceCollection collection, IConfiguration configuration,
            string sesttingsName = "AuthUsers")
        {
            collection.Configure<BasicAuthConfig>(configuration.GetSection(sesttingsName));
            collection.AddSingleton<IUserService, UserServiceFromConfig>();
            return collection;
        }

        public static IServiceCollection AddCertAuth(this IServiceCollection collection, IConfiguration configuration,
            string sesttingsName = "CertAuth")
        {
            //collection.Configure<BasicAuthConfig>(configuration.GetSection(sesttingsName));
            //collection.AddSingleton<IUserService, UserServiceFromConfig>();

            collection.AddAuthentication(
                    CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate();

            return collection;
        }

        /// <param name="mvcBuilder">You can get mvcBuilder from services.AddControllers();</param>
        /// <returns></returns>
        public static IServiceCollection AddChatbot<ChatBot>(this IServiceCollection collection,
            IConfiguration configuration,
            IMvcBuilder mvcBuilder,
            string sesttingsName = "Chatbot") where ChatBot : class, IChatbotBase
        {
            collection.Configure<ChatbotOptions>(configuration.GetSection(sesttingsName));
            mvcBuilder.AddApplicationPart(Assembly.GetAssembly(typeof(ChatbotController)));
            collection.AddScoped<IChatbotBase, ChatBot>();
            return collection;
        }

        public static IServiceCollection AddAPILimit(this IServiceCollection collection,
            List<RateLimitRule> rules = null)
        {
            return APIRateLimiter.APIRateLimiter.AddAPILimit(collection, rules);
        }
    }
}
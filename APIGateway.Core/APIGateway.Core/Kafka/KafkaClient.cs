using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace APIGateway.Core.Kafka
{
    public static class KafkaExt
    {
        public static void AddKafka(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped<IMessageBroker, KafkaClient>();
        }
    }

    public class KafkaOption
    {
        public string Host { get; set; }
    }

    public class KafkaProduceOption
    {
        public string Topic { get; set; }
    }

    public interface IMessageBroker
    {
        Task ProduceMessage(string topic, object message);
    }

    public class KafkaClient : IMessageBroker
    {
        private readonly IOptionsMonitor<KafkaOption> _options;
        private readonly ILogger<KafkaClient> _log;
        private ProducerConfig _config;

        public KafkaClient(IOptionsMonitor<KafkaOption> options, ILogger<KafkaClient> log)
        {
            _options = options;
            _log = log;
            _config = new ProducerConfig
            {
                BootstrapServers = options.CurrentValue.Host,
                ClientId = Dns.GetHostName()
            };
        }

        public async Task ProduceMessage(string topic, object message)
        {
            using (var producer = new ProducerBuilder
                       <Null, string>(_config).Build())
            {
                var serialized = JsonConvert.SerializeObject(message);
                var result = await producer.ProduceAsync
                (topic, new Message<Null, string>
                {
                    Value = serialized
                });

                _log.LogInformation($"Delivery Timestamp:{result.Timestamp.UtcDateTime} ");
            }
        }
    }
}

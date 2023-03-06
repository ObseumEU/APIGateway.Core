using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using APIGateway.Core.Cache;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace APIGateway.Core.MluviiClient
{
    public class JsonSerializer : ISerializer, IDeserializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        public JsonSerializer()
        {
            ContentType = "application/json";
            _serializer = new Newtonsoft.Json.JsonSerializer
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            };

            _serializer.Converters.Add(new StringEnumConverter());
        }

        public JsonSerializer(Newtonsoft.Json.JsonSerializer serializer)
        {
            ContentType = "application/json";
            _serializer = serializer;
        }

        public string DateFormat { get; set; }
        public string RootElement { get; set; }
        public string Namespace { get; set; }

        public T Deserialize<T>(IRestResponse response)
        {
            var content = response.Content;
            return Deserialize<T>(content);
        }

        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    jsonTextWriter.Formatting = Formatting.None;
                    jsonTextWriter.QuoteChar = '"';

                    _serializer.Serialize(jsonTextWriter, obj);

                    var result = stringWriter.ToString();
                    return result;
                }
            }
        }

        public string ContentType { get; set; }

        public T Deserialize<T>(string content)
        {
            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }
    }

    public class BaseClient : RestClient
    {
        public readonly ILogger _log;
        protected ICacheService _cache;
        public bool AutoRetry { get; set; }
        public int MaxRetries { get; set; }

        public BaseClient(ILogger log, ICacheService cache,
            string baseUrl, bool autoRetry = false, int maxRetries = 2)
        {
            _log = log;
            _cache = cache;
            var serializer = new JsonSerializer();
#pragma warning disable 618
            AddHandler("application/json", serializer);
            AddHandler("text/json", serializer);
            AddHandler("text/x-json", serializer);
#pragma warning restore 618
            BaseUrl = new Uri(baseUrl);
        }

        public async Task<(T Value, IRestResponse Response)> ExecuteAsync<T>(IRestRequest request,
            bool logVerbose = false)
        {
            if (AutoRetry)
            {
                IRestResponse<T> response = null;
                for (int i = 0; i < MaxRetries; i++)
                {
                    response = await base.ExecuteAsync<T>(request);
                    if (logVerbose)
                        _log.LogInformation(
                            $"RequestUrl: {BuildUri(request)} RequestBody: {request.Body?.Value?.ToString()} RequestBody: {request?.Parameters?.FirstOrDefault()?.Value} Response Content: {response.Content} StatusCode: {response.StatusCode}");

                    if (response.IsSuccessful)
                    {
                        return (response.Data, response);
                    }
                }

                LogError(BaseUrl, request, response);
                return (response.Data, response);
            }
            else
            {
                base.Timeout = 120000;
                var response = await base.ExecuteAsync<T>(request);
                if (logVerbose)
                    _log.LogInformation(
                        $"RequestUrl: {BuildUri(request)} RequestBody: {request.Body?.Value?.ToString()} RequestBody: {request?.Parameters?.FirstOrDefault()?.Value} Response Content: {response.Content} StatusCode: {response.StatusCode}");

                if (!(response.IsSuccessful))
                {
                    LogError(BaseUrl, request, response);
                }
                return (response.Data, response);
            }
        }

        public async Task<(string Value, IRestResponse Response)> ExecuteAsync(IRestRequest request,
            bool logVerbose = false)
        {
            if (AutoRetry)
            {
                IRestResponse response = null;
                for (int i = 0; i < MaxRetries; i++)
                {
                    response = await base.ExecuteAsync(request);
                    if (logVerbose)
                        _log.LogInformation(
                            $"RequestUrl: {BuildUri(request)} RequestBody: {request.Body?.Value?.ToString()} RequestBody: {request?.Parameters?.FirstOrDefault()?.Value} Response Content: {response.Content} StatusCode: {response.StatusCode}");

                    if (response.IsSuccessful)
                    {
                        return (response.Content, response);
                    }
                }

                LogError(BaseUrl, request, response);
                return (response.Content, response);
            }
            else
            {
                base.Timeout = 120000;
                var response = await base.ExecuteAsync(request);
                if (logVerbose)
                    _log.LogInformation(
                        $"RequestUrl: {BuildUri(request)} RequestBody: {request.Body?.Value?.ToString()} RequestBody: {request?.Parameters?.FirstOrDefault()?.Value} Response Content: {response.Content} StatusCode: {response.StatusCode}");

                if (!(response.IsSuccessful))
                    LogError(BaseUrl, request, response);

                return (response.Content, response);

            }
        }

            public async Task<T> GetFromCacheAsync<T>(IRestRequest request, string cacheKey, int minutes = 5,
            bool logVerbose = false)
            where T : class, new()
        {
            var item = _cache.Get<T>(cacheKey);
            if (item == null)
            {
                var response = await base.ExecuteAsync<T>(request);

                if (logVerbose)
                    _log.LogInformation(
                        $"RequestUrl: {BuildUri(request)} RequestBody: {request.Body} Response Content: {response.Content} StatusCode: {response.StatusCode}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _cache.Set(cacheKey, response.Data, minutes);
                    item = response.Data;
                }
                else
                {
                    LogError(BaseUrl, request, response);
                    throw new Exception(
                        $"RequestUrl: {BuildUri(request)} RequestBody: {request.Body} Response Content: {response.Content} StatusCode: {response.StatusCode}");
                }
            }

            return item;
        }

        private void LogError(Uri BaseUrl,
            IRestRequest request,
            IRestResponse response)
        {
            //Get the values of the parameters passed to the API
            var parameters = string.Join(", ",
                request.Parameters.Select(x => x.Name.ToString() + "=" + (x.Value == null ? "NULL" : x.Value))
                    .ToArray());

            //Set up the information message with the URL, 
            //the status code, and the parameters.
            var info = "Request to " + BaseUrl.AbsoluteUri
                                     + request.Resource + " failed with status code "
                                     + response.StatusCode + ", parameters: "
                                     + parameters + ", and content: " + response.Content;

            //Acquire the actual exception
            Exception ex;
            if (response != null && response.ErrorException != null)
            {
                ex = response.ErrorException;
            }
            else
            {
                ex = new Exception(info);
                info = string.Empty;
            }

            //Log the exception and info message
            _log.LogError(ex, info);
        }
    }
}
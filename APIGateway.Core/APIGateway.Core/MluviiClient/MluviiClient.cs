using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using APIGateway.Core.Cache;
using APIGateway.Core.MluviiClient.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mluvii.ApiModels.Sessions;
using mluvii.ApiModels.Users;
using mluvii.ApiModels.Webhooks;
using RestSharp;

namespace APIGateway.Core.MluviiClient
{
    public class SessionNotFoundException : Exception
    {
    }

    public class MluviiClient : BaseClient, IMluviiUserClient
    {
        private const string mluviiPublicApiScope = "mluviiPublicApi";
        private const string Version = "v1";
        private readonly IOptions<ApiGatewayCoreOptions> _coreOptions;
        private readonly MluviiCredentialOptions _credentials;
        private readonly ILogger<BaseClient> _log;
        private readonly TokenHolder tokenHolder;

        public MluviiClient(
            ILogger<MluviiClient> log,
            IOptions<MluviiCredentialOptions> credentials,
            ICacheService cache,
            ITokenEndpoint tokenEndpoint,
            IOptions<ApiGatewayCoreOptions> coreOptions)
            : base(log, cache, credentials.Value.BaseApiEndpoint)
        {
            _log = log;
            _coreOptions = coreOptions;
            _credentials = credentials.Value;
            tokenHolder = new TokenHolder(async () => await tokenEndpoint.RequestAccessToken(mluviiPublicApiScope),
                log);
        }

        public async Task<(List<User> value, IRestResponse response)> GetAllUsers()
        {
            _log.LogInformation("GET all users");
            var request = await CreateRequest($"api/{Version}/users", Method.GET);
            return await ExecuteAsync<List<User>>(request, true);
        }

        public async Task<IRestResponse> AddUsers(int companyId, User user)
        {
            var request = await CreateRequest($"api/{Version}/users?companyId={companyId}", Method.POST);
            request.AddJsonBody(user);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<IRestResponse> AddUserToDepartment(int departmentId, int userId)
        {
            var request = await CreateRequest($"api/{Version}/users/{userId}/departments", Method.PUT);
            request.AddJsonBody(new
            {
                departments = new List<int> {departmentId}
            });
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<IRestResponse> DisableUsers(List<User> users)
        {
            var request = await CreateRequest($"api/{Version}/users", Method.PUT);
            request.AddJsonBody(users);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<IRestResponse> EnableUsers(int userId)
        {
            var request = await CreateRequest($"api/{Version}/users/{userId}/enabled", Method.PUT);
            request.AddJsonBody(new
            {
                isEnabled = true
            });
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public string GetSessionUrl(long sessionId)
        {
            return _coreOptions.Value.Domain +
                   $"/app/{_credentials.Company}/{_credentials.Tenant}/sessions/{sessionId}";
        }

        public async Task<IRestResponse> SetChatbotCallbackURL(int chatbotID, string callbackUrl)
        {
            var request = await CreateRequest($"api/v1/Chatbot/{chatbotID}?callbackUrl={callbackUrl}", Method.PUT);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<IRestResponse> GetAvaliableOperators(int chatbotID, string callbackUrl)
        {
            var request = await CreateRequest($"api/v1/Chatbot/{chatbotID}?callbackUrl={callbackUrl}", Method.PUT);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<IRestResponse> AddTagToSession(int tagID, long sessionID)
        {
            var request = await CreateRequest($"/api/{Version}/Sessions/{sessionID}/tags/{tagID}", Method.PUT);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<(SessionModel value, IRestResponse response)> GetSession(long sessionId)
        {
            var request = await CreateRequest($"/api/{Version}/Sessions/{sessionId}", Method.GET);
            return await ExecuteAsync<SessionModel>(request, true);
        }

        public async Task<(IDictionary<string, string> value, IRestResponse response)> GetCallParams(long sessionId)
        {
            var session = await GetSession(sessionId);
            if (!session.response.IsSuccessful)
                return (null, session.response);

            var callParam = session.value?.Guest?.CallParams;
            return (callParam, session.response);
        }

        public async Task<(string value, IRestResponse response)> GetCallParam(long sessionId, string callParamKey)
        {
            var callparams = await GetCallParams(sessionId);
            if (!callparams.response.IsSuccessful || !callparams.value.ContainsKey(callParamKey))
                return (null, callparams.response);

            var value = callparams.value[callParamKey];
            return (value, callparams.response);
        }

        public async Task<(List<SessionModel> value, IRestResponse response)> GetSessions(DateTime from, DateTime to,
            string channel = "", string source = "", bool verbose = false)
        {
            var request =
                await CreateRequest(
                    $"/api/{Version}/Sessions" +
                    $"?Created.Min={from.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}" +
                    $"&Created.Max={to.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}" +
                    (!string.IsNullOrEmpty(channel) ? $"&Channel={channel}" : "") +
                    (!string.IsNullOrEmpty(source) ? $"&Source={source}" : ""),
                    Method.GET);
            return await ExecuteAsync<List<SessionModel>>(request, verbose);
        }

        public async Task<(List<OperatorStateModel> value, IRestResponse response)> OperatorStates(bool verbose = false)
        {
            var request =
                await CreateRequest(
                    $"/api/{Version}/Users/operatorStates",
                    Method.GET);
            return await ExecuteAsync<List<OperatorStateModel>>(request, verbose);
        }

        public async Task<(List<WebhookModel> value, IRestResponse response)> GetWebhooks()
        {
            var request = await CreateRequest($"api/{Version}/webhooks", Method.GET);
            return await ExecuteAsync<List<WebhookModel>>(request, true);
        }

        public async Task DownloadRecording(SessionModel.Recording recording, Action<Stream> responseWriter)
        {
            var request = await CreateRequest("", Method.GET);
            request.ResponseWriter = responseWriter;
            var client = new RestClient(recording.DownloadUrl);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception(
                    $"Unable to download file {recording.DownloadUrl} code:{response.StatusCode} response:{response.Content}");
        }

        public async Task<IRestResponse> AddWebhook(List<WebhookEventType> webhookTypes)
        {
            var webhookStrs = webhookTypes.Select(w => nameof(w)).ToList();
            return await AddWebhook(_coreOptions.Value.Domain + "/" + _credentials.WebhookEndpoint, webhookStrs);
        }

        /// Webhook is called on endpoint from MluviiCredentialOptions
        public async Task<IRestResponse> AddWebhook(List<string> webhookTypes)
        {
            return await AddWebhook(_coreOptions.Value.Domain + "/" + _credentials.WebhookEndpoint, webhookTypes);
        }

        public async Task<IRestResponse> UpdateWebhook(int id, List<string> webhookTypes)
        {
            return await UpdateWebhook(id, _coreOptions.Value.Domain + "/" + _credentials.WebhookEndpoint,
                webhookTypes);
        }

        /// Webhook is called on endpoint from MluviiCredentialOptions
        /// <param name="endpointResource">Example secret=d8s7f6p4bsdf087332kdc</param>
        public async Task<IRestResponse> UpdateWebhook(int id, string callbackUrl, List<string> webhookTypes)
        {
            callbackUrl = AddSecretToWebhook(callbackUrl);

            var request = await CreateRequest($"/api/{Version}/webhooks/{id}", Method.PUT);
            request.AddJsonBody(new
            {
                eventTypes = webhookTypes, callbackUrl
            });
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        private string AddSecretToWebhook(string callbackUrl)
        {
            if (!string.IsNullOrEmpty(_credentials.WebhookSecret))
            {
                var longurl = callbackUrl;
                var uriBuilder = new UriBuilder(longurl);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["secret"] = _credentials.WebhookSecret;
                ;
                uriBuilder.Query = query.ToString();
                callbackUrl = uriBuilder.Uri.ToString();
            }

            return callbackUrl;
        }

        public async Task<IRestResponse> AddWebhook(string callBackUrl, List<string> webhookTypes)
        {
            callBackUrl = AddSecretToWebhook(callBackUrl);

            var request = await CreateRequest($"/api/{Version}/webhooks", Method.POST);
            request.AddJsonBody(new
            {
                eventTypes = webhookTypes,
                callbackUrl = callBackUrl
            });
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        private async Task<RestRequest> CreateRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method); //TBD
            var token = await tokenHolder.GetToken();
            request.AddHeader("Authorization", $"bearer {token}");
            return request;
        }

        public async Task<(CallParamsModel value, IRestResponse response)> GetCustomData(long sessionID)
        {
            var request = await CreateRequest($"/api/{Version}/Sessions/{sessionID}/callparams", Method.GET);
            var result = await ExecuteAsync<CallParamsModel>(request, true);
            return result;
        }

        public async Task<IRestResponse> RemoveTagFromSession(int tagID, long sessionID)
        {
            var request = await CreateRequest($"/api/{Version}/Sessions/{sessionID}/tags/{tagID}", Method.DELETE);
            return (await ExecuteAsync<object>(request, true)).Response;
        }


        public async Task<IRestResponse> SendChatbotActivity(int chatbotID, object activity)
        {
            var request = await CreateRequest($"/api/{Version}/Chatbot/{chatbotID}/activity", Method.POST);
            request.AddJsonBody(activity);
            return (await ExecuteAsync<object>(request, true)).Response;
        }
    }

    public interface IMluviiUserClient
    {
        Task<(List<User> value, IRestResponse response)> GetAllUsers();
        Task<IRestResponse> AddUsers(int companyId, User user);
        Task<IRestResponse> AddUserToDepartment(int departmentId, int userId);
        Task<IRestResponse> DisableUsers(List<User> users);
        Task<IRestResponse> EnableUsers(int userId);
    }
}
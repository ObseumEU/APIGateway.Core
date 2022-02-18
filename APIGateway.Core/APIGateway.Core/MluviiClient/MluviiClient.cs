using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using APIGateway.Core.Cache;
using APIGateway.Core.MluviiClient.Models;
using Microsoft.Extensions.Localization.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mluvii.ApiModels.Campaigns;
using mluvii.ApiModels.Emails;
using mluvii.ApiModels.Sessions;
using mluvii.ApiModels.Users;
using mluvii.ApiModels.Webhooks;
using RestSharp;

namespace APIGateway.Core.MluviiClient
{
    public interface IMluviiClient
    {
        Task<IRestResponse> AddContactToCampaign(int campaignId, int contactId);
        Task<IRestResponse> AddContactToCampaign(int campaignId, List<int> contactIds);
        Task<(List<CampaignIdentity> identities, IRestResponse response)> GetCampaignIndetities(long campaignId);
        Task<(List<Contact> contactIds, IRestResponse response)> GetContacts(int departmentId, int limit = 1000000);
        Task<(List<Contact> contactIds, IRestResponse response)> GetContacts(int departmentId, string phoneFilter, int limit = 1000000);
        Task<(List<Contact> contactIds, IRestResponse response)> GetContacts(int departmentId, List<string> phoneFilter, int limit = 1000000);
        Task<(int? contactId, IRestResponse response)> CreateContact(int departmentId, Dictionary<string, string> contact);
        Task<(List<int> contactIds, IRestResponse response)> CreateContact(int departmentId, List<Dictionary<string, string>> contacts);
        Task<(List<Contact> contact, IRestResponse response)> GetContact(long contactId, long departmentId);
        Task<(List<User> value, IRestResponse response)> GetAllUsers();
        Task<IRestResponse> AddUsers(int companyId, User user);
        Task<IRestResponse> AddTag(int departmentId, mluvii.ApiModels.Tags.CreateTagModel tag);
        Task<(List<mluvii.ApiModels.Tags.TagModel> value, IRestResponse response)> GetAllTags();
        Task<IRestResponse> AddUserToDepartment(int departmentId, int userId);
        Task<IRestResponse> DisableUsers(List<User> users);
        Task<IRestResponse> EnableUsers(int userId);
        string GetSessionUrl(long sessionId);
        Task<IRestResponse> SetChatbotCallbackUrl(int chatbotId, string callbackUrl);
        Task<IRestResponse> GetAvaliableOperators(int chatbotId, string callbackUrl);
        Task<IRestResponse> AddTagToSession(int tagId, long sessionId);
        Task<(SessionModel value, IRestResponse response)> GetSession(long sessionId);
        Task<(string email, IRestResponse response)> GetEmailFromSession(long sessionId, int? tenantId = null);
        Task<(IDictionary<string, string> value, IRestResponse response)> GetCallParams(long sessionId);
        Task<IRestResponse> SetCallParam(long sessionId, string key, string value);
        Task<(string value, IRestResponse response)> GetCallParam(long sessionId, string callParamKey);

        Task<(List<SessionModel> value, IRestResponse response)> GetSessions(DateTime? startedFrom = null,
            DateTime? startedTo = null, DateTime? endedFrom = null, DateTime? endedTo = null, string channel = "",
            string source = "", bool verbose = false, int limit = 100000, int? offset = null, string[] status = null);
        Task<(EmailThreadModel value, IRestResponse response)> GetEmailThread(long emailThread);
        Task<(List<OperatorStateModel> value, IRestResponse response)> OperatorStates(bool verbose = false);
        Task<(List<WebhookModel> value, IRestResponse response)> GetWebhooks();
        Task DownloadRecording(SessionModel.Recording recording, Action<Stream> responseWriter);
        Task DownloadRecording(string recordingUrl, Action<Stream> responseWriter);
        Task<IRestResponse> AddWebhook(List<WebhookEventType> webhookTypes);

        /// Webhook is called on endpoint from MluviiCredentialOptions
        Task<IRestResponse> AddWebhook(List<string> webhookTypes);

        Task<IRestResponse> UpdateWebhook(int id, List<string> webhookTypes);

        /// Webhook is called on endpoint from MluviiCredentialOptions
        Task<IRestResponse> UpdateWebhook(int id, string callbackUrl, List<string> webhookTypes);
        Task<IRestResponse> AddWebhook(string callBackUrl, List<string> webhookTypes);
        Task<IRestResponse> AnonymizeSession(long sessionId, bool verbose = false);
        Task<(CallParamsModel value, IRestResponse response)> GetCustomData(long sessionId);
        Task<IRestResponse> RemoveTagFromSession(int tagId, long sessionId);
        Task<IRestResponse> SendChatbotActivity(int chatbotId, object activity);

        Task<(T Value, IRestResponse Response)> ExecuteAsync<T>(IRestRequest request,
            bool logVerbose = false);

        Task<T> GetFromCacheAsync<T>(IRestRequest request, string cacheKey, int minutes = 5,
            bool logVerbose = false)
            where T : class, new();

        Task<(EmailThreadParamsModel value, IRestResponse response)> GetEmailThreadParam(long threadId);
        Task<IRestResponse> AddTagToEmailThread(long threadId, string tagName);
        Task<IRestResponse> RemoveTagToEmailThread(long threadId, string tagName);

        Task GetSessionsPaged(Func<(List<SessionModel> value, IRestResponse response), Task> pageAction, DateTime? startedFrom = null,
           DateTime? startedTo = null, DateTime? endedFrom = null, DateTime? endedTo = null, string channel = "",
           string source = "", bool verbose = false, int limit = 500, string[] status = null, int delayMiliseconds = 200);
    }

    public class MluviiClient : BaseClient, IMluviiUserClient, IMluviiClient
    {
        private const string MluviiPublicApiScope = "mluviiPublicApi";
        private const string Version = "v1";
        private readonly IOptions<ApiGatewayCoreOptions> _coreOptions;
        public readonly MluviiCredentialOptions _credentials;
        private readonly ILogger<BaseClient> _log;
        private readonly TokenHolder _tokenHolder;

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
            _tokenHolder = new TokenHolder(async () => await tokenEndpoint.RequestAccessToken(MluviiPublicApiScope),
                log);
        }

        public async Task<IRestResponse> AddContactToCampaign(int campaignId, int contactId)
        {
            return await AddContactToCampaign(campaignId, new List<int>() {contactId});
        }

        public async Task<IRestResponse> AddContactToCampaign(int campaignId, List<int> contactIds)
        {
            var request = await CreateRequest($"api/{Version}/Campaigns/{campaignId}/contacts", Method.POST);
            request.AddJsonBody(contactIds);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<(List<CampaignIdentity> identities, IRestResponse response)> GetCampaignIndetities(long campaignId)
        {
            var request = await CreateRequest($"api/{Version}/Campaigns/{campaignId}/identities", Method.GET);
            return await ExecuteAsync<List<CampaignIdentity>>(request, true);
        }

        public async Task<(List<Contact> contactIds, IRestResponse response)> GetContacts(int departmentId, int limit = 1000000)
        {
            var request = await CreateRequest($"api/{Version}/Contacts/departments/{departmentId}", Method.GET);
            return await ExecuteAsync<List<Contact>>(request, true);
        }

        public async Task<(List<Contact> contactIds, IRestResponse response)> GetContacts(int departmentId, string phoneFilter, int limit = 1000000)
        {
            return await GetContacts(departmentId, new List<string>() {phoneFilter}, limit);
        }

        public async Task<(List<Contact> contactIds, IRestResponse response)> GetContacts(int departmentId, List<string> phoneFilter, int limit = 1000000)
        {
            var request = await CreateRequest($"api/{Version}/Contacts/departments/{departmentId}", Method.GET);
            var contacts = await ExecuteAsync<List<Contact>>(request, true);

            contacts.Value = contacts.Value
                .Where(c => c.oo1_guest_phone != null)
                .Where(c => c.oo1_guest_phone.Any(p => 
                    phoneFilter.Any(
                        pf => p == pf))).ToList();

            return contacts;
        }

        public async Task<(int? contactId, IRestResponse response)> CreateContact(int departmentId, Dictionary<string, string> contact)
        {
            var res = await CreateContact(departmentId, new List<Dictionary<string, string>>() { contact });

            var value = res.contactIds?.FirstOrDefault();
            var response = res.response;
            return (value, response);
        }

        public async Task<(List<int> contactIds, IRestResponse response)> CreateContact(int departmentId, List<Dictionary<string, string>> contacts)
        {
            var request = await CreateRequest($"api/{Version}/Contacts/departments/{departmentId}", Method.POST);
            request.AddJsonBody(contacts);
            return await ExecuteAsync<List<int>>(request, true);
        }

        public async Task<(List<Contact> contact, IRestResponse response)> GetContact(long contactId, long departmentId)
        {
            var request = await CreateRequest($"api/{Version}/Contacts/{contactId}/departments/{departmentId}", Method.GET);
            return await ExecuteAsync<List<Contact>>(request, false);
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

        public async Task<IRestResponse> AddTag(int departmentId, mluvii.ApiModels.Tags.CreateTagModel tag)
        {
            var request = await CreateRequest($"api/{Version}/tags/departments/{departmentId}", Method.POST);
            request.AddJsonBody(tag);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<(List<mluvii.ApiModels.Tags.TagModel> value, IRestResponse response)> GetAllTags()
        {
            var request = await CreateRequest($"api/{Version}/Tags", Method.GET);
            return (await ExecuteAsync<List<mluvii.ApiModels.Tags.TagModel>>(request, true));
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

        public async Task<IRestResponse> SetChatbotCallbackUrl(int chatbotId, string callbackUrl)
        {
            var request = await CreateRequest($"api/{Version}/Chatbot/{chatbotId}?callbackUrl={callbackUrl}",
                Method.PUT);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<IRestResponse> GetAvaliableOperators(int chatbotId, string callbackUrl)
        {
            var request = await CreateRequest($"api/{Version}/Chatbot/{chatbotId}?callbackUrl={callbackUrl}",
                Method.PUT);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<IRestResponse> AddTagToSession(int tagId, long sessionId)
        {
            var request = await CreateRequest($"/api/{Version}/Sessions/{sessionId}/tags/{tagId}", Method.PUT);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<(SessionModel value, IRestResponse response)> GetSession(long sessionId)
        {
            var request = await CreateRequest($"/api/{Version}/Sessions/{sessionId}", Method.GET);
            return await ExecuteAsync<SessionModel>(request, true);
        }

        public async Task<(EmailThreadParamsModel value, IRestResponse response)> GetEmailThreadParam(long threadId)
        {
            var request = await CreateRequest($"/api/{Version}/EmailThreads/{threadId}/params", Method.GET);
            return await ExecuteAsync<EmailThreadParamsModel>(request, true);
        }

        public async Task<IRestResponse> AddTagToEmailThread(long threadId, string tagName)
        {
            var encodedName = UrlEncoder.Default.Encode(tagName);
            var request = await CreateRequest($"api/{Version}/EmailThreads/{threadId}/tags/{encodedName}", Method.PUT);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<IRestResponse> RemoveTagToEmailThread(long threadId, string tagName)
        {
            var request = await CreateRequest($"api/{Version}/EmailThreads/{threadId}/tags/{tagName}", Method.DELETE);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<(string email, IRestResponse response)> GetEmailFromSession(long sessionId, int? tenantId = null)
        {
            if (tenantId == null)
            {
                tenantId = _credentials.Tenant;
            }

            var sessions = await GetSession(sessionId);
            var identityID = sessions.value?.Guest?.Identity;

            if (string.IsNullOrEmpty(identityID))
                return (null, sessions.response);

            var identity = await GetContact(long.Parse(identityID), _credentials.Tenant);
            var email = identity.contact?.First()?.oo1_guest_email?.FirstOrDefault();
            return (email, sessions.response);
        }

        public async Task<(IDictionary<string, string> value, IRestResponse response)> GetCallParams(long sessionId)
        {
            var session = await GetSession(sessionId);
            if (!session.response.IsSuccessful)
                return (null, session.response);

            var callParam = session.value?.Guest?.CallParams;
            return (callParam, session.response);
        }

        public async Task<IRestResponse> SetCallParam(long sessionId, string key, string value)
        {
            var request = await CreateRequest($"api/{Version}/Sessions/{sessionId}/callparams", Method.PUT);
            var body = new UpdateCallParamsModel {CallParams = new Dictionary<string, string> {[key] = value}};
            request.AddJsonBody(body);

            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<(string value, IRestResponse response)> GetCallParam(long sessionId, string callParamKey)
        {
            var callparams = await GetCallParams(sessionId);
            if (!callparams.response.IsSuccessful || !callparams.value.ContainsKey(callParamKey))
                return (null, callparams.response);

            var value = callparams.value[callParamKey];
            return (value, callparams.response);
        }

        public async Task<(List<SessionModel> value, IRestResponse response)> GetSessions(DateTime? startedFrom = null,
            DateTime? startedTo = null, DateTime? endedFrom = null, DateTime? endedTo = null, string channel = "",
            string source = "", bool verbose = false, int limit = 100000, int? offset = null, string[] status = null)
        {
            var url = $"/api/{Version}/Sessions";

            var urlWithArguments = AddArgumentsToUrl(url, GetSessionArguments(startedFrom, startedTo, endedFrom, endedTo, channel, source, limit, offset, status));

            var request =
                await CreateRequest(urlWithArguments, Method.GET);

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
            await DownloadRecording(recording.DownloadUrl, responseWriter);
        }

        public async Task DownloadRecording(string recordingUrl, Action<Stream> responseWriter)
        {
            var request = await CreateRequest("", Method.GET);
            request.ResponseWriter = responseWriter;

            var client = new RestClient(recordingUrl);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception(
                    $"Unable to download file {recordingUrl} code:{response.StatusCode} response:{response.Content}");
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
            if(string.IsNullOrEmpty(callbackUrl))
                throw new Exception("Callback url cannot be empty.");

            if (!string.IsNullOrEmpty(_credentials.WebhookSecret))
            {
                var longurl = callbackUrl;
                var uriBuilder = new UriBuilder(longurl);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["secret"] = _credentials.WebhookSecret;
                uriBuilder.Query = query.ToString() ?? string.Empty;
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

        public async Task<IRestResponse> AnonymizeSession(long sessionId, bool verbose = false)
        {
            var request = await CreateRequest($"/api/{Version}/Sessions/{sessionId}/anonymize", Method.POST);
            return (await ExecuteAsync<object>(request, verbose)).Response;
        }

        private async Task<RestRequest> CreateRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method); //TBD
            var token = await _tokenHolder.GetToken();
            request.AddHeader("Authorization", $"bearer {token}");
            return request;
        }

        public async Task<(CallParamsModel value, IRestResponse response)> GetCustomData(long sessionId)
        {
            var request = await CreateRequest($"/api/{Version}/Sessions/{sessionId}/callparams", Method.GET);
            var result = await ExecuteAsync<CallParamsModel>(request, true);
            return result;
        }

        public async Task<IRestResponse> RemoveTagFromSession(int tagId, long sessionId)
        {
            var request = await CreateRequest($"/api/{Version}/Sessions/{sessionId}/tags/{tagId}", Method.DELETE);
            return (await ExecuteAsync<object>(request, true)).Response;
        }

        public async Task<IRestResponse> SendChatbotActivity(int chatbotId, object activity)
        {
            var request = await CreateRequest($"/api/{Version}/Chatbot/{chatbotId}/activity", Method.POST);
            request.AddJsonBody(activity);
            return (await ExecuteAsync<object>(request, false)).Response;
        }

        private List<string> GetSessionArguments(DateTime? startedFrom, DateTime? startedTo, DateTime? endedFrom,
            DateTime? endedTo,
            string channel, string source, int limit, int? offset, string[] status)
        {
            var addedArguments = new List<string>();

            if (startedFrom.HasValue)
            {
                addedArguments.Add($"Created.Min={startedFrom.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}");
            }

            if (startedTo.HasValue)
            {
                addedArguments.Add($"Created.Max={startedTo.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}");
            }

            if (endedFrom.HasValue)
            {
                addedArguments.Add($"Ended.Min={endedFrom.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}");
            }

            if (endedTo.HasValue)
            {
                addedArguments.Add($"Ended.Max={endedTo.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}");
            }

            if (offset.HasValue)
            {
                addedArguments.Add($"offset={offset.Value}");
            }

            if (!string.IsNullOrEmpty(channel))
            {
                addedArguments.Add($"Channel={channel}");
            }

            if (!string.IsNullOrEmpty(source))
            {
                addedArguments.Add($"Source={source}");
            }

            if (status != null && status.Length > 0)
            {
                foreach (var oneStatus in status)
                {
                    addedArguments.Add($"status[]={oneStatus}");
                }
            }

            addedArguments.Add($"limit={limit}");

            return addedArguments;
        }

        private string AddArgumentsToUrl(string url, IList<string> queryParameters)
        {
            queryParameters ??= new List<string>();

            string argumentsString = string.Join("&", queryParameters.Where(arg => !string.IsNullOrEmpty(arg)));

            return !string.IsNullOrEmpty(argumentsString) ? $"{url}?{argumentsString}" : url;
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

    public class Contact
    {

        public long id { get; set; }
        public string[] oo1_guest_phone { get; set; }
        public string oo1_guest_ident { get; set; }
        public string[] oo1_guest_guid { get; set; }
        public string[] oo1_guest_email { get; set; }
    }
}
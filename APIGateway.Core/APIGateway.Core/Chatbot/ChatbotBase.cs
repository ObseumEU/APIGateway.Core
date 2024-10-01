using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIGateway.Core.Cache;
using APIGateway.Core.Chatbot.Activities;
using APIGateway.Core.MluviiClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace APIGateway.Core.Chatbot
{
    public abstract class ChatbotBase : IChatbotBase
    {
        public const string IS_DEBUG_KEY = "isDebug";
        public const string CALL_PARAMS_KEY = "callparams";
        private readonly ICacheService _cache;
        private readonly ApiGatewayCoreOptions _coreOptions;
        public readonly ILogger _log;
        private readonly ChatbotOptions _options;
        public readonly IMluviiClient mluviiClient;
        public Dictionary<string, string> CallParams;
        private ActivityBase currentActivity;
        public bool IsDebug;
        public bool LoadOnaStartCallParams = true;
        public long? SessionId;

        /// <summary>
        /// First start on this session. Can be forwarded on chatbot many times
        /// </summary>
        public bool IsFirstStart;

        public ChatbotBase(ILogger logger, IOptions<ChatbotOptions> options, IMluviiClient mluviiClient,
            IOptions<ApiGatewayCoreOptions> coreOptions, ICacheService cache)
        {
            _options = options?.Value;
            _log = logger;
            this.mluviiClient = mluviiClient;
            _cache = cache;
            _coreOptions = coreOptions?.Value;
        }

        public abstract Task OnReceiveActivityBase(ActivityBase activity);

        public async Task OnReceiveActivity(ActivityBase activity)
        {
            currentActivity = activity;
            SessionId = activity.sessionId;
            CallParams = GetLocalSessionParam<Dictionary<string, string>>(CALL_PARAMS_KEY);
            IsDebug = GetLocalSessionParam<bool>(IS_DEBUG_KEY);
            await OnReceiveActivityBase(activity);

            _log.LogDebug($"Chatbot received: {new JsonSerializer().Serialize(activity)}");

            if (activity.Activity == "ConversationStarted")
            {
                if (LoadOnaStartCallParams)
                    await GetCallParams(activity);

                //Check if its first start on this session.
                var IsFirstStartStr = GetLocalSessionParam<string>("FirstStart");
                IsFirstStart = string.IsNullOrEmpty(IsFirstStartStr);
                SaveLocalSessionParam("FirstStart", "false");
                
                await OnReceiveConversationStarted(activity);
            }

            if (activity.Activity == "GetCallParamsResponse")
            {
                CallParams = activity.callParams;
                var debugCallParam = CallParams.FirstOrDefault(d => d.Key.ToLower().Equals("debug"));
                IsDebug = debugCallParam.Value != null && debugCallParam.Value.ToLower().Equals("true");
                SaveLocalSessionParam(IS_DEBUG_KEY, IsDebug);
                SaveLocalSessionParam(CALL_PARAMS_KEY, CallParams);
                foreach (var callParam in CallParams) SaveLocalSessionParam(callParam.Key, callParam);

                await OnReceiveCallParams(activity);
            }

            if (activity.Activity == "Text") await OnReceiveText(activity);

        }

        public async Task<IRestResponse> SetupChatbotInMluvii()
        {
            _log.LogInformation(
                $"Chatbot setrup webhook ChatbotId:{_options.ChatbotID} CallbackUrl:{_coreOptions.Domain + "/" + _options.CallBackURL}");

            try
            {
                return await mluviiClient.SetChatbotCallbackUrl(_options.ChatbotID,
                    _coreOptions.Domain + "/" + _options.CallBackURL);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Cannot register chatbot webhook");
            }

            return null;
        }

        public async Task SendActivity(object activity)
        {
            await mluviiClient.SendChatbotActivity(_options.ChatbotID, activity);
        }

        public async Task Forward(long sessionId, int userId)
        {
            await mluviiClient.SendChatbotActivity(_options.ChatbotID,
                Elements.SendActivity.CreateForwardActivity(sessionId, userId));
        }

        public async Task ForwardRemote(long sessionId, int userId, int chatbotId)
        {
            await mluviiClient.SendChatbotActivity(chatbotId,
                Elements.SendActivity.CreateForwardActivity(sessionId, userId));
        }

        public async Task<bool> IsHealth()
        {
            var client = new RestClient(_coreOptions.Domain);
            var request = new RestRequest(_options.CallBackURL, Method.GET);
            var queryResult = await client.ExecuteGetAsync(request);
            if (queryResult.IsSuccessful != true)
                _log.LogError($"Cannot acces to chatbot endpoint: {queryResult.Content}");

            return queryResult.IsSuccessful;
        }

        public abstract Task OnReceiveText(ActivityBase activity);

        public abstract Task OnReceiveConversationStarted(ActivityBase activity);

        public abstract Task OnReceiveCallParams(ActivityBase activity);

        public void SaveLocalState(object item, double minutesLifetime = 120)
        {
            SaveLocalSessionParam("state", item, minutesLifetime);
        }

        public string GetLocalState()
        {
            return GetLocalSessionParam<string>("state");
        }

        public void SaveLocalSessionParam(string key, object item, double minutesLifetime = 120)
        {
            if (SessionId == null)
                throw new Exception("Save params is possible only after OnReceiveActivity");

            if (item == null)
                return;

            _cache.Set($"{key}-{SessionId}", item, minutesLifetime);
        }

        public T GetLocalSessionParam<T>(string key)
        {
            return _cache.Get<T>($"{key}-{SessionId}");
        }

        public async Task SendText(ActivityBase activity, string text)
        {
            await mluviiClient.SendChatbotActivity(_options.ChatbotID,
                Elements.SendActivity.CreateTextActivity(activity, text));
        }

        public async Task SendText(string text)
        {
            await mluviiClient.SendChatbotActivity(_options.ChatbotID,
                Elements.SendActivity.CreateTextActivity(currentActivity, text));
        }

        public async Task Forward(ActivityBase activity, int userId)
        {
            if (activity.sessionId == null)
            {
                _log.LogError("Cannot redirect null sessionId on chatbot.");
                return;
            }

            await Forward(activity.sessionId.Value, userId);
        }

        public async Task DisableGuestInput(ActivityBase activity)
        {
            var msg = Elements.SendActivity.Create(activity);
            msg.activity = "DisableGuestInput";
            await SendActivity(msg);
        }

        public async Task GetCallParams(ActivityBase activity)
        {
            var msg = Elements.SendActivity.Create(activity);
            msg.activity = "GetCallParams";
            await SendActivity(msg);
        }

        public async Task EnableGuestInput(ActivityBase activity)
        {
            var msg = Elements.SendActivity.Create(activity);
            msg.activity = "EnableGuestInput";
            await SendActivity(msg);
        }

        public async Task EnableGuestInput()
        {
            await EnableGuestInput(currentActivity);
        }

        public async Task HandOff(ActivityBase activity)
        {
            var msg = Elements.SendActivity.Create(activity);
            msg.activity = "HandOff";
            await SendActivity(msg);
        }
    }
}
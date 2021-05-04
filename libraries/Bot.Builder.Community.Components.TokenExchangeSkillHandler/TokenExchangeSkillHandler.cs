using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Components.TokenExchangeSkillHandler
{
#pragma warning disable CA1812 // Avoid uninstantiated internal
    internal class TokenExchangeSkillHandler : CloudSkillHandler
#pragma warning restore CA1812 // Avoid uninstantiated internal
    {
        private readonly BotAdapter _adapter;
        private readonly SkillsConfiguration _skillsConfig;
        private readonly string _botId;
        private readonly string _connectionName;
        private readonly SkillConversationIdFactoryBase _conversationIdFactory;
        private readonly BotFrameworkAuthentication _botAuth;
        private readonly ILogger _logger;

        public TokenExchangeSkillHandler(
            BotAdapter adapter,
            IBot bot,
            IConfiguration configuration,
            SkillConversationIdFactoryBase conversationIdFactory,
            BotFrameworkAuthentication authConfig,
            SkillsConfiguration skillsConfig = default,
            ILogger logger = default)
            : base(adapter, bot, conversationIdFactory, authConfig, logger)
        {
            _adapter = adapter;
            _botAuth = authConfig;
            _conversationIdFactory = conversationIdFactory;
            _skillsConfig = skillsConfig ?? new SkillsConfiguration(configuration);
            _botId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            _logger = logger;

            var settings = configuration.GetSection("Bot.Builder.Community.Components.TokenExchangeSkillHandler").Get<ComponentSettings>() ?? new ComponentSettings();
            _connectionName = settings.TokenExchangeConnectionName ?? configuration.GetSection("tokenExchangeConnectionName")?.Value;
        }

        protected override async Task<ResourceResponse> OnSendToConversationAsync(ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await InterceptOAuthCardsAsync(claimsIdentity, activity).ConfigureAwait(false))
            {
                return new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
            }

            return await base.OnSendToConversationAsync(claimsIdentity, conversationId, activity, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await InterceptOAuthCardsAsync(claimsIdentity, activity).ConfigureAwait(false))
            {
                return new ResourceResponse(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
            }

            return await base.OnReplyToActivityAsync(claimsIdentity, conversationId, activityId, activity, cancellationToken).ConfigureAwait(false);
        }

        private BotFrameworkSkill GetCallingSkill(ClaimsIdentity claimsIdentity)
        {
            var appId = JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims);

            if (string.IsNullOrWhiteSpace(appId))
            {
                return null;
            }

            return _skillsConfig.Skills.Values.FirstOrDefault(s => string.Equals(s.AppId, appId, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> InterceptOAuthCardsAsync(ClaimsIdentity claimsIdentity, Activity activity)
        {
            var oauthCardAttachment = activity.Attachments?.FirstOrDefault(a => a?.ContentType == OAuthCard.ContentType);
            if (oauthCardAttachment != null)
            {
                var targetSkill = GetCallingSkill(claimsIdentity);
                if (targetSkill != null)
                {
                    var oauthCard = ((JObject)oauthCardAttachment.Content).ToObject<OAuthCard>();

                    if (!string.IsNullOrWhiteSpace(oauthCard?.TokenExchangeResource?.Uri))
                    {
                        using (var context = new TurnContext(_adapter, activity))
                        {
                            context.TurnState.Add<IIdentity>("BotIdentity", claimsIdentity);

                            // AAD token exchange
                            try
                            {
                                var tokenClient = await _botAuth.CreateUserTokenClientAsync(claimsIdentity, CancellationToken.None).ConfigureAwait(false);
                                var result = await tokenClient.ExchangeTokenAsync(
                                    activity.Recipient.Id,
                                    _connectionName,
                                    activity.ChannelId,
                                    new TokenExchangeRequest() { Uri = oauthCard.TokenExchangeResource.Uri },
                                    CancellationToken.None).ConfigureAwait(false);

                                if (!string.IsNullOrEmpty(result?.Token))
                                {
                                    // If token above is null, then SSO has failed and hence we return false.
                                    // If not, send an invoke to the skill with the token. 
                                    return await SendTokenExchangeInvokeToSkillAsync(activity, oauthCard.TokenExchangeResource.Id, result.Token, oauthCard.ConnectionName, targetSkill, default).ConfigureAwait(false);
                                }
                            }
#pragma warning disable CA1031 // Do not catch general exception types
                            catch
#pragma warning restore CA1031 // Do not catch general exception types
                            {
                                return false;
                            }

                            return false;
                        }
                    }
                }
            }

            return false;
        }

        private async Task<bool> SendTokenExchangeInvokeToSkillAsync(Activity incomingActivity, string id, string token, string connectionName, BotFrameworkSkill targetSkill, CancellationToken cancellationToken)
        {
            var activity = incomingActivity.CreateReply() as Activity;
            activity.Type = ActivityTypes.Invoke;
            activity.Name = SignInConstants.TokenExchangeOperationName;
            activity.Value = new TokenExchangeInvokeRequest()
            {
                Id = id,
                Token = token,
                ConnectionName = connectionName,
            };

            var skillConversationReference = await _conversationIdFactory.GetSkillConversationReferenceAsync(incomingActivity.Conversation.Id, cancellationToken).ConfigureAwait(false);
            activity.Conversation = skillConversationReference.ConversationReference.Conversation;

            // route the activity to the skill
            using (var client = _botAuth.CreateBotFrameworkClient())
            {
                var response = await client.PostActivityAsync(_botId, targetSkill.AppId, targetSkill.SkillEndpoint, _skillsConfig.SkillHostEndpoint, incomingActivity.Conversation.Id, activity, cancellationToken).ConfigureAwait(false);

                // Check response status: true if success, false if failure
                return response.Status >= 200 && response.Status <= 299;
            }
        }
    }
}

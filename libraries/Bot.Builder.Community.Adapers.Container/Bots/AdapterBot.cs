// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace AdapterBot.Bots
{
    public class AdapterBot : ActivityHandler
    {
        private readonly string _botId;
        private readonly SkillHttpClient _skillClient;
        private readonly BotFrameworkSkill _targetSkill;
        private readonly Uri _callbackUrl;

        public AdapterBot(SkillHttpClient skillClient, IConfiguration configuration)
        {
            _skillClient = skillClient ?? throw new ArgumentNullException(nameof(skillClient));

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _botId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            if (string.IsNullOrWhiteSpace(_botId))
            {
                throw new ArgumentException($"{MicrosoftAppCredentials.MicrosoftAppIdKey} is not set in configuration");
            }

            _targetSkill = new BotFrameworkSkill()
            {
                AppId = configuration?.GetValue<string>("SkillAppId"),
                SkillEndpoint = configuration?.GetValue<Uri>("SkillEndpoint")
            };

            _callbackUrl = configuration?.GetValue<Uri>("SkillHostEndpoint");
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            // route the activity to the skill
            var response = await _skillClient.PostActivityAsync<ExpectedReplies>(_botId, _targetSkill, _callbackUrl, turnContext.Activity, cancellationToken);

            if (turnContext.Activity.DeliveryMode == DeliveryModes.ExpectReplies && response.Body.Activities != null && response.Body.Activities.Any())
            {
                foreach (var activityFromSkill in response.Body.Activities)
                {

                        if (activityFromSkill.Type == ActivityTypesEx.InvokeResponse && activityFromSkill.Value is JObject jObject)
                        {
                            // Ensure the value in the invoke response is of type InvokeResponse (it gets deserialized as JObject by default).
                            activityFromSkill.Value = jObject.ToObject<InvokeResponse>();
                        }

                        // Send the response back to the channel. 
                        await turnContext.SendActivityAsync(activityFromSkill, cancellationToken).ConfigureAwait(false);
                }
            }

            // Check response status
            if (!(response.Status >= 200 && response.Status <= 299))
            {
                throw new HttpRequestException($"Error invoking the skill id: \"{_targetSkill.Id}\" at \"{_targetSkill.SkillEndpoint}\" (status is {response.Status}). \r\n {response.Body}");
            }
        }
    }
}

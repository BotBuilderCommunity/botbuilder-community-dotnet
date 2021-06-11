using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Action.Model;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Attachments;
using Microsoft.Bot.Builder;
using Bot.Builder.Community.Components.Adapters.GoogleBusiness.Core.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness.Action
{
    /// <summary>
    /// Send an activity back to the user.
    /// </summary>
    public class SendGBMActivity : Dialog
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "BotBuilderCommunity.SendGBMActivity";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGBMActivity"/> class.
        /// </summary>
        /// <param name="activity"><see cref="Activity"/> to send.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        public SendGBMActivity(Activity activity, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new StaticActivityTemplate(activity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGBMActivity"/> class.
        /// </summary>
        /// <param name="text">Optional, template to evaluate to create the activity.</param>
        /// <param name="callerPath">Optional, source file full path.</param>
        /// <param name="callerLine">Optional, line number in source file.</param>
        [JsonConstructor]
        public SendGBMActivity(string text = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new ActivityTemplate(text ?? string.Empty);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        /// <summary>
        /// Gets or sets template for the activity.
        /// </summary>
        /// <value>
        /// Template for the activity.
        /// </value>
        [JsonProperty("activity")]
        public ITemplate<Activity> Activity { get; set; }

        /// <summary>
        /// Gets or sets additional OpenUrl actions to send with the activity.
        /// </summary>
        /// <value>
        /// Additional OpenUrl actions.
        /// </value>
        [JsonProperty("openUrlActions")]
        public List<OpenUrlActionProperty> OpenUrlActions { get; set; } = new List<OpenUrlActionProperty>();

        [JsonProperty("dialActions")]
        public List<DialActionProperty> DialActions { get; set; } = new List<DialActionProperty>();

        [JsonProperty("authRequestAction")]
        public AuthActionProperty AuthRequestAction { get; set; }

        [JsonProperty("includeLiveAgentRequestAction")]
        public BoolExpression IncludeLiveAgentRequestAction { get; set; }

        [JsonProperty("richCardDetails")]
        public RichCardDetailsProperty RichCardDetails { get; set; }

        /// <summary>
        /// Called when the dialog is started and pushed onto the dialog stack.
        /// </summary>
        /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
        /// <param name="options">Optional, initial information to pass to the dialog.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (Disabled != null && Disabled.GetValue(dc.State))
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var activity = await Activity.BindAsync(dc, dc.State).ConfigureAwait(false);
            var properties = new Dictionary<string, string>()
            {
                { "template", JsonConvert.SerializeObject(Activity) },
                { "result", activity == null ? string.Empty : JsonConvert.SerializeObject(activity, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }) },
            };

            var suggestions = GetSuggestions(dc);
            activity.Attachments = suggestions.Select(a => a.ToAttachment()).ToList();
            var richCard = GetRichCard(dc);

            if (richCard != null)
            {
                richCard.StandaloneCard.CardContent.Suggestions = suggestions;
                activity.Attachments.Add(richCard.ToAttachment());
            }

            TelemetryClient.TrackEvent("GeneratorResult", properties);

            ResourceResponse response = null;
            if (activity.Type != "message"
                || !string.IsNullOrEmpty(activity.Text)
                || activity.Attachments?.Any() == true
                || !string.IsNullOrEmpty(activity.Speak)
                || activity.SuggestedActions != null
                || activity.ChannelData != null)
            {
                response = await dc.Context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
            }

            return await dc.EndDialogAsync(response, cancellationToken).ConfigureAwait(false);
        }

        private RichCardContent GetRichCard(DialogContext dc)
        {
            if(RichCardDetails != null &&
               (RichCardDetails.Title != null ||
               RichCardDetails.Description != null ||
               RichCardDetails.MediaFileUrl != null))
            {
                var richCardContent = new RichCardContent()
                {
                    StandaloneCard = new StandaloneCard()
                    {
                        CardContent = new CardContent()
                        {
                            Title = RichCardDetails.Title?.GetValue(dc.State),
                            Description = RichCardDetails.Description?.GetValue(dc.State)
                        }
                    }
                };

                if (RichCardDetails.MediaFileUrl != null)
                {
                    richCardContent.StandaloneCard.CardContent.Media = new Media()
                    {
                        ContentInfo = new ContentInfo()
                        {
                            FileUrl = RichCardDetails.MediaFileUrl?.GetValue(dc.State),
                            AltText = RichCardDetails.MediaAltText?.GetValue(dc.State),
                            ForceRefresh = RichCardDetails.ForceRefresh.GetValue(dc.State)
                        },
                        Height = RichCardDetails.MediaHeight?.GetValue(dc.State)
                    };
                }

                return richCardContent;
            }

            return null;
        }

        private List<Suggestion> GetSuggestions(DialogContext dc)
        {
            var suggestions = new List<Suggestion>();

            foreach (var openUrlActionProperty in this.OpenUrlActions)
            {
                var openUrlSuggestedAction = new OpenUrlActionSuggestion()
                {
                    Action = new OpenUrlSuggestedActionContent()
                    {
                        Text = openUrlActionProperty.Text?.GetValue(dc.State),
                        PostbackData = openUrlActionProperty.PostBackData?.GetValue(dc.State),
                        OpenUrlAction = new OpenUrlAction()
                        {
                            Url = openUrlActionProperty.Url?.GetValue(dc.State)
                        }
                    }
                };

                suggestions.Add(openUrlSuggestedAction);
            }

            foreach (var dialActionProperty in this.DialActions)
            {
                var dialSuggestedAction = new DialActionSuggestion()
                {
                    Action = new DialSuggestedActionContent()
                    {
                        Text = dialActionProperty.Text?.GetValue(dc.State),
                        PostbackData = dialActionProperty.PostBackData?.GetValue(dc.State),
                        DialAction = new DialAction()
                        {
                            PhoneNumber = dialActionProperty.PhoneNumber?.GetValue(dc.State)
                        }
                    }
                };

                suggestions.Add(dialSuggestedAction);
            }

            var includeLiveAgentSuggestion = IncludeLiveAgentRequestAction.GetValue(dc.State);
            if (includeLiveAgentSuggestion)
            {
                suggestions.Add(new LiveAgentRequestSuggestion());
            }

            return suggestions;
        }

        /// <inheritdoc/>
        protected override string OnComputeId()
        {
            if (Activity is ActivityTemplate at)
            {
                return $"{GetType().Name}({StringUtils.Ellipsis(at.Template.Trim(), 30)})";
            }

            return $"{GetType().Name}('{StringUtils.Ellipsis(Activity?.ToString().Trim(), 30)}')";
        }
    }
}
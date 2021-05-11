using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Sequence;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static Microsoft.Recognizers.Text.Culture;

namespace Bot.Builder.Community.Components.Dialogs.Input
{
    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum SocialMediaInputType
    {
        Mention,
        Hashtag
    }
    public class SocialMediaInput : InputDialog
    {
        [JsonProperty("$Kind")]
        public const string Kind = "BotBuilderCommunity.SocialMediaInput";

        [JsonProperty("defaultLocale")]
        public StringExpression DefaultLocale { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        [JsonProperty("mediaType")]
        public EnumExpression<SocialMediaInputType> MediaType { get; set; } = SocialMediaInputType.Mention;

        [JsonConstructor]
        public SocialMediaInput([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        protected override Task<IActivity> OnRenderPromptAsync(DialogContext dc, InputState state,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (this.Prompt == null)
            {
                this.Prompt = new StaticActivityTemplate(MessageFactory.Text("Prompt for a Social Media Type"));
            }

            return base.OnRenderPromptAsync(dc, state, cancellationToken);
        }

        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var validateText = dc.State.GetValue<object>(VALUE_PROPERTY);

            if (!(validateText is string strEmailText))
            {
                return Task.FromResult(InputState.Invalid);
            }

            var culture = GetCulture(dc);
            List<ModelResult> modelResult = null;

            switch (this.MediaType.GetValue(dc.State))
            {
                case SocialMediaInputType.Mention:
                    modelResult = SequenceRecognizer.RecognizeMention(validateText.ToString(), culture);
                    break;
                case SocialMediaInputType.Hashtag:
                    modelResult = SequenceRecognizer.RecognizeHashtag(validateText.ToString(), culture);
                    break;
                default:
                    return Task.FromResult(InputState.Invalid);
            }

            if (modelResult == null || modelResult.Count <= 0)
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            var result = modelResult[0].Resolution["value"].ToString();
            dc.State.SetValue(this.ResultProperty.GetValue(dc.State), result);
            return Task.FromResult(InputState.Valid);

        }

        private string GetCulture(DialogContext dc)
        {
            if (!string.IsNullOrEmpty(dc.Context.Activity.Locale))
            {
                return dc.Context.Activity.Locale;
            }

            return this.DefaultLocale != null ? this.DefaultLocale.GetValue(dc.State) : English;
        }
    }
}

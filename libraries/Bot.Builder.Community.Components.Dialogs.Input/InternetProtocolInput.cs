using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Recognizers.Text.Culture;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.Sequence;

namespace Bot.Builder.Community.Components.Dialogs.Input
{
    public class InternetProtocolInput : InputDialog
    {
        [JsonProperty("$Kind")]
        public const string Kind = "BotBuilderCommunity.InternetProtocolInput";

        [JsonProperty("defaultLocale")]
        public StringExpression DefaultLocale { get; set; }

        [JsonProperty("ProtocolType")]
        public EnumExpression<InternetProtocolInputType> ProtocolType { get; set; } = InternetProtocolInputType.IpAddress;

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        [JsonConstructor]
        public InternetProtocolInput([CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        protected override Task<IActivity> OnRenderPromptAsync(DialogContext dc, InputState state,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (this.Prompt == null)
            {
                this.Prompt = new StaticActivityTemplate(MessageFactory.Text("Prompt for a InternetProtocolInput"));
            }

            return base.OnRenderPromptAsync(dc, state, cancellationToken);
        }
        protected override Task<InputState> OnRecognizeInputAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            var validateText = dc.State.GetValue<object>(VALUE_PROPERTY);
            if (!(validateText is string))
            {
                return Task.FromResult(InputState.Invalid);
            }

            var culture = GetCulture(dc);
            List<ModelResult> results;

            var message = validateText.ToString();

            switch (ProtocolType.GetValue(dc.State))
            {
                case InternetProtocolInputType.IpAddress:
                    results = SequenceRecognizer.RecognizeIpAddress(message, culture);
                    break;
                case InternetProtocolInputType.Url:
                    results = SequenceRecognizer.RecognizeURL(message, culture);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (results == null || results.Count <= 0)
            {
                return Task.FromResult(InputState.Unrecognized);
            }

            var result = results[0].Resolution["value"].ToString();

            if (ResultProperty != null)
            {
                dc.State.SetValue(ResultProperty.GetValue(dc.State), result);
            }

            return Task.FromResult(InputState.Valid);
        }

        private string GetCulture(DialogContext dc)
        {
            if (!string.IsNullOrEmpty(dc.Context.Activity.Locale))
            {
                return dc.Context.Activity.Locale;
            }

            return DefaultLocale != null ? DefaultLocale.GetValue(dc.State) : English;
        }
    }

    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum InternetProtocolInputType
    {
        IpAddress,
        Url
    }
}

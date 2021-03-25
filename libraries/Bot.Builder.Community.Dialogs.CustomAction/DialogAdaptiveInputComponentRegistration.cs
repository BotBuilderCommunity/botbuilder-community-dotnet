using System.Collections.Generic;
using Bot.Builder.Community.Dialogs.Adaptive.Input.Input;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Dialogs.Adaptive.Input
{
    public class DialogAdaptiveInputComponentRegistration : ComponentRegistration,IComponentDeclarativeTypes
    {
        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            yield return new DeclarativeType<EmailInput>(EmailInput.Kind);
            yield return new DeclarativeType<PhoneNumberInput>(PhoneNumberInput.Kind);
            yield return new DeclarativeType<SocialMediaInput>(SocialMediaInput.Kind);
        }

        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }
    }
}

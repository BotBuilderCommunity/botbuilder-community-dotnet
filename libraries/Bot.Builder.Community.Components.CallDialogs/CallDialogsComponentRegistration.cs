using System.Collections.Generic;
using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Obsolete;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.CallDialogs
{
    /// <summary>
    /// <see cref="CallDialogsComponentRegistration"/> implementation for adaptive components.
    /// </summary>
    public class CallDialogsComponentRegistration 
        : DeclarativeComponentRegistrationBridge<CallDialogsBotComponent>
    { 
    }
}

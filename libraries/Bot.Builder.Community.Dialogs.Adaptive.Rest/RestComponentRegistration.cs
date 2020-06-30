using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Dialogs.Adaptive.Rest
{
    public class RestComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            return new JsonConverter[] { };
        }

        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            return new DeclarativeType[] { };
        }
    }
}

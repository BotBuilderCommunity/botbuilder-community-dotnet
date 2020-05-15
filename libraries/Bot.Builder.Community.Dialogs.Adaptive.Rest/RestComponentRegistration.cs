using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Dialogs.Adaptive.Rest
{
    public class RestComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, Stack<string> paths)
        {
            return new JsonConverter[] { };
        }

        public IEnumerable<DeclarativeType> GetDeclarativeTypes()
        {
            return new DeclarativeType[] { };
        }
    }
}

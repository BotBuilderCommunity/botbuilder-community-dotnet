using System;
using System.Collections.Generic;
using Bot.Builder.Community.Dialogs.Adaptive.Sql.Actions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Dialogs.Adaptive.Sql
{
    public class SqlComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }

        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            yield return new DeclarativeType<DeleteRow>(DeleteRow.DeclarativeType);
            yield return new DeclarativeType<ExecuteQuery>(ExecuteQuery.DeclarativeType);
            yield return new DeclarativeType<GetRow>(GetRow.DeclarativeType);
            yield return new DeclarativeType<GetRows>(GetRows.DeclarativeType);
            yield return new DeclarativeType<InsertRow>(InsertRow.DeclarativeType);
            yield return new DeclarativeType<UpdateRow>(UpdateRow.DeclarativeType);
        }
    }
}

using System.Collections.Generic;
using AdaptiveExpressions.Converters;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Storage
{
    /// <summary>
    /// <see cref="StorageComponentRegistration"/> implementation for adaptive components.
    /// </summary>
    public class StorageComponentRegistration : ComponentRegistration, IComponentDeclarativeTypes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageComponentRegistration"/> class.
        /// </summary>
        public StorageComponentRegistration()
        {
        }

        /// <summary>
        /// Gets adaptive <see cref="DeclarativeType"/> resources.
        /// </summary>
        /// <param name="resourceExplorer"><see cref="ResourceExplorer"/> with expected path to get all schema resources.</param>
        /// <returns>Adaptive <see cref="DeclarativeType"/> resources.</returns>
        public virtual IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            yield return new DeclarativeType<DeleteStorageItem>(DeleteStorageItem.Kind);
            yield return new DeclarativeType<ReadStorageItem>(ReadStorageItem.Kind);
            yield return new DeclarativeType<WriteStorageItem>(WriteStorageItem.Kind);
            yield break;
        }

        /// <summary>
        /// Gets adaptive <see cref="JsonConverter"/> resources.
        /// </summary>
        /// <param name="resourceExplorer">ResourceExplorer to use to resolve references.</param>
        /// <param name="sourceContext">SourceContext to build debugger source map.</param>
        /// <returns>Adaptive <see cref="JsonConverter"/> resources.</returns>
        public virtual IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            yield break;
        }
    }
}

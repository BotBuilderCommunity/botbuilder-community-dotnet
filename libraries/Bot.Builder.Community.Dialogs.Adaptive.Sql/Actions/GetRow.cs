using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Dapper;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Dialogs.Adaptive.Sql.Actions
{
    public class GetRow : SqlAction
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Community.SqlGetRow";

        public GetRow(string connection, string table, Dictionary<string, ValueExpression> keys, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.Table = table ?? throw new ArgumentNullException(nameof(table));
            this.Keys = keys ?? throw new ArgumentNullException(nameof(keys)); 
        }

        [JsonConstructor]
        public GetRow([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(callerPath, callerLine)
        {
        }

        /// <summary>
        /// Gets or sets the table to insert the row.
        /// </summary>
        /// <value>Table name.</value>
        [JsonProperty("table")]
        public StringExpression Table { get; set; }

        /// <summary>
        /// Gets or sets the row id.
        /// </summary>
        /// <value>Row object.</value>
        [JsonProperty("keys")]
        public Dictionary<string, ValueExpression> Keys { get; set; }

        /// <summary>
        /// Gets or sets the property expression to store the query response. 
        /// </summary>
        /// <remarks>
        /// The result will have 3 properties from the sql query: 
        /// [hasError|errorMessage|row].
        /// </remarks>
        /// <value>
        /// The property expression to store the query response in. 
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            if (this.Disabled != null && this.Disabled.GetValue(dc.State) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var (instanceTable, instanceTableError) = this.Table.TryGetValue(dc.State);
            if (instanceTableError != null)
            {
                throw new ArgumentException(instanceTableError);
            }

            //binding keys
            string keyList = string.Join(" AND ", this.Keys.Select(p => $"{p.Key} = @k_{p.Key}"));
            var keyParamList = this.Keys.Select(k => new KeyValuePair<string, object>($"@k_{k.Key}", k.Value.GetValue(dc.State)));

            string sql = $"SELECT * FROM [{instanceTable}] WHERE {keyList};";

            Result sqlResult = new Result();

            try
            {
                using DbConnection connection = GetConnection(dc.State);
                sqlResult.Row = await connection.QuerySingleOrDefaultAsync(sql, new DynamicParameters(keyParamList)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                sqlResult.HasError = true;
                sqlResult.ErrorMessage = ex.Message;
            }

            if (this.ResultProperty != null)
            {
                dc.State.SetValue(this.ResultProperty.GetValue(dc.State), sqlResult);
            }

            // return the actionResult as the result of this operation
            return await dc.EndDialogAsync(result: sqlResult, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{Connection} {Table}]";
        }

        /// <summary>
        /// Result data of the the http operation.
        /// </summary>
        public class Result
        {
            public Result()
            {
            }

            /// <summary>
            /// Gets or sets a value indicating whether an error occured.
            /// </summary>
            /// <value>Response content body.</value>
            [JsonProperty("hasError")]
            public bool HasError { get; set; }

            /// <summary>
            /// Gets or sets the error message.
            /// </summary>
            /// <value>Response content body.</value>
            [JsonProperty("errorMessage")]
            public string ErrorMessage { get; set; }

            /// <summary>
            /// Gets or sets the row.
            /// </summary>
            /// <value>Row object.</value>
            [JsonProperty("row")]
            public object Row { get; set; }
        }
    }
}

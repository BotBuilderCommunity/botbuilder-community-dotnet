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
    public class UpdateRow : SqlAction
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Sql.UpdateRow";

        public UpdateRow(string connection, string table, Dictionary<string, ValueExpression> values, Dictionary<string, ValueExpression> keys, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.Table = table ?? throw new ArgumentNullException(nameof(table));
            this.Values = values ?? throw new ArgumentNullException(nameof(values));
            this.Keys = keys ?? throw new ArgumentNullException(nameof(keys)); 
        }

        [JsonConstructor]
        public UpdateRow([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// Gets or sets the values to update.
        /// </summary>
        /// <value>Row object.</value>
        [JsonProperty("values")]
        public Dictionary<string, ValueExpression> Values { get; set; }

        /// <summary>
        /// Gets or sets the row id.
        /// </summary>
        /// <value>Row object.</value>
        [JsonProperty("keys")]
        public Dictionary<string, ValueExpression> Keys { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            var dcState = dc.GetState();

            if (this.Disabled != null && this.Disabled.GetValue(dcState) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            var (instanceTable, instanceTableError) = this.Table.TryGetValue(dcState);
            if (instanceTableError != null)
            {
                throw new ArgumentException(instanceTableError);
            }

            //binding values
            string valueList = string.Join(" AND ", this.Values.Select(p => $"{p.Key} =  @v_{p.Key}"));
            var valueParamList = this.Values.Select(v => new KeyValuePair<string, object>($"@v_{v.Key}", v.Value.GetValue(dcState)));

            //binding keys
            string keyList = string.Join(" AND ", this.Keys.Select(p => $"{p.Key} = @k_{p.Key}"));
            var keyParamList = this.Keys.Select(k => new KeyValuePair<string, object>($"@k_{k.Key}", k.Value.GetValue(dcState)));

            //merge params
            var paramList = valueParamList.Union(keyParamList).ToList();

            string sql = $"UPDATE [{instanceTable}] SET {valueList}  WHERE {keyList};";

            Result sqlResult = new Result();

            try
            {
                using DbConnection connection = GetConnection(dcState);
                sqlResult.UpdatedRows = connection.Execute(sql, new DynamicParameters(paramList));
            }
            catch (Exception ex)
            {
                sqlResult.HasError = true;
                sqlResult.ErrorMessage = ex.Message;
            }

            Trace.TraceInformation(sqlResult.UpdatedRows.ToString());

            // return the actionResult as the result of this operation
            return await dc.EndDialogAsync(result: sqlResult, cancellationToken: cancellationToken);
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
            /// Gets or sets the number of rows inserted.
            /// </summary>
            /// <value>Number of row inserted.</value>
            [JsonProperty("updatedRows")]
            public int UpdatedRows { get; set; }
        }
    }
}

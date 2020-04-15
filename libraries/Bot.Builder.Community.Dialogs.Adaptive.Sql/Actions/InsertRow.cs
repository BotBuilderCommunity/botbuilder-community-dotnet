using System;
using System.Collections.Generic;
using System.Data;
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
    public class InsertRow : BaseSqlAction
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Sql.InsertRow";

        public InsertRow(string connection, string table, Dictionary<string, ValueExpression> values, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.Table = table ?? throw new ArgumentNullException(nameof(table));
            this.Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        [JsonConstructor]
        public InsertRow([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// Gets or sets the row.
        /// </summary>
        /// <value>Row object.</value>
        [JsonProperty("values")]
        public Dictionary<string, ValueExpression> Values { get; set; }

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

            var instanceRow = new object();
            var instanceRowJObject = JObject.FromObject(instanceRow);

            string columnList = string.Join(", ", this.Values.Keys);
            string columnParams = string.Join(", @", this.Values.Keys);
            var valueParams = this.Values.Select(v => new KeyValuePair<string, object>($"@{v.Key}", v.Value.GetValue(dc.State))).ToList();

            string sql = $"INSERT INTO [{instanceTable}] ({columnList}) Values (@{columnParams});";

            Trace.TraceInformation(sql);

            Result sqlResult = new Result();

            try
            {
                using DbConnection connection = GetConnection(dc.State);
                sqlResult.InsertedRows = connection.Execute(sql, new DynamicParameters(valueParams));
            }
            catch (Exception ex)
            {
                sqlResult.HasError = true;
                sqlResult.ErrorMessage = ex.Message;
            }

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
            [JsonProperty("insertedRows")]
            public int InsertedRows { get; set; }
        }
    }
}

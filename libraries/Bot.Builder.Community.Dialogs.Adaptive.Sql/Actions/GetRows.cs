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
    public class GetRows : SqlAction
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Community.SqlGetRows";

        public GetRows(string connection, string table, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        [JsonConstructor]
        public GetRows([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
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
        /// Gets or sets the where clause.
        /// </summary>
        /// <value>Table name.</value>
        [JsonProperty("where")]
        public StringExpression WhereClause { get; set; }

        /// <summary>
        /// Gets or sets the property expression to store the query response. 
        /// </summary>
        /// <remarks>
        /// The result will have 3 properties from the sql query: 
        /// [hasError|errorMessage|rows].
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

            string sql = $"SELECT * FROM [{instanceTable}];";

            if (this.WhereClause != null)
            {
                var (instanceWhereClause, instanceWhereClauseError) = this.WhereClause.TryGetValue(dc.State);
                if (instanceTableError != null)
                {
                    throw new ArgumentException(instanceWhereClauseError);
                }

                sql += $" WHERE {instanceWhereClause}";
            }

            Result sqlResult = new Result();

            try
            {
                using DbConnection connection = GetConnection(dc.State);
                sqlResult.Rows = await connection.QueryAsync(sql).ConfigureAwait(false);
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
            /// <value>Boolean.</value>
            [JsonProperty("hasError")]
            public bool HasError { get; set; }

            /// <summary>
            /// Gets or sets the error message.
            /// </summary>
            /// <value>Error Message.</value>
            [JsonProperty("errorMessage")]
            public string ErrorMessage { get; set; }

            /// <summary>
            /// Gets or sets the number of rows inserted.
            /// </summary>
            /// <value>Number of row inserted.</value>
            [JsonProperty("rows")]
            public IEnumerable<object> Rows { get; set; }
        }
    }
}

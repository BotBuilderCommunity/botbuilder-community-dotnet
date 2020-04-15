// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Memory;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Dialogs.Adaptive.Sql.Actions
{
    /// <summary>
    /// Action for performing a sql action.
    /// </summary>
    public abstract class SqlAction : Dialog
    {
        public SqlAction(string connection, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Connection = connection;
        }

        [JsonConstructor]
        public SqlAction([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets the connection to use.
        /// </summary>
        /// <value>
        /// Connection string.
        /// </value>
        [JsonProperty("connection")]
        public StringExpression Connection { get; set; }

        /// <summary>
        /// Gets or sets the provider name to use.
        /// </summary>
        /// <value>
        /// Connection string.
        /// </value>
        [JsonProperty("providerName")]
        public StringExpression ProviderName { get; set; }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{Connection}]";
        }

        protected DbConnection GetConnection(DialogStateManager dialogState)
        {
            var (instanceProviderName, instanceProviderNameError) = this.ProviderName.TryGetValue(dialogState);
            if (instanceProviderNameError != null)
            {
                throw new ArgumentException(instanceProviderNameError);
            }

            DbProviderFactory factory = DbProviderFactories.GetFactory(instanceProviderName);

            var connection = factory.CreateConnection();
            connection.ConnectionString = this.Connection.GetValue(dialogState);

            return connection;
        }
    }
}
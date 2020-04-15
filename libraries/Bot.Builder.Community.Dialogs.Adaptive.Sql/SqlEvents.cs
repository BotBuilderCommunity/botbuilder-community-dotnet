using System;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Builder.Community.Dialogs.Adaptive.Sql
{
    public class SqlEvents : DialogEvents
    {
        /// <summary>
        /// Raised when row is updated.
        /// </summary>
        public const string RowInserted = "SqlRowInserted";

        /// <summary>
        /// Raised when row is updated.
        /// </summary>
        public const string RowUpdated = "SqlRowUpdated";
    }
}

namespace Bot.Builder.Community.Dialogs.Luis.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// The dialog response.
    /// </summary>
    public partial class DialogResponse
    {
        /// <summary>
        /// Initializes a new instance of the DialogResponse class.
        /// </summary>
        public DialogResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the DialogResponse class.
        /// </summary>
        public DialogResponse(string prompt = default(string), string parameterName = default(string), string parameterType = default(string), string contextId = default(string), string status = default(string))
        {
            Prompt = prompt;
            ParameterName = parameterName;
            ParameterType = parameterType;
            ContextId = contextId;
            Status = status;
        }

        /// <summary>
        /// Gets or sets the prompt that should be asked.
        /// </summary>
        /// <value>
        /// The prompt that should be asked.
        /// </value>
        [JsonProperty(PropertyName = "prompt")]
        public string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        /// <value>
        /// The name of the parameter.
        /// </value>
        [JsonProperty(PropertyName = "parameterName")]
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        /// <value>
        /// The type of the parameter.
        /// </value>
        [JsonProperty(PropertyName = "parameterType")]
        public string ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the context id for dialog.
        /// </summary>
        /// <value>
        /// The context id for dialog.
        /// </value>
        [JsonProperty(PropertyName = "contextId")]
        public string ContextId { get; set; }

        /// <summary>
        /// Gets or sets the dialog status. Possible values include: 'Question', 'Finished'
        /// </summary>
        /// <value>
        /// Or sests the dialog status. Possible values include: 'Question', 'Finished'
        /// </value>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}

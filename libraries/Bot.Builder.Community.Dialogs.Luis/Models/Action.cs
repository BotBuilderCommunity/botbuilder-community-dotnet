namespace Bot.Builder.Community.Dialogs.Luis.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;

    public partial class Action
    {
        /// <summary>
        /// Initializes a new instance of the Action class.
        /// </summary>
        public Action()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Action class.
        /// </summary>
        public Action(bool? triggered = default(bool?), string name = default(string), IList<ActionParameter> parameters = default(IList<ActionParameter>))
        {
            Triggered = triggered;
            Name = name;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets or sets if the Luis action is triggered, false otherwise.
        /// </summary>
        /// <value>
        /// True if the Luis action is triggered, false otherwise.
        /// </value>
        [JsonProperty(PropertyName = "triggered")]
        public bool? Triggered { get; set; }

        /// <summary>
        /// Gets or sets the Name of the action.
        /// </summary>
        /// <value>
        /// Name of the action.
        /// </value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameters for the action.
        /// </summary>
        /// <value>
        /// The parameters for the action.
        /// </value>
        [JsonProperty(PropertyName = "parameters")]
        public IList<ActionParameter> Parameters { get; set; }
    }
}

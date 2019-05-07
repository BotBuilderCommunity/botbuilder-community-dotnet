namespace Bot.Builder.Community.Dialogs.Luis.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;

    public partial class ActionParameter
    {
        /// <summary>
        /// Initializes a new instance of the ActionParameter class.
        /// </summary>
        public ActionParameter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ActionParameter class.
        /// </summary>
        public ActionParameter(string name = default(string), bool? required = default(bool?), IList<EntityRecommendation> value = default(IList<EntityRecommendation>))
        {
            Name = name;
            Required = required;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        /// <value>
        /// The name of the parameter.
        /// </value>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the parameter is required, false otherwise.
        /// </summary>
        /// <value>
        /// A flag indicating if the parameter is required, false otherwise.
        /// </value>
        [JsonProperty(PropertyName = "required")]
        public bool? Required { get; set; }

        /// <summary>
        /// Gets or sets the value of extracted entities for this parameter.
        /// </summary>
        /// <value>
        /// The value of extracted entities for this parameter.
        /// </value>
        [JsonProperty(PropertyName = "value")]
        public IList<EntityRecommendation> Value { get; set; }
    }
}

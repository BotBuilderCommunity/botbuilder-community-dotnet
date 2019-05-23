namespace Bot.Builder.Community.Dialogs.Luis.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Child entity in Luis composite entity.
    /// </summary>
    public partial class CompositeChild
    {
        /// <summary>
        /// Initializes a new instance of the CompositeChild class.
        /// </summary>
        public CompositeChild()
        {
        }

        /// <summary>
        /// Initializes a new instance of the CompositeChild class.
        /// </summary>
        public CompositeChild(string type, string value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the type of child entity.
        /// </summary>
        /// <value>
        /// The type of child entity.
        /// </value>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value extracted by Luis.
        /// </summary>
        /// <value>
        /// The value extracted by Luis.
        /// </value>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Validate the object. Throws ValidationException if validation fails.
        /// </summary>
        public virtual void Validate()
        {
            if (Type == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Type");
            }

            if (Value == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Value");
            }
        }
    }
}

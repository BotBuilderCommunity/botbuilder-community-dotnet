namespace Bot.Builder.Community.Dialogs.Luis.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Luis composite entity. Look at https://www.luis.ai/Help for more
    /// information.
    /// </summary>
    public partial class CompositeEntity
    {
        /// <summary>
        /// Initializes a new instance of the CompositeEntity class.
        /// </summary>
        public CompositeEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the CompositeEntity class.
        /// </summary>
        public CompositeEntity(string parentType, string value, IList<CompositeChild> children)
        {
            ParentType = parentType;
            Value = value;
            Children = children;
        }

        /// <summary>
        /// Gets or sets the type of parent entity.
        /// </summary>
        /// <value>
        /// The type of parent entity.
        /// </value>
        [JsonProperty(PropertyName = "parentType")]
        public string ParentType { get; set; }

        /// <summary>
        /// Gets or sets the value for entity extracted by LUIS.
        /// </summary>
        /// <value>
        /// The value for entity extracted by LUIS.
        /// </value>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the Composite Children.
        /// </summary>
        /// <value>
        /// The Composite Children.
        /// </value>
        [JsonProperty(PropertyName = "children")]
        public IList<CompositeChild> Children { get; set; }

        /// <summary>
        /// Validate the object. Throws ValidationException if validation fails.
        /// </summary>
        public virtual void Validate()
        {
            if (ParentType == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "ParentType");
            }

            if (Value == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Value");
            }

            if (Children == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Children");
            }

            if (this.Children != null)
            {
                foreach (var element in this.Children)
                {
                    if (element != null)
                    {
                        element.Validate();
                    }
                }
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Facebook.FacebookEvents.Templates
{
    /// <summary>
    /// Represents an element of a template message.
    /// </summary>
    public class Element
    {
        /// <summary>
        /// Gets or sets the type of the media element.
        /// </summary>
        /// <value>The type of the media element.</value>
        [JsonProperty(PropertyName = "media_type")]
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the URL of the media element.
        /// </summary>
        /// <value>The URL of the media element.</value>
        [JsonProperty(PropertyName = "url")]
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets the title of the element.
        /// </summary>
        /// <value>The title of the element.</value>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the subtitle text of the element.
        /// </summary>
        /// <value>The subtitle of the element.</value>
        [JsonProperty(PropertyName = "subtitle")]
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets the URL of the image.
        /// </summary>
        /// <value>The URL of the image.</value>
        [JsonProperty(PropertyName = "image_url")]
        public Uri ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the default action for the element.
        /// </summary>
        /// <value>The default action for the element.</value>
        [JsonProperty(PropertyName = "default_action")]
        public DefaultAction DefaultAction { get; set; }

        /// <summary>
        /// Gets a list of buttons for the element.
        /// </summary>
        /// <value>The list of buttons for the element.</value>
        [JsonProperty(PropertyName = "buttons")]
        public List<Button> Buttons { get; } = new List<Button>();

        /// <summary>
        /// Newtonsoft JSON method for conditionally serializing the <see cref="Buttons"/> property.
        /// </summary>
        /// <returns>`true` to serialize the property; otherwise, `false`.</returns>
        public bool ShouldSerializeButtons()
        {
            return Buttons.Count > 0;
        }
    }
}

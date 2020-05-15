using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Request
{
    public class SigninArgumentExtension
    {
        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }

        public string Status { get; set; }
    }
}

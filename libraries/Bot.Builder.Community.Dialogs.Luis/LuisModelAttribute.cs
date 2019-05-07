namespace Bot.Builder.Community.Dialogs.Luis
{
    using System;

    /// <summary>
    /// The LUIS model information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    [Serializable]
    public class LuisModelAttribute : Attribute, ILuisModel, ILuisOptions, IEquatable<ILuisModel>
    {
        private string _modelId;
        private double _threshold;
        private string _subscriptionKey;
        private string _domain;
        private LuisApiVersion _apiVersion;
        private Uri _uriBase;

        /// <summary>
        /// Construct the LUIS model information.
        /// </summary>
        /// <param name="modelID">The LUIS model ID.</param>
        /// <param name="subscriptionKey">The LUIS subscription key.</param>
        /// <param name="apiVersion">The LUIS API version.</param>
        /// <param name="domain">Domain where LUIS model is located.</param>
        /// <param name="threshold">Threshold for the top scoring intent.</param>
        public LuisModelAttribute(
            string modelID,
            string subscriptionKey,
            LuisApiVersion apiVersion = LuisApiVersion.V2,
            string domain = null,
            double threshold = 0.0d)
        {
            SetField.NotNull(out this._modelId, nameof(modelID), modelID);
            SetField.NotNull(out this._subscriptionKey, nameof(subscriptionKey), subscriptionKey);
            this._apiVersion = apiVersion;
            this._domain = domain;
            this._uriBase = UriFor(apiVersion, domain);
            this._threshold = threshold;

            this.Log = true;
        }

        /// <summary>
        /// Gets or sets the GUID for the LUIS model.
        /// </summary>
        /// <value>
        /// The GUID for the LUIS model.
        /// </value>
        public string ModelID
        {
            get => _modelId;
            set => _modelId = value;
        }

        /// <summary>
        /// Gets or sets the subscription key for LUIS.
        /// </summary>
        /// <value>
        /// The subscription key for LUIS.
        /// </value>
        public string SubscriptionKey
        {
            get => _subscriptionKey;
            set => _subscriptionKey = value;
        }

        /// <summary>
        /// Gets or sets the domain where LUIS application is located.
        /// </summary>
        /// <remarks>Null means default which is api.projectoxford.ai for V1 API and westus.api.cognitive.microsoft.com for V2 api.</remarks>
        /// <value>
        /// The domain where LUIS application is located.
        /// </value>
        public string Domain
        {
            get => _domain;
            set => _domain = value;
        }

        /// <summary>
        /// Gets or sets the base URI for LUIS calls.
        /// </summary>
        /// <value>
        /// The base URI for LUIS calls.
        /// </value>
        public Uri UriBase
        {
            get => _uriBase;
            set => _uriBase = value;
        }

        /// <summary>
        /// Gets or sets the version of query API to call.
        /// </summary>
        /// <value>
        /// The version of query API to call.
        /// </value>
        public LuisApiVersion ApiVersion
        {
            get => _apiVersion;
            set => _apiVersion = value;
        }

        /// <summary>
        /// Gets or sets the Threshold for top scoring intent
        /// </summary>
        /// <value>
        /// The Threshold for top scoring intent
        /// </value>
        public double Threshold
        {
            get => _threshold;
            set => _threshold = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether logging of queries to LUIS is allowed.
        /// </summary>
        /// <value>
        /// A flag indicating if logging of queries to LUIS is allowed.
        /// </value>
        public bool Log
        {
            get => Options.Log ?? default(bool);
            set => Options.Log = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether spell checking is on or not.
        /// </summary>
        /// <value>
        /// A flag indicating if spell checking is on.
        /// </value>
        public bool SpellCheck
        {
            get => Options.SpellCheck ?? default(bool);
            set => Options.SpellCheck = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the staging endpoint should be used.
        /// </summary>
        /// <value>
        /// A flag indicating if the staging endpoint should be used.
        /// </value>
        public bool Staging
        {
            get => Options.Staging ?? default(bool);
            set => Options.Staging = value;
        }

        /// <summary>
        /// Gets or sets the time zone offset.
        /// </summary>
        /// <value>
        /// The time zone offset.
        /// </value>
        public double TimezoneOffset
        {
            get => Options.TimezoneOffset ?? default(double);
            set => Options.TimezoneOffset = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether verbose should be used.
        /// </summary>
        /// <value>
        /// The verbose flag.
        /// </value>
        public bool Verbose
        {
            get => Options.Verbose ?? default(bool);
            set => Options.Verbose = value;
        }

        /// <summary>
        /// Gets or sets the Bing Spell Check subscription key.
        /// </summary>
        /// <value>
        /// The Bing Spell Check subscription key.
        /// </value>
        public string BingSpellCheckSubscriptionKey
        {
            get => Options.BingSpellCheckSubscriptionKey;
            set => Options.BingSpellCheckSubscriptionKey = value;
        }

        bool? ILuisOptions.Log { get; set; }

        bool? ILuisOptions.SpellCheck { get; set; }

        bool? ILuisOptions.Staging { get; set; }

        double? ILuisOptions.TimezoneOffset { get; set; }

        bool? ILuisOptions.Verbose { get; set; }

        string ILuisOptions.BingSpellCheckSubscriptionKey { get; set; }

        private ILuisOptions Options => (ILuisOptions)this;

        public static Uri UriFor(LuisApiVersion apiVersion, string domain = null)
        {
            if (domain == null)
            {
                domain = apiVersion == LuisApiVersion.V2 ? "westus.api.cognitive.microsoft.com" : "api.projectoxford.ai/luis/v1/application";
            }

            return new Uri(apiVersion == LuisApiVersion.V2 ? $"https://{domain}/luis/v2.0/apps/" : $"https://api.projectoxford.ai/luis/v1/application");
        }

        public bool Equals(ILuisModel other)
        {
            return other != null
                && object.Equals(this.ModelID, other.ModelID)
                && object.Equals(this.SubscriptionKey, other.SubscriptionKey)
                && object.Equals(this.ApiVersion, other.ApiVersion)
                && object.Equals(this.UriBase, other.UriBase)
                ;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as ILuisModel);
        }

        public override int GetHashCode()
        {
            return ModelID.GetHashCode()
                ^ SubscriptionKey.GetHashCode()
                ^ UriBase.GetHashCode()
                ^ ApiVersion.GetHashCode();
        }

        public LuisRequest ModifyRequest(LuisRequest request)
        {
            Options.Apply(request);
            return request;
        }
    }
}

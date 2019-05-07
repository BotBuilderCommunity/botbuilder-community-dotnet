using System;

namespace Bot.Builder.Community.Dialogs.Luis
{
    /// <summary>
    /// Associate a LUIS intent with a dialog method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Serializable]
    public class LuisIntentAttribute : AttributeString
    {
#pragma warning disable SA1401 // Fields must be private
                              /// <summary>
                              /// The LUIS intent name.
                              /// </summary>
        public readonly string IntentName;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// Construct the association between the LUIS intent and a dialog method.
        /// </summary>
        /// <param name="intentName">The LUIS intent name.</param>
        public LuisIntentAttribute(string intentName)
        {
            SetField.NotNull(out this.IntentName, nameof(intentName), intentName);
        }

        protected override string Text
        {
            get
            {
                return this.IntentName;
            }
        }
    }
}

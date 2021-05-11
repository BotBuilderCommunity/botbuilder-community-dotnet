using System.ComponentModel.DataAnnotations;

namespace Bot.Builder.Community.Adapters.Alexa
{
    public class AlexaAdapterOptions
    {
        public bool TryConvertFirstActivityAttachmentToAlexaCard { get; set; } = false;

        public bool ValidateIncomingAlexaRequests { get; set; } = true;

        public bool ShouldEndSessionByDefault { get; set; } = true;

        /// <summary>
        /// The Skill Id for this bot (ie amzn1.ask.skill.{A GUID}).
        /// </summary>
        /// <remarks>
        /// This will be used to verify requests coming from Alexa are for this bot.
        /// See https://developer.amazon.com/en-US/docs/alexa/custom-skills/handle-requests-sent-by-alexa.html for more information including how to get this value.
        /// </remarks>
        [Required]
        public string AlexaSkillId { get; set; }
    }
}

using System;
namespace Bot.Builder.Community.Adapters.Alexa.Directives.Dialogs
{
    /// <summary>
    /// Sends Alexa a command to confirm the all the information the user has provided for the intent before the skill takes action.
    /// </summary>
    public class DialogConfirmIntentDirective : DialogDirective
    {
        public DialogConfirmIntentDirective(string intent, AlexaConfirmationState confirmationStatus)
            : base(intent, confirmationStatus) { }

        public string Type => "Dialog.ConfirmIntent";
    }
}

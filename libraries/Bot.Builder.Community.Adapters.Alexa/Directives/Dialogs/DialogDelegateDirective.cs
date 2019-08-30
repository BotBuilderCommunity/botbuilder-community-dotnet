using System;
namespace Bot.Builder.Community.Adapters.Alexa.Directives.Dialogs
{
    /// <summary>
    /// Sends Alexa a command to handle the next turn in the dialog with the user.
    /// </summary>
    public class DialogDelegateDirective : DialogDirective
    {
        public DialogDelegateDirective(string intent, AlexaConfirmationState confirmationStatus)
            : base(intent, confirmationStatus) { }

        public string Type => "Dialog.Delegate";
    }
}

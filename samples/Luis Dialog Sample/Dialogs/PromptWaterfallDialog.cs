namespace Luis_Dialog_Sample
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;

    /// <summary>
    /// A ComponentDialog implementation containing two waterfall steps.
    /// This dialog can be used to ask the user a question, and return the
    /// response to the consuming dialog.
    /// </summary>
    public class PromptWaterfallDialog : ComponentDialog
    {
        public PromptWaterfallDialog()
            : base(nameof(PromptWaterfallDialog))
        {
            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                this.AskQuestionStepAsync,
                this.ReturnResultStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            this.AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            this.AddDialog(new TextPrompt(nameof(TextPrompt)));

            // The initial child Dialog to run.
            this.InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = stepContext.Options as PromptDialogOptions;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(options.Prompt) }, cancellationToken);
        }

        private async Task<DialogTurnResult> ReturnResultStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = stepContext.Options as PromptDialogOptions;
            options.Result = stepContext.Result as string;

            return await stepContext.EndDialogAsync(new DialogTurnResult(DialogTurnStatus.Complete, options));
        }
    }

    /// <summary>
    /// Options sent to and returned by the PromptWaterfallDialog.
    /// </summary>
    public class PromptDialogOptions
    {
        /// <summary>
        /// Gets or sets the method to execute after the PromptWaterfallDialog completes
        /// </summary>
        public PromptReturnMethods ReturnMethod { get; set; }

        /// <summary>
        /// Gets or sets the text of the prompt to send the user.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Gets or sets the aanswer provided by the user in response to the Prompt
        /// </summary>
        public string Result { get; set; }
    }

    /// <summary>
    /// Methods available on the SimpleNoteDialog that can be executed
    /// after the PromptWaterfallDialog completes
    /// </summary>
    public enum PromptReturnMethods
    {
        After_DeleteTitlePrompt,
        After_TextPrompt,
        After_TitlePrompt
    }
}

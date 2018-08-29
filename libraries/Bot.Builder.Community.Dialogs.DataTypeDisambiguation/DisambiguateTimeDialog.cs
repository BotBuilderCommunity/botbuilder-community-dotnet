using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Dialogs.DataTypeDisambiguation
{
    public class DisambiguateTimeDialog : ComponentDialog
    {
        private DisambiguateTimeDialog() : base(Id)
        {
            var recognizer = new DateTimeRecognizer(Culture.English);
            var model = recognizer.GetDateTimeModel();

            AddDialog(new WaterfallDialog(Id, new WaterfallStep[]
            {
                async (dc, stepContext) =>
                {
                    await dc.Context.SendActivityAsync("What time?");
                    return EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    var stateWrapper = new DisambiguateTimeDialogStateWrapper(dc.ActiveDialog.State);

                    var value = model.Parse(dc.Context.Activity.Text).ParseRecognizer();

                    stateWrapper.Resolutions = value;

                    if (value.ResolutionType != Resolution.ResolutionTypes.Time &&
                        value.ResolutionType != Resolution.ResolutionTypes.TimeRange)
                    {
                        await dc.Context.SendActivityAsync("I don't understand. Please provide a time");
                        return await dc.ReplaceAsync(Id);
                    }

                    if (value.NeedsDisambiguation)
                    {
                        var amOrPmChoices = new List<Choice>(new[]
                            {new Choice() {Value = "AM"}, new Choice() {Value = "PM"}});
                        return await dc.PromptAsync("choicePrompt", new PromptOptions
                        {
                            Prompt = new Activity
                            {
                                Type = ActivityTypes.Message,
                                Text = "Is that AM or PM?"
                            },
                            Choices = amOrPmChoices
                        }).ConfigureAwait(false);
                    }

                    stateWrapper.Time = value.FirstOrDefault().Time1.Value;
                    return await dc.EndAsync(dc.ActiveDialog.State);
                },
                async (dc, stepContext) =>
                {
                    var stateWrapper = new DisambiguateTimeDialogStateWrapper(dc.ActiveDialog.State);
                    var amOrPmChoice = ((FoundChoice) stepContext.Result).Value;
                    var availableTimes = stateWrapper.Resolutions.Select(x => x.Time1.Value);
                    stateWrapper.Time = amOrPmChoice == "AM" ? availableTimes.Min() : availableTimes.Max();
                    return await dc.EndAsync(dc.ActiveDialog.State);
                }
            }));

            AddDialog(new TextPrompt("textPrompt"));
            AddDialog(new ChoicePrompt("choicePrompt", defaultLocale: "en"));
        }

        public static string Id => "disambiguateTimeDialog";

        public static DisambiguateTimeDialog Instance = new DisambiguateTimeDialog();
    }
}

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Dialogs.DataTypeDisambiguation
{
    public class DisambiguateDateDialog : ComponentDialog
    {
        private DisambiguateDateDialog() : base(Id)
        {
            var recognizer = new DateTimeRecognizer(Culture.English);
            var model = recognizer.GetDateTimeModel();

            AddDialog(new WaterfallDialog(Id, new WaterfallStep[]
            {
                async(dc, stepContext) =>
                {
                    await dc.Context.SendActivityAsync("What date?");
                    return EndOfTurn;
                },
                async (dc, stepContext) =>
                {
                    var stateWrapper = new DisambiguateDateDialogStateWrapper(dc.ActiveDialog.State);

                    var value = model.Parse(dc.Context.Activity.Text).ParseRecognizer();

                    stateWrapper.Resolutions = value;

                    if (value.ResolutionType != Resolution.ResolutionTypes.DateTime &&
                        value.ResolutionType != Resolution.ResolutionTypes.DateTimeRange)
                    {
                        await dc.Context.SendActivityAsync("I don't understand. Please provide a date");
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

                    stateWrapper.Date = value.FirstOrDefault().Date1.Value;
                    return await dc.EndAsync(dc.ActiveDialog.State);
                },
                async (dc, stepContext) =>
                {
                    var stateWrapper = new DisambiguateDateDialogStateWrapper(dc.ActiveDialog.State);
                    var amOrPmChoice = ((FoundChoice)stepContext.Result).Value;
                    var availableTimes = stateWrapper.Resolutions.Select(x=>x.Date1.Value);
                    stateWrapper.Date = amOrPmChoice  == "AM" ? availableTimes.Min() : availableTimes.Max();
                    return await dc.EndAsync(dc.ActiveDialog.State);
                }
            }));

            AddDialog(new TextPrompt("textPrompt"));
            AddDialog(new ChoicePrompt("choicePrompt", defaultLocale: "en"));
        }
        public static string Id => "disambiguateDateDialog";

        public static DisambiguateDateDialog Instance = new DisambiguateDateDialog();
    }
}

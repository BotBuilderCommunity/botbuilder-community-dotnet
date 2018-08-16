using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Builder.Community.Dialogs.DataTypeDisambiguation
{
    public class DisambiguateDateDialog : DialogContainer
    {
        private DisambiguateDateDialog() : base(Id)
        {
            var recognizer = new DateTimeRecognizer(Culture.English);
            var model = recognizer.GetDateTimeModel();

            Dialogs.Add(Id, new WaterfallStep[]
            {
                async(dc,args, next) =>{
                    await dc.Context.SendActivity("What date?");
                },
                async (dc, args, next) =>
                {
                    var stateWrapper = new DisambiguateDateDialogStateWrapper(dc.ActiveDialog.State);

                    var value = model.Parse(dc.Context.Activity.Text).ParseRecognizer();

                    stateWrapper.Resolutions = value;

                    if (value.ResolutionType != Resolution.ResolutionTypes.DateTime && value.ResolutionType != Resolution.ResolutionTypes.DateTimeRange)
                    {
                        await dc.Context.SendActivity("I don't understand. Please provide a date");
                        await dc.Replace(Id);
                    }
                    else if (value.NeedsDisambiguation)
                    {
                        var amOrPmChoices = new List<Choice>(new []{new Choice() { Value = "AM" }, new Choice() { Value = "PM" } });
                        await dc.Prompt("choicePrompt", "Is that AM or PM?", new ChoicePromptOptions
                        {
                            Choices = amOrPmChoices
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        stateWrapper.Date = value.FirstOrDefault().Date1.Value;
                        await dc.End(dc.ActiveDialog.State);
                    }                    
                },
                async (dc, args, next) =>
                {
                    var stateWrapper = new DisambiguateDateDialogStateWrapper(dc.ActiveDialog.State);
                    var amOrPmChoice = ((FoundChoice) args["Value"]).Value;
                    var availableTimes = stateWrapper.Resolutions.Select(x=>x.Date1.Value);
                    stateWrapper.Date = amOrPmChoice  == "AM" ? availableTimes.Min() : availableTimes.Max();
                    await dc.End(dc.ActiveDialog.State);
                }
            });

            Dialogs.Add("textPrompt", new TextPrompt());
            Dialogs.Add("choicePrompt", new ChoicePrompt("en"));
        }
        public static string Id => "disambiguateDateDialog";

        public static DisambiguateDateDialog Instance = new DisambiguateDateDialog();
    }
}

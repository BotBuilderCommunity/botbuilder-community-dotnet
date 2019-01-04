using Bot.Builder.Community.Dialogs.ChoiceFlow.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Dialogs.ChoiceFlow
{
    public class ChoiceFlowDialog : ComponentDialog
    {
        public const string DefaultDialogId = "ChoiceFlowDialog";

        private string _pathToChoiceFlowJson;
        private List<ChoiceFlowItem> _initialChoiceFlowItems;

        public ChoiceFlowDialog(string relativePathToChoiceFlowJson = null, List<ChoiceFlowItem> choiceFlowItems = null, IBotTelemetryClient telemetryClient = null, string dialogId = DefaultDialogId) : base(dialogId)
        {
            _pathToChoiceFlowJson = relativePathToChoiceFlowJson;
            _initialChoiceFlowItems = choiceFlowItems;

            TelemetryClient = telemetryClient;

            var guidedWaterfall = new WaterfallStep[]
            {
                LoadChoiceFlowItems,
                PromptUser,
                ActOnPromptResult
            };

            AddDialog(new WaterfallDialog(InitialDialogId ?? DefaultDialogId, guidedWaterfall) { TelemetryClient = telemetryClient });
            AddDialog(new ChoicePrompt(DialogIds.ChoiceFlowPrompt));
        }

        public async Task<DialogTurnResult> LoadChoiceFlowItems(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            if (sc.Options is ChoiceFlowDialogOptions options && options.ChoiceFlowItems.Any())
            {
                return await sc.NextAsync();
            }
            else
            {
                if (!string.IsNullOrEmpty(_pathToChoiceFlowJson))
                {
                    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _pathToChoiceFlowJson);
                    string json = File.ReadAllText(path);

                    try
                    {
                        var categories = JsonConvert.DeserializeObject<List<ChoiceFlowItem>>(json);
                        return await sc.ReplaceDialogAsync(this.Id, new ChoiceFlowDialogOptions() { ChoiceFlowItems = categories });
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else if (_initialChoiceFlowItems != null && _initialChoiceFlowItems.Any())
                {
                    return await sc.ReplaceDialogAsync(this.Id, new ChoiceFlowDialogOptions() { ChoiceFlowItems = _initialChoiceFlowItems });
                }
            }

            throw new Exception("No Choice Flow Items found. Please provide valid ChoiceFlow JSON or a list of ChoiceFlowItem");
        }

        public async Task<DialogTurnResult> PromptUser(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var options = sc.Options as ChoiceFlowDialogOptions;

            var selectedChoiceFlowItemId = options.SelectedChoiceFlowItem;
            var allCategories = options.ChoiceFlowItems.SelectRecursive(c => c.SubChoiceFlowItems).ToList();
            var selectedChoiceFlowItem = allCategories.FirstOrDefault(c => c.Id == options.SelectedChoiceFlowItem);
            var categoriesToPrompt = selectedChoiceFlowItem.SubChoiceFlowItems.ToList();
            
            if (categoriesToPrompt != null && categoriesToPrompt.Any())
            {
                return await sc.PromptAsync(DialogIds.ChoiceFlowPrompt, new PromptOptions()
                {
                    Prompt = sc.Context.Activity.CreateReply(selectedChoiceFlowItem.SubChoiceFlowItemsPrompt),
                    RetryPrompt = sc.Context.Activity.CreateReply(selectedChoiceFlowItem.SubChoiceFlowItemsRetryPrompt),
                    Choices = categoriesToPrompt.Select(c => new Choice
                    {
                        Value = c.Id.ToString(),
                        Synonyms = c.Synonyms,
                        Action = new CardAction(ActionTypes.ImBack, title: c.Name, value: c.Name),
                    }).ToList()
                });
            }
            else
            {
                if(!string.IsNullOrEmpty(selectedChoiceFlowItem.SimpleResponse))
                {
                    await sc.Context.SendActivityAsync(selectedChoiceFlowItem.SimpleResponse);
                }

                return await sc.EndDialogAsync(selectedChoiceFlowItem);
            }
        }

        public async Task<DialogTurnResult> ActOnPromptResult(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var choice = sc.Result as FoundChoice;
            var options = sc.Options as ChoiceFlowDialogOptions;
            options.SelectedChoiceFlowItem = Int32.Parse(choice.Value);
            return await sc.ReplaceDialogAsync(this.Id, options);
        }

        private class DialogIds
        {
            public const string ChoiceFlowPrompt = "choiceFlowPrompt";
        }
    }
}

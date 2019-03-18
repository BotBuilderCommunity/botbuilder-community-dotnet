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

        public ChoiceFlowDialog(string pathToChoiceFlowJson = null, List<ChoiceFlowItem> choiceFlowItems = null, IBotTelemetryClient telemetryClient = null, string dialogId = DefaultDialogId) : base(dialogId)
        {
            _pathToChoiceFlowJson = pathToChoiceFlowJson;
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
                List<ChoiceFlowItem> choiceFlowItems = null;

                if (!string.IsNullOrEmpty(_pathToChoiceFlowJson))
                {
                    try
                    {
                        string json = File.ReadAllText(_pathToChoiceFlowJson);
                        var categories = JsonConvert.DeserializeObject<List<ChoiceFlowItem>>(json);
                        choiceFlowItems = categories;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception ("Error deserializing ChoiceFlow JSON", ex);
                    }
                }
                else if (_initialChoiceFlowItems != null && _initialChoiceFlowItems.Any())
                {
                    choiceFlowItems = _initialChoiceFlowItems;
                }

                if (choiceFlowItems != null && choiceFlowItems.Any())
                {
                    var allChoiceFlowItems = choiceFlowItems.SelectRecursive(c => c.SubChoiceFlowItems).ToList();

                    var duplicateIds = allChoiceFlowItems.GroupBy(c => c.Id).Select(g => new
                    {
                        Id = g.Key,
                        Count = g.Count()
                    }).Where(g => g.Count > 1);

                    if(duplicateIds != null && duplicateIds.Any())
                    {
                        var errorMsg = $"Duplicate IDs detected in ChoiceFlow Items: ";

                        foreach(var id in duplicateIds)
                        {
                            errorMsg += $"{id.Id}, ";
                        }

                        throw new Exception(errorMsg);
                    }

                    return await sc.ReplaceDialogAsync(this.Id, new ChoiceFlowDialogOptions() { ChoiceFlowItems = choiceFlowItems });
                }
                else
                {
                    throw new Exception("No Choice Flow Items found. Please provide valid ChoiceFlow JSON or a list of ChoiceFlowItem");
                }
            }
        }

        public async Task<DialogTurnResult> PromptUser(WaterfallStepContext sc, CancellationToken cancellationToken)
        {
            var options = sc.Options as ChoiceFlowDialogOptions;

            var selectedChoiceFlowItemId = options.SelectedChoiceFlowItem;
            var allChoiceFlowItems = options.ChoiceFlowItems.SelectRecursive(c => c.SubChoiceFlowItems).ToList();
            var selectedChoiceFlowItem = allChoiceFlowItems.FirstOrDefault(c => c.Id == options.SelectedChoiceFlowItem);
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

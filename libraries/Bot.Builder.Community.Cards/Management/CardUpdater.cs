using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardUpdater<T>
        where T : BotState
    {
        private readonly string[] inputTypes = new[] { "Input.Text", "Input.Number", "Input.Date", "Input.Time", "Input.Toggle", "Input.ChoiceSet" };

        public CardUpdater(T botState, CardSet cardSet = null)
            : this(botState?.CreateProperty<CardUpdaterState>(nameof(CardUpdaterState)) ?? throw new ArgumentNullException(nameof(botState)), cardSet)
        {
        }

        public CardUpdater(IStatePropertyAccessor<CardUpdaterState> stateAccessor, CardSet cardSet = null)
        {
            StateAccessor = stateAccessor ?? throw new ArgumentNullException(nameof(stateAccessor));
            Cards = cardSet ?? new CardSet();
        }

        public IStatePropertyAccessor<CardUpdaterState> StateAccessor { get; }

        public CardSet Cards { get; }

        public async Task DeleteAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {

        }

        public async Task PreserveValuesAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var activity = turnContext?.Activity;
            var value = activity?.Value;

            if (value != null)
            {
                var payload = JObject.FromObject(value);

                foreach (var type in new[] { IdType.Action, IdType.Card })
                {
                    string id = payload.GetIdFromPayload(type);

                    if (id != null)
                    {
                        var state = await StateAccessor.GetNotNullAsync(turnContext, () => new CardUpdaterState(), cancellationToken);

                        if (state.ActivitiesByCard.TryGetValue(id, out var cardActivity))
                        {
                            var cardName = cardActivity.CardName;

                            if (cardName != null)
                            {
                                var cardAttachment = Cards.Find(cardActivity.CardName);

                                if (cardAttachment.ContentType == CardConstants.AdaptiveCardContentType && cardAttachment.Content != null)
                                {
                                    var card = JObject.FromObject(cardAttachment.Content);

                                    foreach (var input in card.NonDataDescendants()
                                        .Select(token => token as JObject)
                                        .Where(element => inputTypes.Contains((element[CardConstants.KeyType] as JProperty)?.Value?.ToString())
                                            && element[CardConstants.KeyId] != null
                                            && payload.ContainsKey(element[CardConstants.KeyId].ToString())))
                                    {
                                        input[CardConstants.KeyValue] = payload[input[CardConstants.KeyId].ToString()];
                                    }

                                    var update = MessageFactory.Attachment(cardAttachment);

                                    update.Conversation = activity.Conversation;
                                    update.Id = cardActivity.ActivityId;

                                    try
                                    {
                                        await turnContext.UpdateActivityAsync(update);
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}

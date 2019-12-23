using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management
{
    public class CardSet
    {
        private readonly IDictionary<string, CardAttachment> _cards = new Dictionary<string, CardAttachment>();

        public CardSet()
        {
        }

        public CardSet(IDictionary<string, Attachment> cards)
        {
            if (cards is null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            _cards = cards.ToDictionary(card => card.Key, card => new CardAttachment(card.Value));
        }

        public CardSet Add(string name, Attachment attachment)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (attachment is null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            _cards[name] = new CardAttachment(attachment);

            return this;
        }

        public CardSet AddAdaptiveCard(string name, object adaptiveCard)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (adaptiveCard is null)
            {
                throw new ArgumentNullException(nameof(adaptiveCard));
            }

            return Add(name, new Attachment(CardConstants.AdaptiveCardContentType, content: adaptiveCard));
        }

        public CardSet AddAdaptiveCard(string name, string adaptiveCardJson)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (adaptiveCardJson is null)
            {
                throw new ArgumentNullException(nameof(adaptiveCardJson));
            }

            try
            {
                var jObject = JObject.Parse(adaptiveCardJson);

                return AddAdaptiveCard(name, jObject);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Searches the current <see cref="CardSet"/> for a card attachment by its name.
        /// </summary>
        /// <param name="name">Name of the card attachment to search for.</param>
        /// <returns>The card attachment if found; otherwise <c>null</c>.</returns>
        public Attachment Find(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_cards.TryGetValue(name, out var result))
            {
                return result.Attachment;
            }
            
            return null;
        }

        public IDictionary<string, Attachment> GetCards()
        {
            return _cards.ToDictionary(card => card.Key, card => Find(card.Key));
        }
    }
}

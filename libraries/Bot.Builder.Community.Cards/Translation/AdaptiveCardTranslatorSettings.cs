using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Cards.Translation;

namespace Bot.Builder.Community.Cards.Translation
{
    public class AdaptiveCardTranslatorSettings
    {
        public IsArrayElementTranslatableDelegate IsArrayElementTranslatable { get; set; }

        public IsValueTranslatableDelegate IsValueTranslatable { get; set; }

        public string[] PropertiesToTranslate { get; set; }
    }
}

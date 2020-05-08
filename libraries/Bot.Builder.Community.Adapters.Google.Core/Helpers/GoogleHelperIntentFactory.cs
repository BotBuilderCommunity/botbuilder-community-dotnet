using System;
using System.Collections.Generic;
using System.Text;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;

namespace Bot.Builder.Community.Adapters.Google.Core.Helpers
{
    public static class GoogleHelperIntentFactory
    {
        public static ListIntent CreateListIntent(string title, List<OptionItem> items)
        {
            return new ListIntent()
            {
                InputValueData = new ListOptionIntentInputValueData()
                {
                    ListSelect = new OptionIntentSelect()
                    {
                        Title = "This is the list title",
                        Items = items
                    }
                }
            };
        }

        public static CarouselIntent CreateCarouselIntent(string title, List<OptionItem> items)
        {
            return new CarouselIntent()
            {
                InputValueData = new CarouselOptionIntentInputValueData()
                {
                    CarouselSelect = new OptionIntentSelect()
                    {
                        Title = title,
                        Items = items
                    }
                }
            };
        }
    }
}

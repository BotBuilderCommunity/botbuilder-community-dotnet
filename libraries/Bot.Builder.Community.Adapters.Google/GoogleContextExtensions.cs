using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google.Model;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.Google
{
    public static class GoogleContextExtensions
    {
        public static void GoogleSetChoicePromptHelper(this ITurnContext context, string title, List<GoogleOptionItem> items)
        {
            var optionSystemIntent = new OptionSystemIntent()
            {
                Data = new OptionSystemIntentData
                {
                    ListSelect = new OptionIntentListSelect
                    {
                        Title = title,
                    }
                }
            };

            foreach(var item in items)
            {
                optionSystemIntent.Data.ListSelect.Items = new List<OptionIntentSelectListItem>();

                optionSystemIntent.Data.ListSelect.Items.Add(
                                new OptionIntentSelectListItem()
                                {
                                    OptionInfo = new SelectListItemOptionInfo
                                    {
                                        Key = item.Key,
                                        Synonyms = item.Synonyms
                                    },
                                    Description = item.Description,
                                    Title = item.Title,
                                    Image = new SelectListItemOptionImage
                                    {
                                        Url = item.ImageUrl,
                                        AccessibilityText = item.ImageAccessibilityText
                                    }
                                });
            }

            context.TurnState.Add("systemIntent", optionSystemIntent);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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

            foreach (var item in items)
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

        public static void GoogleSetCard(this ITurnContext context, GoogleBasicCard card)
        {
            context.TurnState.Add("GoogleCard", card);
        }

        public static void GoogleSetCard(this ITurnContext context, string title,
            string subtitle, Image image, ImageDisplayOptions imageDisplayOptions, 
            string formattedText)
        {
            if (image == null && formattedText == null)
            {
                throw new Exception("A Basic Card should have either an Image or Formatted Text set");
            }

            // Fixed
            var card = new GoogleBasicCard()
            {
                Content = new GoogleBasicCardContent()
                {
                    Title = title,
                    Subtitle = subtitle,
                    FormattedText = formattedText,
                    Display = ImageDisplayOptions.DEFAULT,
                    Image = image
                },
            };

            // Just leaving this as commented to give developers a glimpse of implementation. 
            //var card = new GoogleBasicCard()
            //{
            //    Content = new GoogleBasicCardContent()
            //    {
            //        Title = "This is the card title",
            //        Subtitle = "This is the card subtitle",
            //        FormattedText = "This is some text to go into the card." +
            //                        "**This text should be bold** and " +
            //                        "*this text should be italic*.",
            //        Display = ImageDisplayOptions.DEFAULT,
            //        Image = new Image()
            //        {
            //            AccessibilityText = "This is the accessibility text",
            //            Url = "https://dev.botframework.com/Client/Images/ChatBot-BotFramework.png"
            //        },
            //    },
            //};

            context.TurnState.Add("GoogleCard", card);
        }

        public static void GoogleAddSuggestionChipsToResponse(this ITurnContext context, List<Suggestion> suggestionChips)
        {
            context.TurnState.Add("GoogleSuggestionChips", suggestionChips);
        }

        public static void GoogleSetMediaResponse(this ITurnContext context, MediaResponse mediaResponse)
        {
            context.TurnState.Add("GoogleMediaResponse", mediaResponse);
        }

        public static void GoogleSetAudioResponse(this ITurnContext context,
            string audioUrl, string name, string description = null,
            Image icon = null, Image largeImage = null)
        {
            var mediaResponse = new MediaResponse()
            {
                Content = new MediaResponseContent()
                {
                    MediaType = MediaType.AUDIO,
                    MediaObjects = new MediaObject[]
                    {
                        new MediaObject()
                        {
                            ContentUrl = audioUrl,
                            Description = description,
                            Name = name,
                            Icon = icon,
                            LargeImage = largeImage,
                        },
                    },
                },
            };

            context.GoogleSetMediaResponse(mediaResponse);
        }

        public static Payload GetGoogleRequestPayload(this ITurnContext context)
        {
            try
            {
                return (Payload)context.Activity.ChannelData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<string> GoogleGetSurfaceCapabilities(this ITurnContext context)
        {
            var payload = (Payload)context.Activity.ChannelData;
            var capabilities = payload.Surface.Capabilities.Select(c => c.Name);
            return capabilities.ToList();
        }
    }
}

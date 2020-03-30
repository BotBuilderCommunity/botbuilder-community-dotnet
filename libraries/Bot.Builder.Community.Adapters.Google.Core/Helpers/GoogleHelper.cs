using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Adapters.Google.Core.Model.Attachments;
using Bot.Builder.Community.Adapters.Google.Model;
using Bot.Builder.Community.Adapters.Google.Model.Attachments;
using JWT.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Core.Helpers
{
    public static class GoogleHelper
    {
        internal static string GetProjectIdFromRequest(HttpRequest httpRequest)
        {
            if (httpRequest.Headers.ContainsKey("Authorization"))
            {
                var payload = new JwtBuilder().Decode(httpRequest.Headers["Authorization"]);
                var payloadJObj = JObject.Parse(payload);
                return (string) payloadJObj["aud"];
            }

            return null;
        }

        internal static bool ValidateRequest(HttpRequest httpRequest, string actionProjectId)
        {
            return GetProjectIdFromRequest(httpRequest).ToLowerInvariant() == actionProjectId.ToLowerInvariant();
        }

        internal static string StripInvocation(string query, string invocationName)
        {
            if (query != null && (query.ToLower().StartsWith("talk to") || query.ToLower().StartsWith("speak to")
                                                      || query.ToLower().StartsWith("i want to speak to") ||
                                                      query.ToLower().StartsWith("ask")))
            {
                query = query.ToLower().Replace($"talk to", string.Empty);
                query = query.ToLower().Replace($"speak to", string.Empty);
                query = query.ToLower().Replace($"I want to speak to", string.Empty);
                query = query.ToLower().Replace($"ask", string.Empty);
            }

            query = query?.TrimStart().TrimEnd();

            if (!string.IsNullOrEmpty(invocationName)
                && query.ToLower().StartsWith(invocationName.ToLower()))
            {
                query = query.ToLower().Replace(invocationName.ToLower(), string.Empty);
            }

            return query?.TrimStart().TrimEnd();
        }

        internal static List<Suggestion> ConvertSuggestedActivitiesToSuggestionChips(SuggestedActions suggestedActions)
        {
            var suggestionChips = new List<Suggestion>();

            if (suggestedActions != null && suggestedActions.Actions != null && suggestedActions.Actions.Any())
            {
                foreach (var suggestion in suggestedActions.Actions)
                {
                    suggestionChips.Add(new Suggestion { Title = suggestion.Title });
                }
            }

            return suggestionChips;
        }

        internal static OptionIntentData GetOptionIntentDataFromListAttachment(ListAttachment listAttachment)
        {
            switch (listAttachment.ListStyle)
            {
                case ListAttachmentStyle.Carousel:
                    return new CarouselOptionIntentData
                    {
                        CarouselSelect = new OptionIntentSelect()
                        {
                            Items = listAttachment.Items,
                            Title = listAttachment.Title
                        }
                    };
                case ListAttachmentStyle.List:
                    return new ListOptionIntentData
                    {
                        ListSelect = new OptionIntentSelect()
                        {
                            Items = listAttachment.Items,
                            Title = listAttachment.Title
                        }
                    };
                default:
                    return null;
            }
        }
    }
}

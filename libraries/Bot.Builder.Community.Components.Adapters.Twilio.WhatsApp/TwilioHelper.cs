using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Adapters.Twilio.WhatsApp
{
    public class TwilioHelper
    {
        private static string ChannelsTwilio = "Twilio-WhatsApp";
        public static Activity PayloadToActivity(Dictionary<string, string> payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var twilioMessage =
                JsonConvert.DeserializeObject<TwilioWhatsAppMessage>(JsonConvert.SerializeObject(payload));

            var activity = new Activity
            {
                Id = twilioMessage.MessageSid,
                Timestamp = DateTime.UtcNow,
                ChannelId = ChannelsTwilio,
                Conversation = new ConversationAccount()
                {
                    Id = twilioMessage.From ?? twilioMessage.Author,
                },
                From = new ChannelAccount()
                {
                    Id = twilioMessage.From ?? twilioMessage.Author,
                },
                Recipient = new ChannelAccount()
                {
                    Id = twilioMessage.To,
                },
                Text = twilioMessage.Body,
                ChannelData = twilioMessage,
                Attachments = int.TryParse(twilioMessage.NumMedia, out var numMediaResult) &&
                              numMediaResult > 0
                    ? GetMessageAttachments(numMediaResult, payload)
                    : null
            };

            if (!string.IsNullOrEmpty(twilioMessage.Latitude) && !string.IsNullOrEmpty(twilioMessage.Longitude))
            {

                if (activity.Entities == null)
                    activity.Entities = new List<Entity>();

                activity.Entities.Add(new GeoCoordinates()
                {
                    Latitude = Convert.ToDouble(twilioMessage.Latitude),
                    Longitude = Convert.ToDouble(twilioMessage.Longitude),
                    Name = twilioMessage.Address
                });
            }

            activity.Type = TwilioActivityType(twilioMessage.SmsStatus);

            return activity;
        }

        /// <summary>
        /// Writes the HttpResponse.
        /// </summary>
        /// <param name="response">The httpResponse.</param>
        /// <param name="code">The status code to be written.</param>
        /// <param name="text">The text to be written.</param>
        /// <param name="encoding">The encoding for the text.</param>
        /// <param name="cancellationToken">A cancellation token for the task.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WriteAsync(HttpResponse response, int code, string text, Encoding encoding,
            CancellationToken cancellationToken)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            response.ContentType = "text/plain";
            response.StatusCode = code;

            var data = encoding.GetBytes(text);

            await response.Body.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts a query string to a dictionary with key-value pairs.
        /// </summary>
        /// <param name="query">The query string to convert.</param>
        /// <returns>A dictionary with the query values.</returns>
        public static Dictionary<string, string> QueryStringToDictionary(string query)
        {
            var values = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(query))
            {
                return values;
            }

            var pairs = query.Replace("+", "%20").Split('&');

            foreach (var p in pairs)
            {
                var pair = p.Split('=');
                var key = pair[0];
                var value = Uri.UnescapeDataString(pair[1]);

                values.Add(key, value);
            }

            return values;
        }

        /// <summary>
        /// Gets attachments from a Twilio message.
        /// </summary>
        /// <param name="numMedia">The number of media items to pull from the message body.</param>
        /// <param name="message">A dictionary containing the Twilio message elements.</param>
        /// <returns>An Attachments array with the converted attachments.</returns>
        public static List<Attachment> GetMessageAttachments(int numMedia, Dictionary<string, string> message)
        {
            var attachments = new List<Attachment>();
            for (var i = 0; i < numMedia; i++)
            {
                // Ensure MediaContentType and MediaUrl are present before adding the attachment
                if (message.ContainsKey($"MediaContentType{i}") && message.ContainsKey($"MediaUrl{i}"))
                {
                    var attachment = new Attachment()
                    {
                        ContentType = message[$"MediaContentType{i}"],
                        ContentUrl = message[$"MediaUrl{i}"],
                    };
                    attachments.Add(attachment);
                }
            }

            return attachments;
        }

        /// <summary>
        ///  Find activity type
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string TwilioActivityType(string message)
        {
            var activityType = ActivityTypes.Message;
            
            if (string.IsNullOrEmpty(message)) return activityType;

            message = message.ToLower();

            switch (message)
            {
                case "sent":
                    activityType = TwilioWhatsAppActivityTypes.MessageSent;
                    break;
                case "received":
                    activityType = ActivityTypes.Message;
                    break;
                case "delivered":
                    activityType = TwilioWhatsAppActivityTypes.MessageDelivered;
                    break;
                case "read":
                    activityType = TwilioWhatsAppActivityTypes.MessageRead;
                    break;
            }

            return activityType;
        }

        /// <summary>
        /// Bot Activity type convert to Twilio message format
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="twilioNumber"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static TwilioMessageOptions ActivityToTwilio(Activity activity, string twilioNumber = null)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (string.IsNullOrWhiteSpace(twilioNumber) && string.IsNullOrWhiteSpace(activity.From?.Id))
            {
                throw new ArgumentException($"Either {nameof(twilioNumber)} or {nameof(activity.From.Id)} must be provided.");
            }

            object location = string.Empty;

            var mediaUrls = new List<Uri>();
            if (activity.Attachments != null)
            {
                foreach (var attachment in activity.Attachments)
                {
                    if (attachment.Name == "location")
                    {
                        location = attachment.Content;
                    }
                }

                mediaUrls.AddRange(activity.Attachments.Select(attachment => new Uri(attachment.ContentUrl)));
            }

            var messageOptions = new TwilioMessageOptions()
            {
                To = activity.Conversation.Id,
                ApplicationSid = activity.Conversation.Id,
                From = twilioNumber ?? activity.From.Id,
                Body = activity.Text
            };

            messageOptions.MediaUrl.AddRange(mediaUrls);

            messageOptions.Location = location;

            return messageOptions;
        }

        public static Dictionary<string, string> QueryStringToDictionary(IFormCollection httpRequestForm)
        {
            var values = new Dictionary<string, string>();

            if (httpRequestForm == null)
                return values;

            foreach (var form in httpRequestForm)
            {
                values.Add(form.Key, form.Value);
            }

            return values;
        }
    }
}
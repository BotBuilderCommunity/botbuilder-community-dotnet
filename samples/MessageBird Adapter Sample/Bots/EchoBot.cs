// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.10.3

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace MessageBird_Adapter_Sample.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string _channel = "";
            //this is how you can notice that this message is coming from MessageBird WhatsApp Adapter
            if (turnContext.Activity.ChannelId.StartsWith("whatsapp"))
            {
                _channel = "whatsapp";
            }

            switch (turnContext.Activity.Text.ToLower())
            {
                case "location":
                    {
                        var reply = MessageFactory.Text("Location");
                        reply.Entities = new List<Entity>() { new GeoCoordinates() { Latitude = 41.0572, Longitude = 29.0433 } };
                        await turnContext.SendActivityAsync(reply);
                        break;
                    }
                case "file":
                    {
                        var reply = MessageFactory.Text("File");
                        Attachment attachment = new Attachment();
                        attachment.ContentType = "application/pdf";
                        attachment.ContentUrl = "https://qconlondon.com/london-2017/system/files/presentation-slides/microsoft_bot_framework_best_practices.pdf";
                        attachment.Name = "Microsoft Bot Framework Best Practices";
                        reply.Attachments = new List<Attachment>() { attachment };
                        await turnContext.SendActivityAsync(reply);
                        break;
                    }
                case "image":
                    {
                        var reply = MessageFactory.Text("Image");
                        Attachment attachment = new Attachment();
                        attachment.Name = "architecture-resize.png";
                        attachment.ContentType = "image/png";
                        attachment.ContentUrl = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png";
                        reply.Attachments = new List<Attachment>() { attachment };
                        await turnContext.SendActivityAsync(reply);
                        break;
                    }
                default:
                    {
                        var replyText = $"Echo: {turnContext.Activity.Text}";
                        await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);

                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Hello. Type the message you want to receive.\r\n");
                        sb.AppendLine($"*location* for location\r\n");
                        sb.AppendLine($"*file* for file\r\n");
                        sb.AppendLine($"*image* for image\r\n");
                        sb.AppendLine($"anything else to echo back!\r\n");

                        var reply = MessageFactory.Text(sb.ToString());
                        await turnContext.SendActivityAsync(reply);
                        break;
                    }
            }

        }
        /// <summary>
        /// this is how you receive delivery notification after sending the message
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Event)
            {
                if (turnContext.Activity.Name == "delivery")
                {
                    //turnContext.Activity.Id is the message id, you can use this parameter if you want to track delivery status
                }
            }
            return base.OnEventActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}

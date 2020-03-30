using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Alexa.NET.Response.Directive.Templates;
using Alexa.NET.Response.Directive.Templates.Types;
using Bot.Builder.Community.Adapters.Alexa.Core.Attachments;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Samples.Google.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.Text.ToLower())
            {
                case "finish":
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Ok, I won't ask anymore."), cancellationToken);
                    break;

                case "card":
                    var activityWithCard = MessageFactory.Text($"Ok, I included a simple card.");
                    activityWithCard.Attachments.Add(
                        new SimpleCard()
                            {
                                Title = "This is a simple card",
                                Content = "This is the simple card content"
                            }.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithCard, cancellationToken);
                    break;

                case "display":
                    var activityWithDisplayDirective = MessageFactory.Text($"Ok, I included a display directive.");
                    activityWithDisplayDirective.Attachments.Add(
                            new DisplayRenderTemplateDirective()
                            {
                                Template = new BodyTemplate1()
                                {
                                    BackgroundImage = new TemplateImage()
                                    {
                                        ContentDescription = "Test",
                                        Sources = new List<ImageSource>()
                                        {
                                            new ImageSource()
                                            {
                                                Url = "https://via.placeholder.com/576.png/09f/fff",
                                            }
                                        }
                                    },
                                    Content = new TemplateContent()
                                    {
                                        Primary = new TemplateText() { Text = "Test", Type = "PlainText" }
                                    },
                                    Title = "Test title",
                                }
                            }.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithDisplayDirective, cancellationToken);
                    break;

                case "hint":
                    var activityWithHintDirective = MessageFactory.Text($"Ok, I included a hint directive`.");
                    activityWithHintDirective.Attachments.Add(
                        new HintDirective()
                            {
                                Hint = new Hint("This is a hint")
                            }.ToAttachment());
                    await turnContext.SendActivityAsync(activityWithHintDirective, cancellationToken);
                    break;

                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}. What's next?", inputHint: InputHints.ExpectingInput), cancellationToken);
                    break;
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(
                $"Event received. Name: {turnContext.Activity.Name}. Value: {turnContext.Activity.Value}").ConfigureAwait(false);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
}

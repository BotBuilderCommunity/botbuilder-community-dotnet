using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace FormFlow_Sample.Dialogs
{
    [Serializable]
    public class MenuDialog : Dialog
    {
        public MenuDialog() : base(nameof(MenuDialog))
        {
        }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var resultMessage = outerDc.Context.Activity.CreateReply();
            resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            resultMessage.Attachments = new List<Attachment>();            
            resultMessage.Attachments.Add(GetMenuCard().ToAttachment());
            await outerDc.Context.SendActivityAsync(resultMessage);

            return new DialogTurnResult(DialogTurnStatus.Complete );

        }

        private HeroCard GetMenuCard()
        {
            return new HeroCard()
            {
                Title = "Demonstrating FormFlow in v4",
                Subtitle = "(try one of these)",
                Buttons = new List<CardAction>()
                        {
                           GetMenuOption("Hotels Dialog", nameof(HotelsDialog)),
                           GetMenuOption("Simple Sandwich", nameof(SandwichOrder)),
                           GetMenuOption("Builder Sandwich", nameof(BuilderSandwich)),
                           GetMenuOption("Pizza Order", nameof(PizzaOrder)),
                           GetMenuOption("Process Order", nameof(Order)),
                           GetMenuOption("Upload Images", nameof(ImagesForm)),
                           GetMenuOption("Schedule Callback", nameof(ScheduleCallbackDialog)),
                        }
            };
        }
        private CardAction GetMenuOption(string title, string value)
        {
            return new CardAction()
            {
                Title = title,
                Type = ActionTypes.ImBack,
                Value = value
            };
        }
    }
}

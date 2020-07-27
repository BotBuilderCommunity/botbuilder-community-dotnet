using AdaptiveCards;
using Bot.Builder.Community.Adapters.Zoom.Attachments;
using Bot.Builder.Community.Adapters.Zoom.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Teams_Zoom_Sample.Common;
using Teams_Zoom_Sample.Models;

namespace Teams_Zoom_Sample.Dialogs
{
    public class AppointmentDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<Appointment> _appointmentAccessor;

        public AppointmentDialog(UserState userState)
            : base(nameof(AppointmentDialog))
        {
            _appointmentAccessor = userState.CreateProperty<Appointment>("Appointment");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                CarTypeStepAsync,
                PackageTypeAsync,
                DateStepAsync,
                TimeStepAsync,
                NameStepAsync,
                PhoneStepAsync,
                ConfirmStepAsync,
                SummaryStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CarTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.ChannelId == "zoom")
            {

                var message = MessageFactory.SuggestedActions(
                    new List<CardAction>() {
                                        new CardAction(text: "Sedan", displayText: "Sedan", type: ActionTypes.ImBack),
                                        new CardAction(text: "Station Wagon", displayText: "Station Wagon", type: ActionTypes.ImBack),
                                        new CardAction(text: $"SUV / VAN", displayText: $"SUV / VAN", type: ActionTypes.ImBack)
                    }, "Please choose the type of your vehicle.");

                await stepContext.Context.SendActivityAsync(message, cancellationToken);

                return Dialog.EndOfTurn;
            }
            else
            {
                return await stepContext.PromptAsync(nameof(ChoicePrompt),
              new PromptOptions
              {
                  Prompt = MessageFactory.Text("Please choose the type of your vehicle."),
                  Choices = ChoiceFactory.ToChoices(new List<string> { "Sedan", "Station Wagon", "SUV / VAN" }),
              }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> PackageTypeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.ChannelId == "zoom")
            {
                stepContext.Values["carType"] = stepContext.Result;
                var message = MessageFactory.SuggestedActions(
                    new List<CardAction>() {
                                        new CardAction(text: "$25 - Express Wash", displayText: "$25 - Express Wash", type: ActionTypes.ImBack),
                                        new CardAction(text: "$35 - Signature Wash", displayText: "$35 - Signature Wash", type: ActionTypes.ImBack),
                                        new CardAction(text: "$45 - Premium Wash", displayText: "$45 - Premium Wash", type: ActionTypes.ImBack)
                    }, "Please choose the type of your vehicle.");

                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }
            else
            {
                stepContext.Values["carType"] = ((FoundChoice)stepContext.Result).Value;

                var message = Activity.CreateMessageActivity();
                message.Attachments = Common.Helper.GetPackagesCard();
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> DateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["packageType"] = stepContext.Result;

            if(stepContext.Context.Activity.ChannelId == "zoom")
            {
                var message = MessageFactory.SuggestedActions(
                    new List<CardAction>() {
                                        new CardAction(text: "12th May", displayText: "12th May", type: ActionTypes.ImBack),
                                        new CardAction(text: "13th May", displayText: "13th May", type: ActionTypes.ImBack),
                                        new CardAction(text: "14th May", displayText: "14th May", type: ActionTypes.ImBack)
                    }, "Thanks! When do you want to book it?");

                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }
            else
            {
                var message = Activity.CreateMessageActivity();
                message.Attachments = Common.Helper.GetDateTimeCard(true);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> TimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(stepContext.Context.Activity.ChannelId == "zoom")
            {
                stepContext.Values["date"] = stepContext.Result;

                var message = MessageFactory.SuggestedActions(
                  new List<CardAction>() {
                                        new CardAction(text: "12:00", displayText: "12:00", type: ActionTypes.ImBack),
                                        new CardAction(text: "13:00", displayText: "13:00", type: ActionTypes.ImBack),
                                        new CardAction(text: "14:00", displayText: "14:00", type: ActionTypes.ImBack)
                  }, "What is your preferred time slot?");

                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }
            else
            {
                stepContext.Values["date"] = JsonConvert.DeserializeObject<Data>(stepContext.Context.Activity.Value.ToString()).dateTimeValue;

                var message = Activity.CreateMessageActivity();
                message.Attachments = Common.Helper.GetDateTimeCard(false);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Context.Activity.ChannelId == "zoom")
                stepContext.Values["time"] = stepContext.Result;
            else
                stepContext.Values["time"] = JsonConvert.DeserializeObject<Data>(stepContext.Context.Activity.Value.ToString()).dateTimeValue;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Noted. Please enter your name.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = stepContext.Result;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Lastly, your mobile number please?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phoneNumber"] = stepContext.Result;

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Thanks! Should I book it for you?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // Get the current profile object from user state.
                var appointment = await _appointmentAccessor.GetAsync(stepContext.Context, () => new Appointment(), cancellationToken);

                appointment.Name = (string)stepContext.Values["name"];
                appointment.Phone = (string)stepContext.Values["phoneNumber"];
                appointment.CarType = (string)stepContext.Values["carType"];
                appointment.PackageType = (string)stepContext.Values["packageType"];
                appointment.Date = (string)stepContext.Values["date"];
                appointment.Time = (string)stepContext.Values["time"];

                var message = $"Thanks {appointment.Name}, your appointment for {appointment.PackageType} has been booked. On {appointment.Date}, you're requested to be 15 minutes before {appointment.Time}";

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sure! Thank you for being with me."), cancellationToken);
            }

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}

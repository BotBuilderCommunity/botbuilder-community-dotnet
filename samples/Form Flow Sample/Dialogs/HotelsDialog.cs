// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Bot.Builder.Community.Dialogs.FormFlow;
using Microsoft.Bot.Schema;
using FormFlow_Sample.Models;

namespace FormFlow_Sample.Dialogs
{
    /// <summary>
    /// Ported from: https://github.com/Microsoft/BotBuilder-Samples/blob/6c3d09c92ebeaf12ee7597a11bb09ac6e2fca6e5/CSharp/core-MultiDialogs/Dialogs/HotelsDialog.cs
    /// </summary>
    [Serializable]
    public class HotelsDialog : ComponentDialog
    {
        public HotelsDialog() : base(nameof(HotelsDialog))
        {
            var hotelsFormDialog = FormDialog.FromForm(this.BuildHotelsForm, FormOptions.PromptInStart);
            base.AddDialog(hotelsFormDialog);
        }
        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext outerDc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
             await outerDc.Context.SendActivityAsync("Welcome to the Hotels finder!");

            return base.BeginDialogAsync(outerDc, options, cancellationToken).Result;
        }

        public async override Task<DialogTurnResult> ContinueDialogAsync(DialogContext outerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            DialogTurnResult turnResult = null;
            try
            {
                turnResult = await base.ContinueDialogAsync(outerDc, cancellationToken);
                if (turnResult.Status == DialogTurnStatus.Complete)
                {
                    await ResumeAfterHotelsFormDialog(outerDc, turnResult.Result as HotelsQuery);
                }
            }
            catch (FormCanceledException<HotelsQuery>)
            {
                //user cancelled, ignore the exception
                await outerDc.Context.SendActivityAsync("Okay, cancelled.");
                turnResult = new DialogTurnResult(DialogTurnStatus.Cancelled);
                await outerDc.EndDialogAsync(cancellationToken);
            }
            return turnResult;
        }

        private IForm<HotelsQuery> BuildHotelsForm()
        {
            OnCompletionAsyncDelegate<HotelsQuery> processHotelsSearch = async (context, state) =>
            {
                await context.Context.SendActivityAsync($"Ok. Searching for Hotels in {state.Destination} from {state.CheckIn.ToString("MM/dd")} to {state.CheckIn.AddDays(state.Nights).ToString("MM/dd")}...");
            };

            return new FormBuilder<HotelsQuery>()
                .Field(nameof(HotelsQuery.Destination))
                .Message("Looking for hotels in {Destination}...")
                .AddRemainingFields()
                .OnCompletion(processHotelsSearch)
                .Build();
        }

        private async Task ResumeAfterHotelsFormDialog(DialogContext context, HotelsQuery searchQuery)
        {
            try
            {
                var hotels = await this.GetHotelsAsync(searchQuery);

                await context.Context.SendActivityAsync($"I found in total {hotels.Count()} hotels for your dates:");

                var resultMessage = context.Context.Activity.CreateReply();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var hotel in hotels)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = hotel.Name,
                        Subtitle = $"{hotel.Rating} starts. {hotel.NumberOfReviews} reviews. From ${hotel.PriceStarting} per night.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = hotel.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/search?q=hotels+in+" + HttpUtility.UrlEncode(hotel.Location)
                            }
                        }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }

                await context.Context.SendActivityAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation. Quitting from the HotelsDialog";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.Context.SendActivityAsync(reply);
            }
            finally
            {
                await context.EndDialogAsync(null);
            }
        }

        private async Task<IEnumerable<Hotel>> GetHotelsAsync(HotelsQuery searchQuery)
        {
            var hotels = new List<Hotel>();

            // Filling the hotels results manually just for demo purposes
            for (int i = 1; i <= 5; i++)
            {
                var random = new Random(i);
                Hotel hotel = new Hotel()
                {
                    Name = $"{searchQuery.Destination} Hotel {i}",
                    Location = searchQuery.Destination,
                    Rating = random.Next(1, 5),
                    NumberOfReviews = random.Next(0, 5000),
                    PriceStarting = random.Next(80, 450),
                    Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=Hotel+{i}&w=500&h=260"
                };

                hotels.Add(hotel);
            }

            hotels.Sort((h1, h2) => h1.PriceStarting.CompareTo(h2.PriceStarting));

            return hotels;
        }
    }
}

using AdaptiveCards;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teams_Zoom_Sample.Common
{
    public class Helper
    {
        // Helper Method to modify the Adaptive Card template. 
        static AdaptiveCard GetPackageAdaptiveCard(string packageName, string packagePrice, string packageDescription)
        {
            string json = System.IO.File.ReadAllText(@"Resources\\carPackages.json");
            var card = AdaptiveCards.AdaptiveCard.FromJson(json).Card;
            AdaptiveColumnSet adaptiveColumnSet = (AdaptiveColumnSet)card.Body[0];

            AdaptiveSubmitAction adaptiveSubmitAction = (AdaptiveSubmitAction)card.Actions[1];
            adaptiveSubmitAction.Data = packageName;

            // Get Column
            AdaptiveColumn adaptiveColumn = (AdaptiveCards.AdaptiveColumn)adaptiveColumnSet.Columns[0];

            // Set Tag Line
            AdaptiveTextBlock taglineBlock = (AdaptiveTextBlock)adaptiveColumn.Items[0];
            taglineBlock.Text = packageDescription;

            // Set Package Name
            AdaptiveTextBlock packageBlock = (AdaptiveTextBlock)adaptiveColumn.Items[1];
            packageBlock.Text = packageName;

            // Set Package Price
            AdaptiveTextBlock priceBlock = (AdaptiveTextBlock)adaptiveColumn.Items[2];
            priceBlock.Text = packagePrice;

            return card;
        }

        static AdaptiveCard GetDateAdaptiveCard(DateTime dateTime)
        {
            string json = System.IO.File.ReadAllText(@"Resources\\carBookingDate.json");
            var card = AdaptiveCards.AdaptiveCard.FromJson(json).Card;
            AdaptiveContainer adaptiveContainer = (AdaptiveContainer)card.Body[1];
            AdaptiveDateInput adaptiveDateInput = (AdaptiveDateInput)adaptiveContainer.Items[0];
            adaptiveDateInput.Value = dateTime.ToString("yyyy-MM-dd");
            
            return card;
        }

        static AdaptiveCard GetTimeAdaptiveCard(DateTime dateTime)
        {
            string json = System.IO.File.ReadAllText(@"Resources\\carBookingTime.json");
            var card = AdaptiveCards.AdaptiveCard.FromJson(json).Card;
            AdaptiveContainer adaptiveContainer = (AdaptiveContainer)card.Body[1];
            AdaptiveTimeInput adaptiveTimeInput = (AdaptiveTimeInput)adaptiveContainer.Items[0];
            adaptiveTimeInput.Value = dateTime.ToString("hh:mm");

            return card;
        }

        public static IList<Attachment> GetPackagesCard()
        {
            IList<Attachment> attachments = new List<Attachment>()
            {
                new Attachment()
                {
                    Content = Common.Helper.GetPackageAdaptiveCard("Express Wash", "$25.00", "Our favorite one"),
                    ContentType = AdaptiveCard.ContentType,
                    Name = "Express Wash"
                },
                new Attachment()
                {
                    Content = Common.Helper.GetPackageAdaptiveCard("Signature Wash", "$35.00","Our speciality, your comfort"),
                    ContentType = AdaptiveCard.ContentType,
                    Name = "Signature Wash"
                },
                new Attachment()
                {
                    Content = Common.Helper.GetPackageAdaptiveCard("Premium Wash", "$45.00","All the goodness included"),
                    ContentType = AdaptiveCard.ContentType,
                    Name = "Premium Wash"
                }
            };

            return attachments;
        }

        public static IList<Attachment> GetDateTimeCard(bool isDate)
        {
            IList<Attachment> attachments = new List<Attachment>()
            {
                new Attachment()
                {
                    Content = isDate ? Common.Helper.GetDateAdaptiveCard(DateTime.Now) : Common.Helper.GetTimeAdaptiveCard(DateTime.Now),
                    ContentType = AdaptiveCard.ContentType,
                    Name = "DateTime"
                },
            };

            return attachments;
        }

    }
}

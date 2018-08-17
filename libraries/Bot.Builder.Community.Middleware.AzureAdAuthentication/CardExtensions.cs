using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace Bot.Builder.Community.Middleware.AzureAdAuthentication
{
    public static class CardExtensions
    {

        public static void AddSignInCard(this Activity activity, string url)
        {
            activity.Attachments = new List<Attachment>() { CreateSignInCard(url) };
        }

        private static Attachment CreateSignInCard(string url)
        {
                return new SigninCard()
                {
                    Text = "Please sign in with your Office 365 account to continue",
                    Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Type = ActionTypes.Signin,
                        Title = "Sign in",
                        Value = url
                    }
                }
                }.ToAttachment();
        }
    }
}

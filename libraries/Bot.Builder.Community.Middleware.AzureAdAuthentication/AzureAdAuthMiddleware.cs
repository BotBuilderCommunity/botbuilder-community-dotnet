using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Middleware.AzureAdAuthentication
{
    public class AzureAdAuthMiddleware : IMiddleware
    {
        public AzureAdAuthMiddleware(IAuthTokenStorage tokenStorage, IConfiguration configuration)
        {
            TokenStorage = tokenStorage;
            AzureAdTenant = configuration.GetValue<string>("AzureAdTenant");
            AppClientId = configuration.GetValue<string>("AppClientId");
            AppRedirectUri = configuration.GetValue<string>("AppRedirectUri");
            AppClientSecret = configuration.GetValue<string>("AppClientSecret");
            PermissionsRequested = configuration.GetValue<string>("PermissionsRequested");
        }

        public IAuthTokenStorage TokenStorage { get; }
        public string AzureAdTenant { get; }
        public string AppClientId { get; }
        public string AppRedirectUri { get; }
        public string AppClientSecret { get; }
        public string PermissionsRequested { get; }

        public const string AUTH_TOKEN_KEY = "authToken";

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            var authToken = TokenStorage.LoadConfiguration(context.Activity.Conversation.Id);

            if (authToken == null)
            {
                if (context.Activity.UserHasJustSentMessage() || context.Activity.UserHasJustJoinedConversation())
                {
                    var conversationReference = TurnContext.GetConversationReference(context.Activity);

                    var serializedCookie = WebUtility.UrlEncode(JsonConvert.SerializeObject(conversationReference));

                    var signInUrl = AzureAdExtensions.GetUserConsentLoginUrl(AzureAdTenant, AppClientId, AppRedirectUri, PermissionsRequested, serializedCookie);

                    var activity = context.Activity.CreateReply();
                    activity.AddSignInCard(signInUrl);

                    await context.SendActivity(activity);
                }
            }
            else if (authToken.ExpiresIn < DateTime.Now.AddMinutes(-10))
            {
                if (context.Activity.UserHasJustSentMessage() || context.Activity.UserHasJustJoinedConversation())
                {
                    var client = new HttpClient();
                    var accessToken = await AzureAdExtensions.GetAccessTokenUsingRefreshToken(client, AzureAdTenant, authToken.RefreshToken, AppClientId, AppRedirectUri, AppClientSecret, PermissionsRequested);

                    // have to save it
                    authToken = new ConversationAuthToken(context.Activity.Conversation.Id)
                    {
                        AccessToken = accessToken.accessToken,
                        RefreshToken = accessToken.refreshToken,
                        ExpiresIn = accessToken.refreshTokenExpiresIn
                    };
                    TokenStorage.SaveConfiguration(authToken);

                    // make the authtoken available to downstream pipeline components
                    context.Services.Add(AUTH_TOKEN_KEY, authToken);
                    await next();
                }
            }
            else
            {
                // make the authtoken available to downstream pipeline components
                context.Services.Add(AUTH_TOKEN_KEY, authToken);
                await next();
            }
        }
    }
}

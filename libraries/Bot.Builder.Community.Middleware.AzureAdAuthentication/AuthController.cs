using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
namespace Bot.Builder.Community.Middleware.AzureAdAuthentication
{
    [Produces("application/json")]
    [Route("redirect")]
    public class AuthController : Controller
    {
        public AuthController(IAuthTokenStorage stateManager, IConfiguration configuration)
        {
            StateManager = stateManager;
            AzureAdTenant = configuration.GetValue<string>("AzureAdTenant");
            AppClientId = configuration.GetValue<string>("AppClientId");
            AppRedirectUri = configuration.GetValue<string>("AppRedirectUri");
            AppClientSecret = configuration.GetValue<string>("AppClientSecret");
            PermissionsRequested = configuration.GetValue<string>("PermissionsRequested");
        }

        public IAuthTokenStorage StateManager { get; }
        public string AzureAdTenant { get; }
        public string AppClientId { get; }
        public string AppRedirectUri { get; }
        public string AppClientSecret { get; }
        public string PermissionsRequested { get; }

        public async Task<ContentResult> Get(string code, string state)
        {
            // get the conversation reference
            var botFrameworkConversationReference = JsonConvert.DeserializeObject<ConversationReference>(state);

            // get the access token and store against the conversation id
            var authToken = await new HttpClient().GetAccessTokenUsingAuthorizationCode(AzureAdTenant, code, AppClientId, AppRedirectUri, AppClientSecret, PermissionsRequested);
            StateManager.SaveConfiguration(new ConversationAuthToken(botFrameworkConversationReference.Conversation.Id)
            {
                AccessToken = authToken.accessToken,
                ExpiresIn = authToken.refreshTokenExpiresIn,
                RefreshToken = authToken.refreshToken                
            });

            // send a proactive message back to user
            var connectorClient = new ConnectorClient(new Uri(botFrameworkConversationReference.ServiceUrl));
            var proactiveMessage = botFrameworkConversationReference.GetContinuationActivity();
            proactiveMessage.Text = "How can i help?";
            //proactiveMessage.AddSuggestedActions();
            connectorClient.Conversations.SendToConversation(proactiveMessage);
            return Content("Thank you! you have been logged in and may now close this window!");
        }
    }
}

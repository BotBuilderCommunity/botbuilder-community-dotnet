using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.Announcer
{
    public interface IServiceAgentAnnouncer : IObservable<AgentJob>
    {
        void SendAnnouncement();
        void UserLastAccessTime(string userId);

        void RegisterUser(ITurnContext turnContext);

        void SetController(IBot bot, IBotFrameworkHttpAdapter botHttpAdapter);
    }
}
using System;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Announcer;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.Receiver
{
    public interface IServiceAgentReceiver : IObserver<AgentJob>
    {
        IDisposable Disposable { get; set; }
    }
}
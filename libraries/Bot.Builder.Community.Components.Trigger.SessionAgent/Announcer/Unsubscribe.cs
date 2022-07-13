using System;
using System.Collections.Concurrent;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.Announcer
{
    public partial class ServiceAgentAnnouncer
    {
        private class Unsubscribe : IDisposable
        {
            private readonly ConcurrentDictionary<string, IObserver<AgentJob>> _observers;
            private readonly string _userId;

            public Unsubscribe()
            {
                _observers = null;
                _userId = string.Empty;
            }

            public Unsubscribe(ConcurrentDictionary<string, IObserver<AgentJob>> observers, string userId)
            {
                _observers = observers;
                _userId = userId;
            }

            public void Dispose()
            {
                _observers?.TryRemove(_userId, out _);
            }
        }
    }
}
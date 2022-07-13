using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Receiver;
using Bot.Builder.Community.Components.Trigger.SessionAgent.UserInformation;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.Announcer
{
    public partial class ServiceAgentAnnouncer : IServiceAgentAnnouncer
    {
        private readonly ConcurrentDictionary<string, IObserver<AgentJob>> _usersDictionary =
            new ConcurrentDictionary<string, IObserver<AgentJob>>();

        private IBot _bot;
        private IBotFrameworkHttpAdapter _botAdapter;

        private IDisposable Subscribe(string userId, IObserver<AgentJob> observer)
        {
            if (!_usersDictionary.ContainsKey(userId))
            {
                _usersDictionary.TryAdd(userId, observer);
            }

            return new Unsubscribe(_usersDictionary, userId);
        }

        public IDisposable Subscribe(IObserver<AgentJob> observer)
        {
            throw new NotImplementedException("Subscribe function handle by internally");
        }

        public void SendAnnouncement()
        {
            if (!_usersDictionary.IsEmpty)
            {
                Parallel.ForEach(_usersDictionary, number =>
                {
                    number.Value.OnNext(AgentJob.Track);
                });
            }
        }

        public void UserLastAccessTime(string userId)
        {
            if (_usersDictionary.TryGetValue(userId, out var subscribeTime))
            {
                subscribeTime.OnNext(AgentJob.Update);
            }
        }

        public void RegisterUser(ITurnContext turnContext)
        {
            if (_bot != null && _botAdapter != null)
            {
                IUser user = new User(_bot, _botAdapter, turnContext);

                var watcher = new ServiceAgentReceiver(user);

                watcher.Disposable = Subscribe(user.UserId, watcher);
            }
        }

        public void SetController(IBot bot, IBotFrameworkHttpAdapter botHttpAdapter)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            _botAdapter = botHttpAdapter ?? throw new ArgumentNullException(nameof(botHttpAdapter));
        }

    }
}
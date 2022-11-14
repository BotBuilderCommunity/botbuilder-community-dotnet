using System;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Announcer;
using Bot.Builder.Community.Components.Trigger.SessionAgent.Helper;
using Bot.Builder.Community.Components.Trigger.SessionAgent.UserInformation;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.Receiver
{
    public sealed class ServiceAgentReceiver : IServiceAgentReceiver
    {
        private readonly IUser _user;
        public IDisposable Disposable { get; set; }

        public ServiceAgentReceiver(IUser user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(AgentJob value)
        {
            switch (value)
            {
                case AgentJob.Track:
                {
                    var timeInterval = DateTime.UtcNow - _user.LastAccessTime;

                    if (ActivityHelper.ReminderSeconds > 0 && timeInterval >=
                                                            TimeSpan.FromSeconds(ActivityHelper.ReminderSeconds)
                                                            && _user.IsReminder)
                    {
                        if (ActivityHelper.ExpireAfterSeconds <= 0)
                        {
                            _user.SendTrigger(ActivityHelper.ReminderTrigger);
                            _user.IsReminder = false;
                        }
                        else if (ActivityHelper.ReminderSeconds <= ActivityHelper.ExpireAfterSeconds)
                        {
                            _user.SendTrigger(ActivityHelper.ReminderTrigger);
                            _user.IsReminder = false;
                        }
                    }
                    else if (ActivityHelper.ExpireAfterSeconds > 0 && 
                            timeInterval >= TimeSpan.FromSeconds(ActivityHelper.ExpireAfterSeconds))
                    {
                        _user.SendTrigger(ActivityHelper.SessionExpireTrigger);
                        Disposable.Dispose();
                    }

                    break;
                }
                case AgentJob.Update:
                    _user.LastAccessTime = DateTime.UtcNow;
                    _user.IsReminder = true;
                    break;
            }
        }
    }
}
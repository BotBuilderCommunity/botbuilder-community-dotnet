using System;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Components.Trigger.SessionAgent.UserInformation
{
    public interface IUser
    {
        Task SendTrigger(string triggerName);

        DateTime LastAccessTime { get; set; }

        string UserId { get; }

        bool IsReminder { get; set; }

    }
}
using Microsoft.AspNetCore.Mvc;

namespace Bot.Builder.Community.Components.Handoff.Shared
{
    [ApiController]
    public class HandoffController : ControllerBase
    {
        protected ConversationHandoffRecordMap ConversationHandoffRecordMap;

        public HandoffController(ConversationHandoffRecordMap conversationHandoffRecordMap)
        {
            ConversationHandoffRecordMap = conversationHandoffRecordMap;
        }
    }
}

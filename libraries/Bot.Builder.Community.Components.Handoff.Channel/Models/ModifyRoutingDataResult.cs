using Newtonsoft.Json;

namespace Bot.Builder.Community.Components.Handoff.Channel.Models
{
    public enum ModifyRoutingDataResultType
    {
        Added,
        AlreadyExists,
        Removed,
        Error
    }

    public class ModifyRoutingDataResult : MessageRouterResult
    {
        public ModifyRoutingDataResultType Type
        {
            get;
            set;
        }

        public override string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
using Bot.Builder.Community.Adapters.Infobip.Core;

namespace Bot.Builder.Community.Adapters.Infobip.Viber
{
    public class InfobipViberClient: InfobipClientBase, IInfobipViberClient
    {
        public InfobipViberClient(InfobipViberAdapterOptions infobipViberAdapterOptions) : base(infobipViberAdapterOptions)
        {
        }
    }
}

using Bot.Builder.Community.Adapters.Infobip.Core;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Infobip.Sms
{
    public sealed class InfobipSmsClient : InfobipClientBase, IInfobipSmsClient
    {
        public InfobipSmsClient(InfobipSmsAdapterOptions infobipSmsAdapterOptions): base(infobipSmsAdapterOptions)
        {
        }
    }
}
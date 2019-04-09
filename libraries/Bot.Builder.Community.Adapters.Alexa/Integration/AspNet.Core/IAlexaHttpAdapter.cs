using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;

namespace Bot.Builder.Community.Adapters.Alexa.Integration.AspNet.Core
{
    public interface IAlexaHttpAdapter
    {
        Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default(CancellationToken));
    }
}
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Zoom.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Zoom
{
    internal class ValidationHelper
    {
        public static async Task<bool> ValidateRequest(HttpRequest request, ZoomRequest zoomRequest, string body, ILogger logger)
        {
            return true;
        }
    }
}
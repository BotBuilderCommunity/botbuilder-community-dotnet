using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness
{
    internal class ValidationHelper
    {
        public static async Task<bool> ValidateRequest(HttpRequest request, string mspId, string tokenSecret, ILogger logger)
        {
            return true;
        }
    }
}
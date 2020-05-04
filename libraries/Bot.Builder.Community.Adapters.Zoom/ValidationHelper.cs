using System;
using Bot.Builder.Community.Adapters.Zoom.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Zoom
{
    internal class ValidationHelper
    {
        public static bool ValidateRequest(HttpRequest request, ZoomRequest zoomRequest, string body, ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}
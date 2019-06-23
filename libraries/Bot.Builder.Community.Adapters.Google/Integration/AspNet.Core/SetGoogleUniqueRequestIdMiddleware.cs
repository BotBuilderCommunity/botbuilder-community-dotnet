using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bot.Builder.Community.Adapters.Google.Integration.AspNet.Core
{
    public class SetGoogleUniqueRequestIdMiddleware
    {
        private readonly RequestDelegate _next;

        public SetGoogleUniqueRequestIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // IMyScopedService is injected into Invoke
        public async Task Invoke(HttpContext httpContext, IHttpContextAccessor httpContextAccessor)
        {
            httpContextAccessor.HttpContext?.Items?.Add("GoogleUniqueRequestId", $"google-{Guid.NewGuid().ToString()}");

            await _next(httpContext);
        }
    }
}

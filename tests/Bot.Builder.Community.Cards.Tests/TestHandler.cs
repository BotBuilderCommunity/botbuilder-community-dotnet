using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Cards.Tests
{
    public class TestHandler : HttpMessageHandler
    {
        public TestHandler(HttpMessageDelegate func)
        {
            Func = func;
        }

        public HttpMessageDelegate Func { get; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Func(request));
        }
    }
}

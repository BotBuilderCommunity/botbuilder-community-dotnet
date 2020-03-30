using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Moq;

namespace Bot.Builder.Community.Adapters.RingCentral.Tests.Helper
{
    public static class OptionsHelper
    {
        public static IOptionsMonitor<T> GetOptionsMonitor<T>(T options) 
            where T : class
        {
            var optionsMonitorMock = new Mock<IOptionsMonitor<T>>();
            optionsMonitorMock.Setup(o => o.CurrentValue).Returns(options);
            return optionsMonitorMock.Object;
        }
    }
}

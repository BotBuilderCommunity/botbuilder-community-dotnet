// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Bot.Builder.Community.Adapters.Alexa.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bot.Builder.Community.Adapters.Alexa.Tests.Integration.AspNet.Core
{
    public class AlexaHttpAdapterTests
    {
        private static readonly Mock<ILogger> TestLogger = new Mock<ILogger>();

        [Fact]
        public void ConstructorWithNoArgumentsShouldSucceed()
        {
            Assert.NotNull(new AlexaHttpAdapter());
        }

        [Fact]
        public void ConstructorWithOptionsOnlyShouldSucceed()
        {
            var options = new AlexaAdapterOptions();
            Assert.NotNull(new AlexaHttpAdapter(options));
        }

        [Fact]
        public void ConstructorWithLoggerOnlyShouldSucceed()
        {
            Assert.NotNull(new AlexaHttpAdapter(null, TestLogger.Object));
        }

        [Fact]
        public void ConstructorWithValidateRequestsOnlyShouldSucceed()
        {
            Assert.NotNull(new AlexaHttpAdapter(true));
        }

        [Fact]
        public void ConstructorWithValidateRequestsAndLoggerShouldSucceed()
        {
            Assert.NotNull(new AlexaHttpAdapter(false, TestLogger.Object));
        }
    }
}

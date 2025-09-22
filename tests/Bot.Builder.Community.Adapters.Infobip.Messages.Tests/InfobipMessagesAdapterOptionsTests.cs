using System;
using Microsoft.Extensions.Configuration;
using Xunit;
using Bot.Builder.Community.Adapters.Infobip.Messages;
using Bot.Builder.Community.Adapters.Infobip.Messages.Models;
using System.Collections.Generic;

namespace Bot.Builder.Community.Adapters.Infobip.Messages.Tests
{
    public class InfobipMessagesAdapterOptionsTests
    {
        [Fact]
        public void Constructor_WithValidParameters_SetsProperties()
        {
            // Arrange
            var apiKey = "test-api-key";
            var baseUrl = "https://api.infobip.com";
            var appSecret = "test-secret";

            // Act
            var options = new InfobipMessagesAdapterOptions(apiKey, baseUrl, appSecret);

            // Assert
            Assert.Equal(apiKey, options.InfobipApiKey);
            Assert.Equal(baseUrl, options.InfobipMessagesApiBaseUrl);
            Assert.Equal(appSecret, options.InfobipAppSecret);
            Assert.Equal(InfobipChannels.WhatsApp, options.DefaultChannel);
            Assert.Equal(InfobipAdaptationMode.Flexible, options.AdaptationMode);
        }

        [Fact]
        public void Constructor_WithNullApiKey_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new InfobipMessagesAdapterOptions(null, "https://api.infobip.com"));
        }

        [Fact]
        public void Constructor_WithNullBaseUrl_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new InfobipMessagesAdapterOptions("test-key", null));
        }

        [Fact]
        public void Constructor_WithConfiguration_ParsesAllSettings()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["InfobipApiKey"] = "config-api-key",
                ["InfobipMessagesApiBaseUrl"] = "https://config.infobip.com",
                ["InfobipAppSecret"] = "config-secret",
                ["InfobipMessagesApiKey"] = "messages-api-key",
                ["DefaultSender"] = "test-sender",
                ["DefaultChannel"] = "SMS",
                ["AdaptationMode"] = "STRICT",
                ["DefaultValidityPeriod"] = "24",
                ["DefaultValidityPeriodTimeUnit"] = "HOURS",
                ["EnableUrlShortening"] = "true",
                ["EnableUrlTracking"] = "true",
                ["EnableInteractiveMessaging"] = "false",
                ["EnableWhatsAppTemplates"] = "false",
                ["MaxCarouselCards"] = "5",
                ["MaxInteractiveButtons"] = "2"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var options = new InfobipMessagesAdapterOptions(configuration);

            // Assert
            Assert.Equal("messages-api-key", options.InfobipApiKey); // Should prefer Messages API key
            Assert.Equal("https://config.infobip.com", options.InfobipMessagesApiBaseUrl);
            Assert.Equal("config-secret", options.InfobipAppSecret);
            Assert.Equal("test-sender", options.DefaultSender);
            Assert.Equal("SMS", options.DefaultChannel);
            Assert.Equal("STRICT", options.AdaptationMode);
            Assert.Equal(24, options.DefaultValidityPeriod);
            Assert.Equal("HOURS", options.DefaultValidityPeriodTimeUnit);
            Assert.True(options.EnableUrlShortening);
            Assert.True(options.EnableUrlTracking);
            Assert.False(options.EnableInteractiveMessaging);
            Assert.False(options.EnableWhatsAppTemplates);
            Assert.Equal(5, options.MaxCarouselCards);
            Assert.Equal(2, options.MaxInteractiveButtons);
        }

        [Fact]
        public void Constructor_WithEmptyConfiguration_UsesDefaults()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["InfobipApiKey"] = "test-key",
                ["InfobipMessagesApiBaseUrl"] = "https://api.infobip.com"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var options = new InfobipMessagesAdapterOptions(configuration);

            // Assert - Should use default values
            Assert.Equal(InfobipChannels.WhatsApp, options.DefaultChannel);
            Assert.Equal(InfobipAdaptationMode.Flexible, options.AdaptationMode);
            Assert.True(options.EnableInteractiveMessaging);
            Assert.True(options.EnableWhatsAppTemplates);
            Assert.Equal(10, options.MaxCarouselCards);
            Assert.Equal(3, options.MaxInteractiveButtons);
            Assert.Equal(30, options.ApiTimeoutSeconds);
            Assert.Equal(3, options.RetryAttempts);
        }

        [Fact]
        public void DefaultProperties_HaveExpectedValues()
        {
            // Arrange & Act
            var options = new InfobipMessagesAdapterOptions("test", "https://api.infobip.com");

            // Assert - Check all default values
            Assert.Equal(InfobipAdaptationMode.Flexible, options.AdaptationMode);
            Assert.False(options.EnableUrlShortening);
            Assert.False(options.EnableUrlTracking);
            Assert.True(options.EnableInteractiveMessaging);
            Assert.True(options.EnableWhatsAppTemplates);
            Assert.True(options.EnableCarouselSupport);
            Assert.True(options.EnableLocationSharing);
            Assert.True(options.EnableWhatsAppFlows);
            Assert.True(options.EnableContactSharing);
            Assert.True(options.EnableStickers);
            Assert.True(options.EnableCalendarEvents);
            Assert.True(options.EnableReplyContext);
            Assert.True(options.AutoConvertToInteractiveList);
            Assert.Equal(3, options.InteractiveListThreshold);
            Assert.True(options.AutoConvertToCarousel);
            Assert.Equal(10, options.MaxCarouselCards);
            Assert.Equal(3, options.MaxInteractiveButtons);
            Assert.Equal(10, options.MaxListSections);
            Assert.Equal(10, options.MaxListRows);
            Assert.Equal(InfobipFallbackBehavior.ConvertToText, options.FallbackBehavior);
            Assert.Equal("en", options.DefaultLanguageCode);
            Assert.True(options.EnableMediaOptimization);
            Assert.Equal(16 * 1024 * 1024, options.MaxMediaSize);
            Assert.True(options.EnableDeliveryReports);
            Assert.True(options.EnableSeenReports);
            Assert.Equal(30, options.ApiTimeoutSeconds);
            Assert.Equal(3, options.RetryAttempts);
            Assert.Equal(1000, options.RetryDelayMilliseconds);
            Assert.True(options.EnableAutomaticChannelDetection);
            Assert.Equal(InfobipChannels.WhatsApp, options.DefaultChannel);
            Assert.True(options.EnableAdaptationMode);
        }
    }
}
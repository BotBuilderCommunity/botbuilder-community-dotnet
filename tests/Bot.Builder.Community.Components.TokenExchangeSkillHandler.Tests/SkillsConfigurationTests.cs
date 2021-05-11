using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bot.Builder.Community.Components.TokenExchangeSkillHandler.Tests
{
    [TestClass]
    public class SkillsConfigurationTests
    {
        [TestMethod]
        public void MissingHostThrows()
        {
            var config = GetConfig("configSettings_missingHost.json");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () =>
                {
                    var skillsConfig = new SkillsConfiguration(config);
                }, "Should have thrown: Missing 'skillHostEndpoint' in appsettings.");

            Assert.AreEqual("Missing 'skillHostEndpoint' in appsettings.", ex.Message);
        }

        [TestMethod]
        public void MissingSkillAppIdThrows()
        {
            var config = GetConfig("configSettings_missingSkillAppId.json");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () =>
                {
                    var skillsConfig = new SkillsConfiguration(config);
                }, "Should have thrown: Skill in appsettings is missing 'msAppId'.");

            Assert.AreEqual("Skill in appsettings is missing 'msAppId'.", ex.Message);
        }

        [TestMethod]
        public void MissingSkillsThrows()
        {
            var config = GetConfig("configSettings_missingSkills.json");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () =>
                {
                    var skillsConfig = new SkillsConfiguration(config);
                }, "Should have thrown: Missing 'skill' section in appsettings.");

            Assert.AreEqual("Missing 'skill' section in appsettings.", ex.Message);
        }

        [TestMethod]
        public void MissingSkillEndpointUrlThrows()
        {
            var config = GetConfig("configSettings_missingSkillEndpointUrl.json");

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () =>
                {
                    var skillsConfig = new SkillsConfiguration(config);
                }, "Should have thrown: Skill in appsettings is missing 'endpointUrl'.");

            Assert.AreEqual("Skill in appsettings is missing 'endpointUrl'.", ex.Message);
        }

        private IConfigurationRoot GetConfig(string configName)
        {
            return new ConfigurationBuilder()
                .AddJsonFile($".\\configs\\{configName}")
                .Build();

        }
    }
}

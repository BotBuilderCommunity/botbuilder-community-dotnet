using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Components.TokenExchangeSkillHandler
{
    internal class SkillsConfiguration
    {
        public SkillsConfiguration(IConfiguration configuration)
        {
            var section = configuration?.GetSection("skill");
            var skills = section?.GetChildren();
            if (section == null || skills == null || skills.Count() < 1)
            {
                throw new InvalidOperationException("Missing 'skill' section in appsettings.");
            }

            foreach (var child in skills)
            {
                var props = child.GetChildren().ToDictionary(x => x.Key, x => x.Value);

                Skills.Add(child.Key, new BotFrameworkSkill()
                {
                    Id = child.Key,
                    AppId = props["msAppId"],
                    SkillEndpoint = new Uri(props["endpointUrl"]),
                });
            }

            var hostEndpoint = configuration?.GetValue<string>("skillHostEndpoint");
            if (string.IsNullOrEmpty(hostEndpoint))
            {
                throw new InvalidOperationException("Missing 'skillHostEndpoint' in appsettings.");
            }

            SkillHostEndpoint = new Uri(hostEndpoint);
        }

        public Uri SkillHostEndpoint { get; }

        public Dictionary<string, BotFrameworkSkill> Skills { get; } = new Dictionary<string, BotFrameworkSkill>();
    }
}

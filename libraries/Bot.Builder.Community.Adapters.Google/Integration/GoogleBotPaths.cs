// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Bot.Builder.Community.Adapters.Google.Integration
{
    public class GoogleBotPaths
    {
        public GoogleBotPaths()
        {
            this.BasePath = "/api";
            this.SkillRequestsPath = "actionrequests";
        }

        public string BasePath { get; set; }
        public string SkillRequestsPath { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;

namespace Bot.Builder.Community.Dialogs.Adaptive.Rest.Tests
{
    public class TestUtils
    {
        public static IConfiguration DefaultConfiguration { get; set; } = new ConfigurationBuilder().AddInMemoryCollection()
            .Build();

        public static string RootFolder { get; set; } = GetProjectPath();

        public static IEnumerable<object[]> GetTestScripts(string relativeFolder)
        {
            string testFolder = Path.GetFullPath(Path.Combine(RootFolder, PathUtils.NormalizePath(relativeFolder)));
            return Directory.EnumerateFiles(testFolder, "*.test.dialog", SearchOption.AllDirectories).Select(s => new object[] { Path.GetFileName(s) }).ToArray();
        }

        public static async Task RunTestScript(ResourceExplorer resourceExplorer, string resourceId = null, [CallerMemberName] string testName = null)
        {
            var script = resourceExplorer.LoadType<TestScript>(resourceId ?? $"{testName}.test.dialog");
            script.Description = script.Description ?? resourceId;

            await script.ExecuteAsync(testName: testName, resourceExplorer: resourceExplorer).ConfigureAwait(false);
        }

        public static string GetProjectPath()
        {
            var parent = Environment.CurrentDirectory;
            while (!string.IsNullOrEmpty(parent))
            {
                if (Directory.EnumerateFiles(parent, "*proj").Any())
                {
                    break;
                }
                else
                {
                    parent = Path.GetDirectoryName(parent);
                }
            }

            return parent;
        }
    }
}

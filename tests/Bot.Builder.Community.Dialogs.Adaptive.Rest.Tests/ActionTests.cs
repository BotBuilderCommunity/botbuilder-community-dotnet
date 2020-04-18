// <copyright file="ActionTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Bot.Builder.Community.Dialogs.Adaptive.Rest.Tests
{
    using System.IO;
    using System.Threading.Tasks;
    using Bot.Builder.Community.Dialogs.Adaptive.Rest.Tests.Fakes;
    using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ActionTests
    {
        public static ResourceExplorer ResourceExplorer { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "Tests", nameof(ActionTests)), monitorChanges: false)
                .RegisterType<FakeEchoRestAction>(FakeEchoRestAction.DeclarativeType);
        }

        [TestMethod]
        public async Task Action_EchoApi()
        {
           await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}

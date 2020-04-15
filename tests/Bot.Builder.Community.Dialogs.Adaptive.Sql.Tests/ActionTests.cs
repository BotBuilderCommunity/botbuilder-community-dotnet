using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Dialogs.Adaptive.Sql.Actions;
using Dapper;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.TestActions;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bot.Builder.Community.Dialogs.Adaptive.Sql.Tests
{
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
                .RegisterType<TestLogAction>(TestLogAction.DeclarativeType);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            DbProviderFactories.RegisterFactory("SQLite", SQLiteFactory.Instance);

            string sql = @"
                DROP TABLE IF EXISTS ActionTable;
                CREATE TABLE ActionTable (
                    id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                    TextValue TEXT NOT NULL,
                    ExpressionValue TEXT NOT NULL
                );
            ";

            var connection = DbProviderFactories.GetFactory("SQLite").CreateConnection();
            connection.ConnectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";
            connection.Execute(sql);
        }

        [TestMethod]
        public async Task Action_DeleteRow()
        {
            //ensure lib is loaded
            var t = new InsertRow();

            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Action_GetRow()
        {
            //ensure lib is loaded
            var t = new InsertRow();

            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Action_GetRows()
        {
            //ensure lib is loaded
            var t = new InsertRow();

            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Action_InsertRow()
        {
            //ensure lib is loaded
            var t = new InsertRow();

            await TestUtils.RunTestScript(ResourceExplorer);
        }

        [TestMethod]
        public async Task Action_UpdateRow()
        {
            //ensure lib is loaded
            var t = new InsertRow();

            await TestUtils.RunTestScript(ResourceExplorer);
        }
    }
}

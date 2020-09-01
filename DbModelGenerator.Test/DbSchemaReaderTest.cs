using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DeepEqual.Syntax;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using NUnit.Framework;

namespace DbModelGenerator.Test
{
    [TestFixture]
    public class DbSchemaReaderTest
    {
        private static IBuildEngine GetTask()
        {
            var task = new Mock<IBuildEngine>();
            task.Setup(engine =>
                    engine.LogMessageEvent(It.IsAny<BuildMessageEventArgs>()))
                .Callback((BuildMessageEventArgs e) => { Console.WriteLine("MESSAGE {0}", e.Message); });

            task.Setup(engine =>
                    engine.LogCustomEvent(It.IsAny<CustomBuildEventArgs>()))
                .Callback((CustomBuildEventArgs e) => { Console.WriteLine("CUSTOM {0}", e.Message); });
            task.Setup(engine =>
                    engine.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
                .Callback((BuildErrorEventArgs e) => { Console.WriteLine("ERROR {0}", e.Message); });
            return task.Object;
        }

        [Test]
        public void ShouldGenerateModelFromScripts()
        {
            var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
            var scriptsPath = Path.Combine(testProjectDirectory, "Scripts");

            var dbSchemaReader = new DbSchemaReader();

            var actual = dbSchemaReader.Read(scriptsPath, new TaskLoggingHelper(GetTask(), "build"));

            var brandTable = new Table("brand", ImmutableList.Create(
                new Column("id", "int", false, true, true),
                new Column("name", "string", false, false, false),
                new Column("logo", "string", true, false, false),
                new Column("archived", "bool", true, false, false),
                new Column("color", "string", false, false, false),
                new Column("external_id", "string", true, false, false)
            ));
            actual.Tables.ShouldDeepEqual(ImmutableList.Create(brandTable));
        }

        [Test]
        public void ShouldGenerateModelFromScripts2()
        {
            var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
            var scriptsPath = Path.Combine(testProjectDirectory, "Scripts2");

            var dbSchemaReader = new DbSchemaReader();

            var actual = dbSchemaReader.Read(scriptsPath, new TaskLoggingHelper(GetTask(), "build"));

            var brandTable = new Table("brand", ImmutableList.Create(
                new Column("id", "int", false, true, true),
                new Column("name", "string", false, false, false),
                new Column("logo", "string", true, false, false),
                new Column("archived", "bool", true, false, false),
                new Column("color", "string", false, false, false),
                new Column("external_id", "string", true, false, false)
            ));
            var bbbTable = new Table("bbbb", ImmutableList.Create(
                new Column("id", "int", false, true, true),
                new Column("name", "string", false, false, false),
                new Column("logo", "string", true, false, false)
            ));
            actual.Tables.ShouldDeepEqual(ImmutableList.Create(bbbTable, brandTable));
        }


        [Test]
        public void ShouldGenerateModelFromScripts3()
        {
            var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
            var scriptsPath = Path.Combine(testProjectDirectory, "Scripts3");

            var dbSchemaReader = new DbSchemaReader();

            var actual = dbSchemaReader.Read(scriptsPath, new TaskLoggingHelper(GetTask(), "build"));

            var bbbTable = new Table("bbbb", ImmutableList.Create(
                new Column("id", "int", false, true, true),
                new Column("name", "string", false, false, false),
                new Column("logo", "string", true, false, false)
            ));
            var brandiTable = new Table("brandi", ImmutableList.Create(
                new Column("id", "int", false, false, true),
                new Column("pipas", "bool", true, false, false),
                new Column("nom", "string", false, false, false),
                new Column("logo", "string", true, false, false)
            ));
            actual.Tables.ShouldDeepEqual(ImmutableList.Create(bbbTable, brandiTable));
        }

        [Test]
        public void ShouldGenerateModelFromScripts4()
        {
            var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
            var scriptsPath = Path.Combine(testProjectDirectory, "Scripts4");

            var dbSchemaReader = new DbSchemaReader();

            var actual = dbSchemaReader.Read(scriptsPath, new TaskLoggingHelper(GetTask(), "build"));

            var bbbTable = new Table("bbbb", ImmutableList.Create(
                new Column("name", "string", false, false, false),
                new Column("lobo", "string", true, false, false),
                new Column("jiji", "int", false, false, false),
                new Column("tartuffe", "int", true, false, false)
            ));
            actual.Tables.ShouldDeepEqual(ImmutableList.Create(bbbTable));
        }

        [Test]
        public void ShouldGenerateModelFromScripts5()
        {
            var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
            var scriptsPath = Path.Combine(testProjectDirectory, "Scripts5");

            var dbSchemaReader = new DbSchemaReader();

            var actual = dbSchemaReader.Read(scriptsPath, new TaskLoggingHelper(GetTask(), "build"));

            var userProfile = new Table("user_profile", ImmutableList.Create(
                new Column("id", "int", false, true, true),
                new Column("firstName", "string", false, false, false),
                new Column("lastName", "string", false, false, false),
                new Column("password", "string", false, false, false),
                new Column("algorithm", "int", false, false, false),
                new Column("balance", "decimal", false, false, false),
                new Column("salt", "string", false, false, false),
                new Column("disabled", "bool", false, false, false),
                new Column("groupId", "string", false, false, false),
                new Column("latitude", "decimal", true, false, false)
            ));
            var userGroup = new Table("user_group", ImmutableList.Create(
                new Column("id", "string", false, true, false)
            ));
            var userRole = new Table("role", ImmutableList.Create(
                new Column("id", "string", false, true, false)
            ));
            var userGroupRole = new Table("user_group_role", ImmutableList.Create(
                new Column("groupId", "string", false, true, false),
                new Column("roleId", "int", false, true, false)
            ));
            actual.Tables.ShouldDeepEqual(ImmutableList.Create(userRole, userGroup, userGroupRole, userProfile));
        }

        [Test]
        public void ShouldGenerateModelFromScripts6()
        {
            var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
            var scriptsPath = Path.Combine(testProjectDirectory, "Scripts6");

            var dbSchemaReader = new DbSchemaReader();

            var actual = dbSchemaReader.Read(scriptsPath, new TaskLoggingHelper(GetTask(), "build"));

            var tenant = new Table("tenant", ImmutableList.Create(
                new Column("id", "int", false, true, true),
                new Column("name", "string", true, false, false),
                new Column("groupId", "int", false, false, false)
            ));
            var userGroup = new Table("lamorosso", ImmutableList.Create(
                new Column("id", "int", false, true, true),
                new Column("name", "string", true, false, false)
            ));
            actual.Tables.ShouldDeepEqual(ImmutableList.Create(userGroup, tenant));
        }

        [Test]
        public void ShouldGenerateModelFromScripts7()
        {
            var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
            var scriptsPath = Path.Combine(testProjectDirectory, "Scripts7");

            var dbSchemaReader = new DbSchemaReader();

            var actual = dbSchemaReader.Read(scriptsPath, new TaskLoggingHelper(GetTask(), "build"));

            Assert.AreEqual(8, actual.Tables.Count());
            Assert.AreEqual(2, actual.Tables.First(t => t.Name == "group_type").Columns.Count);
            Assert.AreEqual(8, actual.Tables.First(t => t.Name == "group_type_available_scope").Columns.Count);
            Assert.AreEqual(3, actual.Tables.First(t => t.Name == "role").Columns.Count);
            Assert.AreEqual(5, actual.Tables.First(t => t.Name == "user_group").Columns.Count);
            Assert.AreEqual(30, actual.Tables.First(t => t.Name == "user_profile").Columns.Count);
            Assert.AreEqual(2, actual.Tables.First(t => t.Name == "user_group_role").Columns.Count);
            Assert.AreEqual(6, actual.Tables.First(t => t.Name == "user_scope_access").Columns.Count);
            Assert.AreEqual(10, actual.Tables.First(t => t.Name == "user_grid_state").Columns.Count);
        }

        [Test]
        public void ShouldGenerateModelFromScripts8()
        {
            var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
            var scriptsPath = Path.Combine(testProjectDirectory, "Scripts8");

            var dbSchemaReader = new DbSchemaReader();

            var actual = dbSchemaReader.Read(scriptsPath, new TaskLoggingHelper(GetTask(), "build"));

            var contract = new Table("contract", ImmutableList.Create(
                new Column("id", "int", false, true, true),
                new Column("code", "string", false, false, false),
                new Column("created_by", "string", false, false, false),
                new Column("creation_date", "DateTime", false, false, false),
                new Column("last_modified_by", "string", true, false, false),
                new Column("last_modification_date", "DateTime", true, false, false),
                new Column("country_id", "int", false, false, false)
            ));
            actual.Tables.ShouldDeepEqual(ImmutableList.Create(contract));
        }
    }
}
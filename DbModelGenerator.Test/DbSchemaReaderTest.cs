using System;
using System.Collections.Immutable;
using System.IO;
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
        [Test]
        public void ShouldGenerateModelFromScripts()
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

            var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
            var scriptsPath = Path.Combine(testProjectDirectory, "Scripts");

            using var dbSchemaReader = new DbSchemaReader();

            var actual = dbSchemaReader.Read(scriptsPath, new TaskLoggingHelper(task.Object, "build"));

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
    }
}
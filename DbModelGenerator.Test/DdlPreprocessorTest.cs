using System;
using System.IO;
using System.Reflection;
using DbModelGenerator.Preprocessor;
using NUnit.Framework;

namespace DbModelGenerator.Test
{
    [TestFixture]
    public sealed class DdlPreprocessorTest
    {
        [Test]
        public void ShouldReturnEmptyWhenEmpty()
        {
            var ddlPreprocessor = new DdlPreprocessor();
            var actual = ddlPreprocessor.Process("");

            Assert.AreEqual("", actual);
        }

        [Test]
        public void ShouldReturnCreateTable()
        {
            var ddlPreprocessor = new DdlPreprocessor();
            var script = @"
CREATE TABLE concept
(
    id   SERIAL NOT NULL,
    code VARCHAR(50),
    PRIMARY KEY (id),
    UNIQUE (code)
);

CREATE TABLE states
(
    id         SERIAL  NOT NULL,
    country_id INTEGER NOT NULL REFERENCES country (id),
    name       VARCHAR(250),
    PRIMARY KEY (id),
    UNIQUE (name)
);
";
            var actual = ddlPreprocessor.Process(script);

            Assert.AreEqual(@"CREATE TABLE concept
(
    id   SERIAL NOT NULL,
    code VARCHAR(50),
    PRIMARY KEY (id),
    UNIQUE (code)
);
CREATE TABLE states
(
    id         SERIAL  NOT NULL,
    country_id INTEGER NOT NULL REFERENCES country (id),
    name       VARCHAR(250),
    PRIMARY KEY (id),
    UNIQUE (name)
);
", actual);
        }


        [Test]
        public void ShouldReturnAlterTable()
        {
            var ddlPreprocessor = new DdlPreprocessor();
            var script = @"
ALTER TABLE concept DROP COLUMN id;
";
            var actual = ddlPreprocessor.Process(script);

            Assert.AreEqual("ALTER TABLE concept DROP COLUMN id;\n", actual);
        }

        [Test]
        public void ShouldReturnDropTable()
        {
            var ddlPreprocessor = new DdlPreprocessor();
            var script = @"
DROP TABLE concept;
";
            var actual = ddlPreprocessor.Process(script);

            Assert.AreEqual("DROP TABLE concept;\n", actual);
        }

        [Test]
        public void ShouldNotReturnAlterSequence()
        {
            var ddlPreprocessor = new DdlPreprocessor();
            var script = @"
ALTER SEQUENCE concept_id_seq RESTART WITH 10;
";
            var actual = ddlPreprocessor.Process(script);

            Assert.AreEqual("", actual);
        }

        [Test]
        public void ShouldFilterScript()
        {
            var ddlPreprocessor = new DdlPreprocessor();

            var script = ReadFixture("script.sql");

            var actual = ddlPreprocessor.Process(script);

            var expected = ReadFixture("script_expected.sql");
            Assert.AreEqual(expected, actual);
        }

        private static string ReadFixture(string file)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"DbModelGenerator.Test.Fixtures.{file}");
            if (stream == null)
            {
                throw new ArgumentException($"{file} not found in assembly");
            }

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
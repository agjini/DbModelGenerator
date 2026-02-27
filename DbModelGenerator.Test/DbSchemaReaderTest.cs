using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using DeepEqual.Syntax;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DbModelGenerator.Test;

[TestFixture]
public class DbSchemaReaderTest
{
    private static ImmutableList<InputFile> GetDirectoryContent(string scriptDirectory)
    {
        var scriptNamespace = Path.GetFileName(scriptDirectory);

        if (scriptNamespace == null)
        {
            throw new ArgumentException($"Project script namespace not found for '{scriptDirectory}' !");
        }

        return Directory.GetFiles(scriptDirectory).OrderBy(f => f)
            .Select(file => new InputFile(file, File.ReadAllText(file, Encoding.UTF8))).ToImmutableList();
    }

    [Test]
    public void ShouldGenerateModelFromScripts()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts");
        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var brandTable = new Table("brand", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("name", "string", false, false, false),
            new Column("logo", "string", true, false, false),
            new Column("archived", "bool", true, false, false),
            new Column("color", "string", false, false, false),
            new Column("external_id", "string", true, false, false)
        ), ImmutableSortedSet.Create("id"));
        actual.Tables.ShouldDeepEqual(ImmutableList.Create(brandTable));
    }

    [Test]
    public void ShouldGenerateModelFromScripts2()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts2");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var brandTable = new Table("brand", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("name", "string", false, false, false),
            new Column("logo", "string", true, false, false),
            new Column("archived", "bool", true, false, false),
            new Column("color", "string", false, false, false),
            new Column("external_id", "string", true, false, false)
        ), ImmutableSortedSet.Create("id"));
        var bbbTable = new Table("bbbb", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("name", "string", false, false, false),
            new Column("logo", "string", true, false, false)
        ), ImmutableSortedSet.Create("id"));
        actual.Tables.ShouldDeepEqual(ImmutableList.Create(bbbTable, brandTable));
    }


    [Test]
    public void ShouldGenerateModelFromScripts3()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts3");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var bbbTable = new Table("bbbb", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("name", "string", false, false, false),
            new Column("logo", "string", true, false, false)
        ), ImmutableSortedSet.Create("id"));
        var brandiTable = new Table("brandi", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("pipas", "bool", true, false, false),
            new Column("nom", "string", false, false, false),
            new Column("logo", "string", true, false, false)
        ), ImmutableSortedSet<string>.Empty);
        actual.Tables.ShouldDeepEqual(ImmutableList.Create(bbbTable, brandiTable));
    }

    [Test]
    public void ShouldGenerateModelFromScripts4()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts4");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var bbbTable = new Table("bbbb", ImmutableList.Create(
            new Column("name", "string", false, false, false),
            new Column("lobo", "string", true, false, false),
            new Column("jiji", "int", false, false, false),
            new Column("tartuffe", "int", true, false, false)
        ), ImmutableSortedSet<string>.Empty);
        actual.Tables.ShouldDeepEqual(ImmutableList.Create(bbbTable));
    }

    [Test]
    public void ShouldGenerateModelFromScripts5()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts5");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var userProfile = new Table("user_profile", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("firstName", "string", false, false, false),
            new Column("lastName", "string", false, false, false),
            new Column("password", "string", false, false, false),
            new Column("algorithm", "int", false, false, false),
            new Column("balance", "decimal", false, false, false),
            new Column("salt", "string", false, false, false),
            new Column("disabled", "bool", false, false, false),
            new Column("groupId", "string", false, false, false),
            new Column("latitude", "decimal", true, false, false)
        ), ImmutableSortedSet.Create("id"));
        var userGroup = new Table("user_group", ImmutableList.Create(
            new Column("id", "string", false, false, false)
        ), ImmutableSortedSet.Create("id"));
        var userRole = new Table("role", ImmutableList.Create(
            new Column("id", "string", false, false, false)
        ), ImmutableSortedSet.Create("id"));
        var userGroupRole = new Table("user_group_role", ImmutableList.Create(
            new Column("groupId", "string", false, false, false),
            new Column("roleId", "int", false, false, false)
        ), ImmutableSortedSet.Create("groupId", "roleId"));
        actual.Tables.ShouldDeepEqual(ImmutableList.Create(userRole, userGroup, userGroupRole, userProfile));
    }

    [Test]
    public void ShouldGenerateModelFromScripts6()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts6");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var tenant = new Table("tenant", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("name", "string", true, false, false),
            new Column("groupId", "int", false, false, false)
        ), ImmutableSortedSet.Create("id"));
        var userGroup = new Table("lamorosso", ImmutableList.Create(
            new Column("id", "int", false, true, false),
            new Column("name", "string", true, false, false)
        ), ImmutableSortedSet.Create("id"));
        actual.Tables.ShouldDeepEqual(ImmutableList.Create(userGroup, tenant));
    }

    [Test]
    public void ShouldGenerateModelFromScripts7()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts7");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        ClassicAssert.AreEqual(8, actual.Tables.Count());
        ClassicAssert.AreEqual(2, actual.Tables.First(t => t.Name == "group_type").Columns.Count);
        ClassicAssert.AreEqual(8, actual.Tables.First(t => t.Name == "group_type_available_scope").Columns.Count);
        ClassicAssert.AreEqual(3, actual.Tables.First(t => t.Name == "role").Columns.Count);
        ClassicAssert.AreEqual(5, actual.Tables.First(t => t.Name == "user_group").Columns.Count);
        ClassicAssert.AreEqual(30, actual.Tables.First(t => t.Name == "user_profile").Columns.Count);
        ClassicAssert.AreEqual(2, actual.Tables.First(t => t.Name == "user_group_role").Columns.Count);
        ClassicAssert.AreEqual(6, actual.Tables.First(t => t.Name == "user_scope_access").Columns.Count);
        ClassicAssert.AreEqual(10, actual.Tables.First(t => t.Name == "user_grid_state").Columns.Count);
    }

    [Test]
    public void ShouldGenerateModelFromScripts8()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts8");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var contract = new Table("contract", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("code", "string", false, false, false),
            new Column("created_by", "string", false, false, false),
            new Column("creation_date", "DateTime", false, false, false),
            new Column("last_modified_by", "string", true, false, false),
            new Column("last_modification_date", "DateTime", true, false, false),
            new Column("pyjame", "int", true, false, false)
        ), ImmutableSortedSet.Create("id"));
        actual.Tables.ShouldDeepEqual(ImmutableList.Create(contract));
    }

    [Test]
    public void ShouldGenerateModelFromScripts9()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts9");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var contract = new Table("contract", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("code", "string", false, false, false),
            new Column("title", "int", false, false, false),
            new Column("created_by", "string", true, false, false),
            new Column("creation_date", "DateTime", true, false, false),
            new Column("last_modified_by", "string", true, false, false),
            new Column("last_modification_date", "DateTime", true, false, false),
            new Column("country_id", "int", false, false, false)
        ), ImmutableSortedSet.Create("id"));
        actual.Tables.ShouldDeepEqual(ImmutableList.Create(contract));
    }

    [Test]
    public void ShouldGenerateModelFromScripts10()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts10");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var contract = new Table("contract", ImmutableList.Create(
            new Column("id", "string", false, false, false)
        ), ImmutableSortedSet<string>.Empty);

        var other = new Table("other", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("contract_id", "int", false, false, false)
        ), ImmutableSortedSet.Create("id"));
        actual.Tables.ShouldDeepEqual(ImmutableList.Create(contract, other));
    }

    [Test]
    public void ShouldGenerateModelFromScripts11()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts11");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var tenantSaml = new Table("tenant_saml", ImmutableList.Create(
            new Column("tenant_id", "string", false, false, false),
            new Column("is_enabled", "bool", false, false, false),
            new Column("identity_provider_metadata", "string", false, false, false),
            new Column("default_group_id", "int", true, false, false),
            new Column("default_culture_id", "string", true, false, false)
        ), ImmutableSortedSet.Create("tenant_id"));

        actual.Tables.ShouldDeepEqual(ImmutableList.Create(tenantSaml));
    }

    [Test]
    public void ShouldGenerateModelFromScripts12()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts12");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var tenantSaml = new Table("tenant_saml", ImmutableList.Create(
            new Column("new_tenant_id", "string", false, false, false),
            new Column("is_enabled", "bool", false, false, false),
            new Column("identity_provider_metadata", "string", false, false, false),
            new Column("default_group_id", "int", true, false, false),
            new Column("default_culture_id", "string", true, false, false)
        ), ImmutableSortedSet.Create("new_tenant_id"));

        actual.Tables.ShouldDeepEqual(ImmutableList.Create(tenantSaml));
    }

    [Test]
    public void ShouldGenerateModelFromScripts13()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts13");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var table = new Table("user_grid_state", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("filter_expression", "string", true, false, false),
            new Column("shared_view_id", "int", true, false, false),
            new Column("is_sharded", "bool", false, false, false)
        ), ImmutableSortedSet.Create("id"));

        actual.Tables.ShouldDeepEqual(ImmutableList.Create(table));
    }

    [Test]
    public void ShouldGenerateModelFromScripts14()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts14");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var table = new Table("user_grid_state", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("filter_expression", "string", true, false, false)
        ), ImmutableSortedSet.Create("id"));

        actual.Tables.ShouldDeepEqual(ImmutableList.Create(table));
    }

    [Test]
    public void ShouldGenerateModelFromScripts15()
    {
        var testProjectDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../");
        var scriptsPath = Path.Combine(testProjectDirectory, "Scripts15");

        var (actual, _) = DbSchemaReader.Read(scriptsPath, GetDirectoryContent(scriptsPath));

        var table = new Table("date_time_only", ImmutableList.Create(
            new Column("id", "int", false, false, true),
            new Column("date", "DateOnly", true, false, false),
            new Column("time", "TimeOnly", true, false, false)
        ), ImmutableSortedSet.Create("id"));

        actual.Tables.ShouldDeepEqual(ImmutableList.Create(table));
    }
}
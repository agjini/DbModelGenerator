using System.Collections.Immutable;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace DbModelGenerator.Test
{
    public sealed class TemplateGeneratorTest
    {
	    [Test]
        public void GenerateAClassForOneTable()
        {
            var table = new Table("user_profile",
                new[] { new Column("id", "string", false, false, false) }.ToImmutableList(),
                ImmutableSortedSet.Create("id"));

            var actual = TemplateGenerator.GenerateClass("Project.Generated.Global", table, null, null, null, "db");

            const string expected = @"
namespace Project.Generated.Global
{

	public sealed class UserProfileDb
	{

		public UserProfileDb(string id)
		{
			Id = id;
		}

		public string Id { get; }

	}

}";
            ClassicAssert.AreEqual(expected, actual);
        }

        [Test]
        public void GenerateAClassForOneTableWithUsing()
        {
            var table = new Table("user_profile",
                new[] { new Column("id", "Guid", false, false, false) }.ToImmutableList(),
                ImmutableSortedSet.Create("id"));

            var actual = TemplateGenerator.GenerateClass("Project.Generated.Global", table, null, null, null, "Db");

            const string expected = @"using System;

namespace Project.Generated.Global
{

	public sealed class UserProfileDb
	{

		public UserProfileDb(Guid id)
		{
			Id = id;
		}

		public Guid Id { get; }

	}

}";
            ClassicAssert.AreEqual(expected, actual);
        }


        [Test]
        public void GenerateAClassForOneTableWithUsingAndIdentity()
        {
            var table = new Table("user_profile",
                new[] { new Column("id", "Guid", false, false, false) }.ToImmutableList(),
                ImmutableSortedSet.Create("id"));

            var actual =
                TemplateGenerator.GenerateClass("Project.Generated.Global", table, "Odin.Api.IIdentity", null, null,
                    "Db");

            const string expected = @"using System;
using Odin.Api;

namespace Project.Generated.Global
{

	public sealed class UserProfileDb : IIdentity<Guid>
	{

		public UserProfileDb(Guid id)
		{
			Id = id;
		}

		public Guid Id { get; }

	}

}";
            ClassicAssert.AreEqual(expected, actual);
        }

        [Test]
        public void GenerateAClassForOneTableWithUsingAndIdentityWithoutId()
        {
            var table = new Table("user_profile",
                new[]
                {
                    new Column("role_id", "int", false, true, true),
                    new Column("group_id", "int", false, false, false)
                }.ToImmutableList(),
                ImmutableSortedSet.Create("role_id", "group_id"));

            var actual =
                TemplateGenerator.GenerateClass("Project.Generated.Global", table,
                    "Odin.Api.IIdentity;Odin.Api.IRoleEntity(role_id);Odin.Api.IGroupEntity(role_id,group_id!);Odin.Api.Entity.IDbEntity(model_id,created_by,creation_date,modified_by,modification_date)",
                    "Odin.Api.PrimaryKey",
                    "Odin.Api.Generated",
                    null);

            const string expected = @"using Odin.Api;

namespace Project.Generated.Global
{

	public sealed class UserProfile : IRoleEntity, IGroupEntity<int>
	{

		public UserProfile(int? role_id, int group_id)
		{
			RoleId = role_id;
			GroupId = group_id;
		}

		[PrimaryKey]
		[Generated]
		public int? RoleId { get; }

		[PrimaryKey]
		public int GroupId { get; }

	}

}";
            ClassicAssert.AreEqual(expected, actual);
        }
    }
}
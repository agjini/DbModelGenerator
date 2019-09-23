using System;
using NUnit.Framework;

namespace DbModelGenerator.Test
{
    public sealed class TemplateGeneratorTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GenerateAClassForOneTable()
        {
            var table = new Table("user_profile", new[] {new Column("id", "string", false, true, false)});

            var actual = TemplateGenerator.GenerateClass("Project.Generated.Global", table, null, null, null);

            Console.WriteLine(actual);

            const string expected = @"
namespace Project.Generated.Global
{

	public sealed class UserProfile
	{

		public UserProfile(string id)
		{
			Id = id;
		}

		public string Id { get; }

	}

}";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GenerateAClassForOneTableWithUsing()
        {
            var table = new Table("user_profile", new[] {new Column("id", "Guid", false, true, false)});

            var actual = TemplateGenerator.GenerateClass("Project.Generated.Global", table, null, null, null);

            Console.WriteLine(actual);

            const string expected = @"using System;

namespace Project.Generated.Global
{

	public sealed class UserProfile
	{

		public UserProfile(Guid id)
		{
			Id = id;
		}

		public Guid Id { get; }

	}

}";
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void GenerateAClassForOneTableWithUsingAndIdentity()
        {
            var table = new Table("user_profile", new[] {new Column("id", "Guid", false, true, false)});

            var actual =
                TemplateGenerator.GenerateClass("Project.Generated.Global", table, "Odin.Api.IIdentity", null, null);

            Console.WriteLine(actual);

            const string expected = @"using System;
using Odin.Api;

namespace Project.Generated.Global
{

	public sealed class UserProfile : IIdentity<Guid>
	{

		public UserProfile(Guid id)
		{
			Id = id;
		}

		public Guid Id { get; }

	}

}";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GenerateAClassForOneTableWithUsingAndIdentityWithoutId()
        {
            var table = new Table("user_profile",
                new[]
                {
                    new Column("roleId", "Guid", false, true, false), new Column("groupId", "Guid", false, true, false)
                });

            var actual =
                TemplateGenerator.GenerateClass("Project.Generated.Global", table, "Odin.Api.IIdentity", null, null);

            Console.WriteLine(actual);

            const string expected = @"using System;

namespace Project.Generated.Global
{

	public sealed class UserProfile
	{

		public UserProfile(Guid roleId, Guid groupId)
		{
			RoleId = roleId;
			GroupId = groupId;
		}

		public Guid RoleId { get; }

		public Guid GroupId { get; }

	}

}";
            Assert.AreEqual(expected, actual);
        }
    }
}
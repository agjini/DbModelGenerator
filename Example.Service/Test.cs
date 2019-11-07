using System;
using Example.Service.Generated.Db.Tenant;

namespace Example.Service
{
    public class Test
    {
        public static void Main(string[] args)
        {
            var roleDb = new RoleDb("test");
            Console.WriteLine("Works : {0}", roleDb);
        }
    }
}
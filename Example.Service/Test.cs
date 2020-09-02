using System;
using Example.Service.Generated.Db.Tenant;

namespace Example.Service
{
    public class Test
    {
        public static void Main(string[] args)
        {
            var userGroupDb = new UserGroupDb("test");
            Console.WriteLine("Works : {0}", userGroupDb);
        }
    }
}
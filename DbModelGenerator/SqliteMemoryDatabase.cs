using System;
using DbUp;
using DbUp.Builder;
using DbUp.Engine.Output;
using Microsoft.Data.Sqlite;

namespace DbModelGenerator
{
    public sealed class SqliteMemoryDatabase
    {
        private readonly string databasePath;

        public SqliteMemoryDatabase(string databasePath)
        {
            this.databasePath = databasePath;
        }

        public void UpdgradeSchema(string path, string database)
        {
            var engineBuilder = DeployChanges.To.SQLiteDatabase(NewConnectionString(database));
            PerformUpdgrade(path, database, engineBuilder);
        }

        public static void PerformUpdgrade(string path, string database, UpgradeEngineBuilder engineBuilder)
        {
            var upgrader = engineBuilder
                .WithScriptsFromFileSystem(path)
                .WithTransaction()
                .LogTo(new DbUpgradeLogger())
                .Build();

            if (upgrader.PerformUpgrade().Successful)
            {
                Console.WriteLine($"{database} migrated successfully");
            }
        }

        public SqliteConnection NewConnection(string database)
        {
            var connectionString = NewConnectionString(database);
            return new SqliteConnection(connectionString);
        }

        private string NewConnectionString(string database)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = $"{databasePath}/{database}.db", Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };

            return builder.ConnectionString;
        }
    }

    internal sealed class DbUpgradeLogger : IUpgradeLog
    {
        public void WriteInformation(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void WriteError(string format, params object[] args)
        {
            Console.Error.WriteLine(format, args);
        }

        public void WriteWarning(string format, params object[] args)
        {
            Console.Error.WriteLine(format, args);
        }
    }
}
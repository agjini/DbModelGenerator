using System;
using DbModelGenerator.Preprocessor;
using DbUp;
using DbUp.Builder;
using DbUp.Engine.Output;
using DbUp.SQLite;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
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

        public void UpdgradeSchema(string path, string database, TaskLoggingHelper log)
        {
            var engineBuilder = DeployChanges.To.SQLiteDatabase(NewConnectionString(database, log));
            PerformUpdgrade(path, database, engineBuilder, log);
        }

        public static void PerformUpdgrade(string path, string database, UpgradeEngineBuilder engineBuilder,
            TaskLoggingHelper log)
        {
            var upgrader = engineBuilder
                .WithScriptsFromFileSystem(path)
                .WithVariablesDisabled()
                .WithPreprocessor(new DdlPreprocessor())
                .WithPreprocessor(new SQLitePreprocessor())
                .WithTransaction()
                .LogTo(new DbUpgradeLogger(log))
                .Build();

            if (upgrader.PerformUpgrade().Successful)
            {
                log.LogMessage(MessageImportance.Normal, $"{database} migrated successfully");
            }
        }

        public SqliteConnection NewConnection(string database, TaskLoggingHelper log)
        {
            var connectionString = NewConnectionString(database, log);
            return new SqliteConnection(connectionString);
        }

        private string NewConnectionString(string database, TaskLoggingHelper log)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = $"{databasePath}/{database}.db",
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };
            log.LogMessage($"Connection : {builder}", builder);
            return builder.ConnectionString;
        }
    }

    internal sealed class DbUpgradeLogger : IUpgradeLog
    {
        private readonly TaskLoggingHelper log;

        public DbUpgradeLogger(TaskLoggingHelper log)
        {
            this.log = log;
        }

        public void WriteInformation(string format, params object[] args)
        {
            log.LogMessage(MessageImportance.Normal, format, args);
        }

        public void WriteError(string format, params object[] args)
        {
            log.LogError(format, args);
            throw new ArgumentException(string.Format(format, args));
        }

        public void WriteWarning(string format, params object[] args)
        {
            log.LogWarning(format, args);
        }
    }
}
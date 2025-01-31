namespace KDG.EventSourcing.Migrations
{
    public static class MigrationHelpers
    {
        public static void Migrate(string connectionString)
        {
            var fsso = new DbUp.ScriptProviders.FileSystemScriptOptions();
            fsso.IncludeSubDirectories = true;

            DbUp.DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithTransaction()
                .LogToConsole()
                .WithScriptsFromFileSystem("Migrations/Scripts", fsso)
                .Build()
                .PerformUpgrade();
        }
    }
}

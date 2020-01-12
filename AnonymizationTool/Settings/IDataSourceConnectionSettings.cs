namespace AnonymizationTool.Settings
{
    public interface IDataSourceConnectionSettings
    {
        DatabaseType Type { get; set; }

        string ConnectionString { get; set; }

        public enum DatabaseType
        {
            Access,
            MSSQL,
            MySQL,
            SQLite
        }
    }
}

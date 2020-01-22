using AnonymizationTool.Data;

namespace AnonymizationTool.Settings
{
    public interface IDataSourceConnectionSettings
    {
        DatabaseType Type { get; set; }

        string ConnectionString { get; set; }
    }
}

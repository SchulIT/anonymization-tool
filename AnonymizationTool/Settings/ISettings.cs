namespace AnonymizationTool.Settings
{
    public interface ISettings
    {
        bool KeepGender { get; set; }

        IEmailSettings Email { get; }

        IDataSourceConnectionSettings SchILDConnection { get; }

        IDataSourceConnectionSettings DatabaseConnection { get; }
    }
}

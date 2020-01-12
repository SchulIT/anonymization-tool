using Newtonsoft.Json;
using static AnonymizationTool.Settings.IDataSourceConnectionSettings;

namespace AnonymizationTool.Settings
{
    public class JsonSettings : ISettings
    {
        [JsonProperty("keep_gender")]
        public bool KeepGender { get; set; }

        public IEmailSettings Email { get; } = new JsonEmailSettings();

        [JsonProperty("schild")]
        public IDataSourceConnectionSettings SchILDConnection { get; } = new JsonDataSourceConnectionSettings();

        [JsonProperty("database")]
        public IDataSourceConnectionSettings DatabaseConnection { get; } = new JsonDataSourceConnectionSettings();
    }

    public class JsonDataSourceConnectionSettings : IDataSourceConnectionSettings
    {
        [JsonProperty("type")]
        public DatabaseType Type { get; set; }

        [JsonProperty("connection_string")]
        public string ConnectionString { get; set; }
    }

    public class JsonEmailSettings : IEmailSettings
    {
        [JsonProperty("type")]
        public IEmailSettings.AnonymizationType Type { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("random_length")]
        public int RandomLength { get; set; }
    }
}

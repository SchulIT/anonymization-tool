using AnonymizationTool.Data;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.ComponentModel;
using static AnonymizationTool.Settings.IEmailSettings;

namespace AnonymizationTool.Settings
{
    public class JsonSettings : ObservableObject, ISettings
    {
        private bool keepGender = true;

        [JsonProperty("keep_gender")]
        public bool KeepGender
        {
            get => keepGender;
            set => Set(ref keepGender, value);
        }

        public IEmailSettings Email { get; } = new JsonEmailSettings();

        [JsonProperty("schild")]
        public IDataSourceConnectionSettings SchILDConnection { get; } = new JsonDataSourceConnectionSettings();

        [JsonProperty("database")]
        public IDataSourceConnectionSettings DatabaseConnection { get; } = new JsonDataSourceConnectionSettings
        {
            ConnectionString = "datasource=:memory:",
            Type = DatabaseType.SQLite
        };

        public JsonSettings()
        {
            var jsonEmailSettings = Email as JsonEmailSettings;
            if (jsonEmailSettings != null)
            {
                jsonEmailSettings.PropertyChanged += OnPropertyChanged;
            }

            var jsonSchILDConnection = SchILDConnection as JsonDataSourceConnectionSettings;
            if (jsonSchILDConnection != null)
            {
                jsonSchILDConnection.PropertyChanged += OnPropertyChanged;
            }

            var jsonDatabaseConnection = DatabaseConnection as JsonDataSourceConnectionSettings;
            if (jsonDatabaseConnection != null)
            {
                jsonDatabaseConnection.PropertyChanged += OnPropertyChanged;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged();
        }
    }

    public class JsonDataSourceConnectionSettings : ObservableObject, IDataSourceConnectionSettings
    {
        private DatabaseType type;

        [JsonProperty("type")]
        public DatabaseType Type
        {
            get => type;
            set => Set(ref type, value);
        }

        private string connectionString;

        [JsonProperty("connection_string")]
        public string ConnectionString
        {
            get => connectionString;
            set => Set(ref connectionString, value);
        }
    }

    public class JsonEmailSettings : ObservableObject, IEmailSettings
    {
        private AnonymizationType type = AnonymizationType.FirstnameLastname;

        [JsonProperty("type")]
        public IEmailSettings.AnonymizationType Type
        {
            get => type;
            set => Set(ref type, value);
        }

        private string domain = "example.org";

        [JsonProperty("domain")]
        public string Domain
        {
            get => domain;
            set => Set(ref domain, value);
        }

        private int randomLength = 10;

        [JsonProperty("random_length")]
        public int RandomLength
        {
            get => randomLength;
            set => Set(ref randomLength, value);
        }
    }
}

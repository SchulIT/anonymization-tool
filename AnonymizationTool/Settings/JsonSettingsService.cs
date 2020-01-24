using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace AnonymizationTool.Settings
{
    public class JsonSettingsService : ISettingsService
    {
        public const string JsonFileName = "settings.json";
        public const string ApplicationName = "AnonymizationTool";
        public const string ApplicationVendor = "SchulIT";

        private JsonSettings jsonSettings;

        public ISettings Settings { get { return jsonSettings; } }

        public event SettingsChangedEventHandler Changed;

        protected void OnChanged(SettingsChangedEventArgs args)
        {
            Changed?.Invoke(this, args);
        }

        public void LoadSettings()
        {
            if(jsonSettings != null)
            {
                jsonSettings.PropertyChanged -= OnJsonSettingsPropertyChanged;
            }

            var path = GetPath();
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(path))
            {
                var jsonSettings = new JsonSettings();
                using (var writer = new StreamWriter(path))
                {
                    writer.Write(JsonConvert.SerializeObject(jsonSettings, Formatting.Indented));
                }
            }

            using (var reader = new StreamReader(path))
            {
                var json = reader.ReadToEnd();
                jsonSettings = JsonConvert.DeserializeObject<JsonSettings>(json);
            }

            // Write settings back to create possibly missing new setting items
            using (var writer = new StreamWriter(path))
            {
                writer.Write(JsonConvert.SerializeObject(jsonSettings, Formatting.Indented));
            }

            if (jsonSettings != null)
            {
                jsonSettings.PropertyChanged += OnJsonSettingsPropertyChanged;
            }
        }

        private async void OnJsonSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await SaveAsync();
            OnChanged(new SettingsChangedEventArgs());
        }

        private string GetPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                ApplicationVendor,
                ApplicationName,
                "settings.json"
            );
        }

        public async Task SaveAsync()
        {
            var path = GetPath();

            // Write settings back to create possibly missing new setting items
            using (var writer = new StreamWriter(path))
            {
                await writer.WriteAsync(JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
        }
    }
}

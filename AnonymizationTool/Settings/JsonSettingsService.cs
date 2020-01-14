using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AnonymizationTool.Settings
{
    public class JsonSettingsService : ISettingsService
    {
        public const string JsonFileName = "settings.json";
        public const string ApplicationName = "AnonymizationTool";
        public const string ApplicationVendor = "SchulIT";

        public ISettings Settings { get; private set; }

        public event SettingsChangedEventHandler Changed;

        protected void OnChanged(SettingsChangedEventArgs args)
        {
            Changed?.Invoke(this, args);
        }

        public void LoadSettings()
        {
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
                var settings = JsonConvert.DeserializeObject<JsonSettings>(json);
                Settings = settings;
            }

            // Write settings back to create possibly missing new setting items
            using (var writer = new StreamWriter(path))
            {
                writer.Write(JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
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

using Microsoft.Extensions.Logging;
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

        private readonly ILogger<JsonSettingsService> logger;

        public JsonSettingsService(ILogger<JsonSettingsService> logger)
        {
            this.logger = logger;
        }

        public void LoadSettings()
        {
            var path = GetPath();

            try
            {
                var directory = Path.GetDirectoryName(path);

                if (!Directory.Exists(directory))
                {
                    logger.LogDebug($"Creating directory {directory} as it does not exist.");
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(path))
                {
                    logger.LogDebug("Settings file does not exist, creating a default one.");

                    var jsonSettings = new JsonSettings();
                    using (var writer = new StreamWriter(path))
                    {
                        writer.Write(JsonConvert.SerializeObject(jsonSettings, Formatting.Indented));
                    }

                    logger.LogDebug("Settings file created successfully.");
                }

                logger.LogDebug($"Reading settings from file {path}.");

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

                logger.LogDebug("Settings read successfully.");
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed loading settings from file {path}.");
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
            logger.LogDebug($"Saving settings to {path}");

            // Write settings back to create possibly missing new setting items
            using (var writer = new StreamWriter(path))
            {
                await writer.WriteAsync(JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }

            logger.LogDebug("Settings saved successfully.");
        }
    }
}

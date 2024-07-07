using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;

namespace WAIModelDownloader
{
    public class SettingsManager
    {
        public Settings LoadSettings(string filePath)
        {
            if (File.Exists(filePath))
            {
                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new StringEnumConverter() }
                };
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Settings>(json, settings);
            }
            return new Settings();
        }

        public void SaveSettings(string filePath, Settings settings)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                Formatting = Formatting.Indented
            };
            string json = JsonConvert.SerializeObject(settings, jsonSettings);
            File.WriteAllText(filePath, json);
        }
    }
}

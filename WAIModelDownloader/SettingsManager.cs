using Newtonsoft.Json;
using System.IO;

namespace WAIModelDownloader
{
    public class SettingsManager
    {
        public Settings LoadSettings(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Settings>(json);
            }
            return new Settings();
        }

        public void SaveSettings(string filePath, Settings settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}

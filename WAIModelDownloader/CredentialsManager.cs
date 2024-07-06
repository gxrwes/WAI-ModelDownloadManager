using Newtonsoft.Json;
using System.IO;

namespace WAIModelDownloader
{
    public static class CredentialsManager
    {
        public static LoginCredentials LoadCredentials(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<LoginCredentials>(json);
            }
            return new LoginCredentials();
        }

        public static void SaveCredentials(string filePath, LoginCredentials credentials)
        {
            string json = JsonConvert.SerializeObject(credentials, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}

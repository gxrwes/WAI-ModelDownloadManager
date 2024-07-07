using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace WAIModelDownloader
{
    public class Settings
    {
        public string BaseDirectory { get; set; } = "C:\\DEVD\\AI\\Fooocus_win64_2-1-831\\Fooocus\\";
        public List<string> PathCheckpoints { get; set; } = new List<string>();
        public List<string> PathLoras { get; set; } = new List<string>();
        public List<string> PathUpscalers { get; set; } = new List<string>();
        public string PathTextualInversion { get; set; }
        public string PathEmbeddings { get; set; }
        public string PathVAE { get; set; }
        public string PathControlNet { get; set; }
        public string PathStyleGAN { get; set; }
        public string PathInpainting { get; set; }
        public string PathSuperResolution { get; set; }

        public void UpdatePaths(string baseDirectory)
        {
            BaseDirectory = baseDirectory;
            PathCheckpoints = new List<string> { Path.Combine(baseDirectory, "models", "checkpoints") };
            PathLoras = new List<string> { Path.Combine(baseDirectory, "models", "loras") };
            PathUpscalers = new List<string> { Path.Combine(baseDirectory, "models", "upscalers") };
            PathTextualInversion = Path.Combine(baseDirectory, "models", "textual_inversion");
            PathEmbeddings = Path.Combine(baseDirectory, "models", "embeddings");
            PathVAE = Path.Combine(baseDirectory, "models", "vae");
            PathControlNet = Path.Combine(baseDirectory, "models", "controlnet");
            PathStyleGAN = Path.Combine(baseDirectory, "models", "stylegan");
            PathInpainting = Path.Combine(baseDirectory, "models", "inpainting");
            PathSuperResolution = Path.Combine(baseDirectory, "models", "super_resolution");
        }

        public void Save(string filePath)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static Settings Load(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Settings>(json);
            }
            return new Settings();
        }
    }
}

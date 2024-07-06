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
        public string PathEmbeddings { get; set; }
        public string PathVaeApprox { get; set; }
        public string PathVae { get; set; }
        public string PathUpscaleModels { get; set; }
        public string PathInpaint { get; set; }
        public string PathControlnet { get; set; }
        public string PathClipVision { get; set; }
        public string PathFooocusExpansion { get; set; }
        public string PathWildcards { get; set; }
        public string PathSafetyChecker { get; set; }
        public string PathOutputs { get; set; }

        public void UpdatePaths(string baseDirectory)
        {
            BaseDirectory = baseDirectory;
            PathCheckpoints = new List<string> { Path.Combine(baseDirectory, "models", "checkpoints") };
            PathLoras = new List<string> { Path.Combine(baseDirectory, "models", "loras") };
            PathEmbeddings = Path.Combine(baseDirectory, "models", "embeddings");
            PathVaeApprox = Path.Combine(baseDirectory, "models", "vae_approx");
            PathVae = Path.Combine(baseDirectory, "models", "vae");
            PathUpscaleModels = Path.Combine(baseDirectory, "models", "upscale_models");
            PathInpaint = Path.Combine(baseDirectory, "models", "inpaint");
            PathControlnet = Path.Combine(baseDirectory, "models", "controlnet");
            PathClipVision = Path.Combine(baseDirectory, "models", "clip_vision");
            PathFooocusExpansion = Path.Combine(baseDirectory, "models", "prompt_expansion", "fooocus_expansion");
            PathWildcards = Path.Combine(baseDirectory, "wildcards");
            PathSafetyChecker = Path.Combine(baseDirectory, "models", "safety_checker");
            PathOutputs = Path.Combine(baseDirectory, "outputs");
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

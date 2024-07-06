using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using WAIModelDownloader.Jobs;

namespace WAIModelDownloader
{
    public class JobManager
    {
        public ObservableCollection<DownloadModelJob> LoadJobs(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<ObservableCollection<DownloadModelJob>>(json);
            }
            return new ObservableCollection<DownloadModelJob>();
        }

        public void SaveJobs(string filePath, ObservableCollection<DownloadModelJob> jobs)
        {
            string json = JsonConvert.SerializeObject(jobs, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}

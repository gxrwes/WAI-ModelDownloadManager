using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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
                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new StringEnumConverter() },
                    Error = HandleDeserializationError
                };
                string json = File.ReadAllText(filePath);
                var jobs = JsonConvert.DeserializeObject<ObservableCollection<DownloadModelJob>>(json, settings);

                foreach (var job in jobs)
                {
                    if (job.LastDownloaded == null)
                    {
                        job.LastDownloaded = DateTime.UtcNow;
                    }
                }

                return jobs;
            }
            return new ObservableCollection<DownloadModelJob>();
        }

        public void SaveJobs(string filePath, ObservableCollection<DownloadModelJob> jobs)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                Formatting = Formatting.Indented
            };
            string json = JsonConvert.SerializeObject(jobs, settings);
            File.WriteAllText(filePath, json);
        }

        private void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            var job = errorArgs.CurrentObject as DownloadModelJob;
            if (job != null)
            {
                job.Enabled = false;
                job.ModelType = ModelType.Lora;
                job.Errors = currentError;
            }
            errorArgs.ErrorContext.Handled = true;
        }
    }
}

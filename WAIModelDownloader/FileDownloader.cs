using System;
using System.IO;
using System.Threading.Tasks;
using WAIModelDownloader.Jobs;

namespace WAIModelDownloader
{
    public class FileDownloader
    {
        private readonly HttpClientHandlerService _httpClientHandlerService;
        private readonly JobDataExtractor _jobDataExtractor;

        public FileDownloader(HttpClientHandlerService httpClientHandlerService, JobDataExtractor jobDataExtractor)
        {
            _httpClientHandlerService = httpClientHandlerService;
            _jobDataExtractor = jobDataExtractor;
        }

        public async Task DownloadFilesAsync(DownloadModelJob job, DownloadProgressWindow downloadProgressWindow, Settings appSettings)
        {
            try
            {
                if (job.Downloaded)
                {
                    Log(downloadProgressWindow, $"Skipping download for job: {job.Name}, already downloaded.");
                    return;
                }

                Log(downloadProgressWindow, $"Preparing to download file for job: {job.Name}");
                string tempFolder = Path.Combine(Path.GetTempPath(), "ModelDownloads");
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                string fileName = Path.GetFileName(job.ModelDownloadLink);
                string tempFilePath = Path.Combine(tempFolder, fileName);

                await _httpClientHandlerService.DownloadFileAsync(job.ModelDownloadLink, tempFilePath);
                downloadProgressWindow.UpdateProgress(50, $"Downloaded {job.Name}", $"Downloaded to {tempFilePath}");

                string targetFolder = GetTargetFolder(job.ModelType, appSettings);
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                string extractedName = _jobDataExtractor.ExtractNameFromUrl(job.ModelUrl);
                string newFileName = $"{job.Name}_{extractedName}.safetensors";
                string targetFilePath = Path.Combine(targetFolder, newFileName);

                if (File.Exists(targetFilePath))
                {
                    File.Delete(targetFilePath);
                }

                File.Move(tempFilePath, targetFilePath);
                job.ModelDownloadPath = targetFilePath; // Set the correct path
                Log(downloadProgressWindow, $"File downloaded and moved to target folder for job: {job.Name}");
            }
            catch (Exception ex)
            {
                LogError(downloadProgressWindow, $"Error downloading file for job: {job.Name}", ex);
                throw;
            }
        }

        private string GetTargetFolder(string modelType, Settings appSettings)
        {
            switch (modelType.ToLower())
            {
                case "lora":
                    return appSettings.PathLoras[0];
                case "checkpoints":
                    return appSettings.PathCheckpoints[0];
                default:
                    return appSettings.BaseDirectory;
            }
        }

        private void Log(DownloadProgressWindow window, string message)
        {
            Console.WriteLine(message);
            window.UpdateProgress((int)window.ProgressBar.Value, window.StatusTextBlock.Text, message);
        }

        private void LogError(DownloadProgressWindow window, string message, Exception ex)
        {
            Log(window, message + ": " + ex.Message);
        }
    }
}

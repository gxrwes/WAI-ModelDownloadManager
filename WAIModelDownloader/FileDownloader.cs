using System;
using System.IO;
using System.Net.Http;
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

                string fileName = Path.GetFileName(job.Name);
                string tempFilePath = Path.Combine(tempFolder, fileName);

                await DownloadFileAsync(job.ModelDownloadLink, tempFilePath, downloadProgressWindow);

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
                job.Downloaded = true;
                job.LastDownloaded = DateTime.Now;
                job.Errors = null;

                Log(downloadProgressWindow, $"File downloaded and moved to target folder for job: {job.Name}");
            }
            catch (Exception ex)
            {
                LogError(downloadProgressWindow, $"Error downloading file for job: {job.Name}", ex);
                job.Errors = ex.Message;
                throw;
            }
        }

        private async Task DownloadFileAsync(string url, string destinationPath, DownloadProgressWindow downloadProgressWindow)
        {
            using (var response = await _httpClientHandlerService.HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1;

                    var buffer = new byte[8192];
                    var totalRead = 0L;
                    var bytesRead = 0;
                    var progress = 0;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;

                        if (canReportProgress)
                        {
                            var currentProgress = (int)((totalRead * 100) / totalBytes);
                            if (currentProgress != progress)
                            {
                                progress = currentProgress;
                                downloadProgressWindow.UpdateProgress(progress, $"Downloading...", $"Downloaded {totalRead / (1024 * 1024)} MB of {totalBytes / (1024 * 1024)} MB");
                            }
                        }
                    }
                }
            }
        }

        private string GetTargetFolder(ModelType modelType, Settings appSettings)
        {
            switch (modelType)
            {
                case ModelType.Lora:
                    return appSettings.PathLoras[0];
                case ModelType.Checkpoints:
                    return appSettings.PathCheckpoints[0];
                case ModelType.Upscalers:
                    return appSettings.PathUpscalers[0];
                case ModelType.TextualInversion:
                    return appSettings.PathTextualInversion;
                case ModelType.Embeddings:
                    return appSettings.PathEmbeddings;
                case ModelType.VAE:
                    return appSettings.PathVAE;
                case ModelType.ControlNet:
                    return appSettings.PathControlNet;
                case ModelType.StyleGAN:
                    return appSettings.PathStyleGAN;
                case ModelType.Inpainting:
                    return appSettings.PathInpainting;
                case ModelType.SuperResolution:
                    return appSettings.PathSuperResolution;
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

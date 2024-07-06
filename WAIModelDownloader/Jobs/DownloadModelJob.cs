using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using WAIModelDownloader.Commands;

namespace WAIModelDownloader.Jobs
{
    public class DownloadModelJob
    {
        public string Name { get; set; }
        public string ModelUrl { get; set; }
        public string ModelType { get; set; }
        public string ModelDownloadPath { get; set; }
        public bool Downloaded { get; set; }
        public string ModelDownloadLink { get; set; }
        public DateTime LastDownloaded { get; set; }
        public string Errors { get; set; }
        public bool Enabled { get; set; }
        public ICommand OpenPathCommand { get; set; }

        public DownloadModelJob()
        {
            OpenPathCommand = new RelayCommand(OpenPath);
        }

        private void OpenPath()
        {
            if (!string.IsNullOrEmpty(ModelDownloadPath))
            {
                if (File.Exists(ModelDownloadPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select, \"{ModelDownloadPath}\"",
                        UseShellExecute = true
                    });
                }
                else if (Directory.Exists(ModelDownloadPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{ModelDownloadPath}\"",
                        UseShellExecute = true
                    });
                }
                else
                {
                    var directoryPath = Path.GetDirectoryName(ModelDownloadPath);
                    if (directoryPath != null && Directory.Exists(directoryPath))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            Arguments = $"\"{directoryPath}\"",
                            UseShellExecute = true
                        });
                    }
                }
            }
        }
    }
}

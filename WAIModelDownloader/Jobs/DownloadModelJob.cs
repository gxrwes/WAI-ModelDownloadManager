﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Input;
using WAIModelDownloader.Commands;

namespace WAIModelDownloader.Jobs
{
    public class DownloadModelJob : IDeserializationCallback
    {
        public string Name { get; set; }
        public string ModelUrl { get; set; }

        public ModelType ModelType { get; set; }

        public string ModelDownloadPath { get; set; }
        public bool Downloaded { get; set; }
        public string ModelDownloadLink { get; set; }
        public DateTime? LastDownloaded { get; set; }
        public string Errors { get; set; }
        public bool Enabled { get; set; }

        public ICommand OpenPathCommand { get; set; }
        public ICommand ViewMetadataCommand { get; set; }

        public DownloadModelJob()
        {
            OpenPathCommand = new RelayCommand(OpenPath);
            ViewMetadataCommand = new RelayCommand(ViewMetadata, CanViewMetadata);
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

        private bool CanViewMetadata()
        {
            return Downloaded && !string.IsNullOrEmpty(ModelDownloadPath);
        }

        private void ViewMetadata()
        {
            var viewWindow = new ViewSafetensorWindow(ModelDownloadPath);
            viewWindow.Show();
        }

        public void OnDeserialization(object sender)
        {
            if (!Enum.IsDefined(typeof(ModelType), ModelType))
            {
                ModelType = ModelType.Lora;
                Enabled = false;
                Errors = "Invalid ModelType detected during deserialization. Set to default (Lora) and disabled.";
            }
        }
    }
}

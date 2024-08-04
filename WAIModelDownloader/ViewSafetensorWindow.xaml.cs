using System;
using System.Windows;
using Newtonsoft.Json;
using WAIModelDownloader.SafeTensorManagement;

namespace WAIModelDownloader
{
    public partial class ViewSafetensorWindow : Window
    {
        public ViewSafetensorWindow(string filePath)
        {
            InitializeComponent();

            var settings = Settings.Load("settings.json");
            var extractor = new SafeTensorMetaDataExtractor(settings.PythonPath);

            try
            {
                var metadata = extractor.ExtractMetadata(filePath);
                MetadataTextBox.Text = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            }
            catch (Exception ex)
            {
                MetadataTextBox.Text = $"Error extracting metadata: {ex.Message}";
            }
        }
    }
}

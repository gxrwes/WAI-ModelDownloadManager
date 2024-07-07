using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using WAIModelDownloader.Jobs;

namespace WAIModelDownloader
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<DownloadModelJob> DownloadModelJobs { get; set; }
        public Settings AppSettings { get; set; }
        public LoginCredentials Credentials { get; set; }
        private HttpClientHandlerService _httpClientHandlerService;
        private SettingsManager _settingsManager;
        private JobManager _jobManager;
        private JobDataExtractor _jobDataExtractor;
        private FileDownloader _fileDownloader;

        public MainWindow()
        {
            InitializeComponent();
            _settingsManager = new SettingsManager();
            _jobManager = new JobManager();
            _jobDataExtractor = new JobDataExtractor();
            _httpClientHandlerService = new HttpClientHandlerService();
            _fileDownloader = new FileDownloader(_httpClientHandlerService, _jobDataExtractor);

            LoadSettings();
            LoadCredentials();
            LoadData();
            dataGrid.ItemsSource = DownloadModelJobs;
        }

        private void LoadSettings()
        {
            AppSettings = _settingsManager.LoadSettings("settings.json");
        }

        private void LoadCredentials()
        {
            Credentials = CredentialsManager.LoadCredentials("credentials.json");
        }

        private void LoadData()
        {
            DownloadModelJobs = _jobManager.LoadJobs("jobs.json");
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsViewModel settingsWindow = new SettingsViewModel(AppSettings, Credentials);
            if (settingsWindow.ShowDialog() == true)
            {
                LoadSettings();
                LoadCredentials();
                LoadData();
            }
        }

        private void EditJob_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is DownloadModelJob selectedJob)
            {
                var editableCollectionView = CollectionViewSource.GetDefaultView(DownloadModelJobs) as IEditableCollectionView;
                if (editableCollectionView != null && editableCollectionView.IsEditingItem)
                {
                    editableCollectionView.CommitEdit();
                }

                EditJobWindow editJobWindow = new EditJobWindow(selectedJob);
                if (editJobWindow.ShowDialog() == true)
                {
                    dataGrid.Items.Refresh();
                    SaveData(); // Save data after editing
                }
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var downloadProgressWindow = new DownloadProgressWindow();
            downloadProgressWindow.Show();

            if (Credentials == null || string.IsNullOrEmpty(Credentials.LoginLink))
            {
                MessageBox.Show("Please provide a valid login link in the credentials.json file.");
                Log(downloadProgressWindow, "No valid credentials found.");
                return;
            }

            Log(downloadProgressWindow, "Starting authentication with login link.");
            bool authSuccess = await _httpClientHandlerService.AuthenticateAsync(Credentials.LoginLink);

            if (!authSuccess)
            {
                MessageBox.Show("Authentication failed. Please check your login link.");
                Log(downloadProgressWindow, "Authentication failed.");
                return;
            }

            Log(downloadProgressWindow, "Authentication successful. Starting download of enabled jobs.");

            var enabledJobs = new ObservableCollection<DownloadModelJob>(DownloadModelJobs.Where(job => job.Enabled && !job.Downloaded));

            for (int i = 0; i < enabledJobs.Count; i++)
            {
                var job = enabledJobs[i];
                int percentage = (i * 100) / enabledJobs.Count;
                downloadProgressWindow.UpdateProgress(percentage, $"Downloading {job.Name}", "");
                Log(downloadProgressWindow, $"Starting download for job: {job.Name} with URL: {job.ModelUrl}");

                try
                {
                    string html = await _httpClientHandlerService.GetHtmlAsync(job.ModelUrl);
                    _jobDataExtractor.ExtractJobData(job, html);
                    await _fileDownloader.DownloadFilesAsync(job, downloadProgressWindow, AppSettings);

                    job.Downloaded = true;
                    job.LastDownloaded = DateTime.Now;
                    job.Errors = null;
                    Log(downloadProgressWindow, $"Successfully downloaded job: {job.Name}");
                }
                catch (Exception ex)
                {
                    job.Errors = ex.Message;
                    downloadProgressWindow.UpdateProgress(percentage, $"Error downloading {job.Name}", ex.Message);
                    LogError(downloadProgressWindow, $"Error downloading job: {job.Name}", ex);
                }

                dataGrid.Items.Refresh();
                SaveData();
            }

            downloadProgressWindow.UpdateProgress(100, "Download Complete", "All jobs completed.");
            Log(downloadProgressWindow, "All downloads completed.");
        }

        private void SaveData()
        {
            try
            {
                Log("Saving job data to jobs.json");
                _jobManager.SaveJobs("jobs.json", DownloadModelJobs);
                Log("Job data saved successfully.");
            }
            catch (Exception ex)
            {
                LogError("Error saving job data", ex);
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

        private void Log(string message)
        {
            Console.WriteLine(message);
        }

        private void LogError(string message, Exception ex)
        {
            Log(message + ": " + ex.Message);
        }
        private void ExportJobs_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;
                try
                {
                    _jobManager.SaveJobs(filePath, DownloadModelJobs);
                    MessageBox.Show("Jobs exported successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting jobs: {ex.Message}");
                }
            }
        }

        private void ImportJobs_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                try
                {
                    var importedJobs = _jobManager.LoadJobs(filePath);
                    foreach (var job in importedJobs)
                    {
                        if (!Enum.IsDefined(typeof(ModelType), job.ModelType))
                        {
                            job.ModelType = ModelType.Lora; // Default to Lora if invalid value
                            job.Enabled = false;
                            job.Errors = "Invalid ModelType detected during import. Set to default (Lora) and disabled.";
                        }
                        DownloadModelJobs.Add(job);
                    }
                    dataGrid.Items.Refresh();
                    SaveData(); // Save the imported jobs to the main jobs.json
                    MessageBox.Show("Jobs imported successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing jobs: {ex.Message}");
                }
            }
        }



        private void CreateNewJob_Click(object sender, RoutedEventArgs e)
        {
            var newJob = new DownloadModelJob
            {
                Name = "New Job",
                ModelUrl = "",
                ModelType = ModelType.Lora,
                ModelDownloadPath = "",
                Downloaded = false,
                ModelDownloadLink = "",
                LastDownloaded = DateTime.Now,
                Errors = null,
                Enabled = false
            };
            DownloadModelJobs.Add(newJob);
            dataGrid.Items.Refresh();
            SaveData(); // Save data after adding a new job
        }
    }
}

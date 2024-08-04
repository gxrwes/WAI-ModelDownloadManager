using System.Windows;

namespace WAIModelDownloader
{
    public partial class DownloadProgressWindow : Window
    {
        public DownloadProgressWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int percentage, string status, string log)
        {
            ProgressBar.Value = percentage;
            StatusTextBlock.Text = status;
            LogTextBox.AppendText(log + "\n");
            LogTextBox.ScrollToEnd();
        }

        public void UpdateJobProgress(int completedJobs, int totalJobs)
        {
            JobProgressTextBlock.Text = $"Jobs Completed: {completedJobs}/{totalJobs}";
        }
    }
}

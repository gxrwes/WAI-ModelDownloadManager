using System.Windows;
using WAIModelDownloader.Jobs;

namespace WAIModelDownloader
{
    public partial class EditJobWindow : Window
    {
        private DownloadModelJob _job;

        public EditJobWindow(DownloadModelJob job)
        {
            InitializeComponent();
            _job = job;
            DataContext = _job;

            if (_job.ModelType == 0) // Default to Lora if not set
            {
                _job.ModelType = ModelType.Lora;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

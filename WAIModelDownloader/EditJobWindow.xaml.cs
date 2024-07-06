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
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

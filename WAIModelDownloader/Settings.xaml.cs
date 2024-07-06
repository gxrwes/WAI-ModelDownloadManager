using System.Windows;

namespace WAIModelDownloader
{
    public partial class SettingsViewModel : Window
    {
        private Settings _settings;
        private LoginCredentials _credentials;

        public SettingsViewModel(Settings settings, LoginCredentials credentials)
        {
            InitializeComponent();
            _settings = settings;
            _credentials = credentials;
            DataContext = _settings;
            LoginLinkTextBox.Text = _credentials?.LoginLink;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _credentials.LoginLink = LoginLinkTextBox.Text;

            _settings.Save("settings.json");
            CredentialsManager.SaveCredentials("credentials.json", _credentials);

            DialogResult = true;
            Close();
        }
    }
}

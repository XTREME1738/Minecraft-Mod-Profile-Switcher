using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Minecraft_Profile_Switcher
{
    public partial class SearchSettingsWindow
    {
        private readonly string _apiKeyFile;
        
        public SearchSettingsWindow(string apiKeyFile)
        {
            InitializeComponent();
            _apiKeyFile = apiKeyFile;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResponseTextBox.Text is "" or null)
            {
                MessageBox.Show("API Key Box is empty. Cannot save.", "No API Key", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            File.WriteAllText(_apiKeyFile, ResponseTextBox.Text);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GetAPIKeyButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://console.curseforge.com/#/api-keys");
        }
    }
}
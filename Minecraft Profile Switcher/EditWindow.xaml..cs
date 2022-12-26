using System.Text.RegularExpressions;
using System.Windows;

namespace Minecraft_Profile_Switcher;

public partial class EditWindow
{
    public EditWindow(string question, string title, string defaultValue = "", string minecraftVersion = "")
    {
        InitializeComponent();
        Title = title;
        QuestionLabel.Content = question;
        ResponseTextBox.Text = defaultValue;
        MinecraftVersionTextBox.Text = minecraftVersion;
    }

    public string ResponseText { get; private set; }
    public string MinecraftVersion { get; private set; }

    private static bool IsValidMinecraftVersion(string version)
    {
        // Validate that the version string is not null or empty
        if (string.IsNullOrEmpty(version)) return false;

        // Use a regular expression to check that the version string is in the correct format
        var regex = new Regex(@"^(\d+\.\d+\.\d+)$");
        return regex.IsMatch(version);
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsValidMinecraftVersion(MinecraftVersionTextBox.Text))
        {
            DialogResult = true;
            ResponseText = ResponseTextBox.Text;
            MinecraftVersion = MinecraftVersionTextBox.Text;
            Close();
            return;
        }

        MessageBox.Show("The entered minecraft version is not in the valid format.", "Version Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
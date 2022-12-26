using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace Minecraft_Profile_Switcher;

public class CurseForgeMod
{
    public string ModName { get; set; }
    public string Downloads { get; set; }
    public string DownloadUrl { get; set; }
    public string ImageUrl { get; set; }
    public string ModUrl { get; set; }
    public string ModFileName { private get; set; }
    public bool Installed { get; set; }
    public bool NeedsUpdate { get; set; }

    public void Install(string profileDirectory)
    {
        var wc = new WebClient();
        wc.DownloadFile(DownloadUrl, Path.Combine(profileDirectory, ModFileName));
    }

    public void Uninstall(string profileDirectory)
    {
        try
        {
            File.Delete(Path.Combine(profileDirectory));
        }
        catch (IOException)
        {
            MessageBox.Show("File is open in another program or does not exist", "No Permission", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public partial class ModDownloaderWindow
{

    private CurseForgeMod _selectedMod;
    private readonly string _profileDirectory;
    private readonly string _profileVersion;
    private readonly string _apiKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ModProfileSwitcher", "apikey");

    public ModDownloaderWindow(string profileDirectory, string profileVersion)
    {
        InitializeComponent();
        _profileDirectory = profileDirectory;
        _profileVersion = profileVersion;
    }
    
    private void SearchMods(string query)
    {
        // Make the API call using the HttpClient class
        using var client = new WebClient();
        string response;
        try
        {
            response = client.DownloadString(new Uri(
                $"https://api.curseforge.com/v1/mods/search?gameId=432&searchFilter={WebUtility.UrlEncode(query)}&gameVersion={_profileVersion}&categoryId=6"));
        }
        catch (WebException e)
        {
            MessageBox.Show(e.ToString(), "Web Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Parse the JSON string into a JArray object
        var results = JArray.Parse(response);

        // Iterate through the array and add each mod to the ListView
        foreach (var result in results)
        {
            var latestFile = result["latestFiles"][0];
            var downloadCount = latestFile["downloadCount"];
            var downloadUrl = latestFile["downloadUrl"];
            var modFileName = latestFile["fileName"];
            // Create a new ModItem object with the information from the API
            var mod = new CurseForgeMod
            {
                ModName = result["name"].ToString(),
                ImageUrl = result["logo"]["thumbnailUrl"].ToString(),
                ModUrl = result["links"]["websiteUrl"].ToString(),
                Downloads = downloadCount.ToString(),
                DownloadUrl = downloadUrl.ToString(),
                ModFileName = modFileName.ToString()
            };

            // Add the mod to the ListView
            ModListView.Items.Add(mod);
        }
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        // Clear the ListView and search for mods with the query entered by the user
        ModListView.Items.Clear();
        SearchMods(SearchTextBox.Text);
    }


    private void ModListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedMod = (CurseForgeMod)ModListView.SelectedItem;
        InstallModButton.IsEnabled = _selectedMod is { Installed: false };
        UninstallModButton.IsEnabled = _selectedMod is { Installed: true };
        VisitModPageButton.IsEnabled = _selectedMod is not null;
    }

    private void InstallModButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedMod.Install(_profileDirectory);
    }

    private void UninstallModButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedMod.Uninstall(_profileDirectory);
    }

    private void VisitModPageButton_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(_selectedMod.ModUrl);
    }

    private void SearchSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        new SearchSettingsWindow(_apiKeyFile).Show();
    }
}
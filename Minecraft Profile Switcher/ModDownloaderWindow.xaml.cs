using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Minecraft_Profile_Switcher.Utilities;

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
    public int NeedsUpdate { get; set; }

    public void Install(string profileDirectory)
    {
        var wc = new WebClient();
        wc.DownloadFile(DownloadUrl, Path.Combine(profileDirectory, ModFileName));
    }

    public void Uninstall(string profileDirectory, string modFileName = "")
    {
        try
        {
            File.Delete(NeedsUpdate == 0
                ? Path.Combine(profileDirectory, ModFileName)
                : Path.Combine(profileDirectory, modFileName));
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
    private readonly string _tmpDir;
    private readonly Dictionary<string, (dynamic, string)> _modList;

    public ModDownloaderWindow(string profileDirectory, string profileVersion, Dictionary<string, (dynamic, string)> modList, string tmpDir)
    {
        InitializeComponent();
        _profileDirectory = profileDirectory;
        _profileVersion = profileVersion;
        _modList = modList;
        _tmpDir = tmpDir;
    }

    private int IsUpdated(bool installed, JToken result, string downloadUrl, string modFileName)
    {
        if (!installed) return 2;
        var client = new WebClient();
        var modFile = Path.Combine(_tmpDir, modFileName);
        var modTmpDir = Path.Combine(_tmpDir, modFileName);
        if (Directory.Exists(modTmpDir)) Directory.Delete(modTmpDir, true);
        Directory.CreateDirectory(modTmpDir);
        client.DownloadFile(downloadUrl, modFile);
        var jsonString = ReadZip(modFile, modTmpDir);
        dynamic jsonObject = JsonConvert.DeserializeObject(jsonString);
        if (jsonObject == null)
        {
            return 2;
        }

        dynamic jsonArray;
        try
        {
            jsonArray = jsonObject[0];
        }
        catch (ArgumentException)
        {
            jsonArray = jsonObject;
        }

        if (jsonArray.ToString().Contains("modList")) jsonArray = jsonArray["modList"][0];
        var modVersion = jsonArray.ToString().Contains("version") ? jsonArray["version"].ToString() : "???";
        if (modVersion == "${version}")
        {
            var modVersions = ReadZipVerAlt(modFile, modTmpDir);
            modVersion = modVersions[0];
        }

        modVersion = modVersion.Replace(_profileVersion + "-", "");
        var values = _modList[result["slug"].ToString()];
        return int.Parse(modVersion == values.Item1);
    }
    
    private void SearchMods(string query)
    {
        // Make the API call using the HttpClient class
        using var client = new WebClient();
        string apiKey;
        try
        {
            apiKey = File.ReadAllText(_apiKeyFile);
        }
        catch (IOException)
        {
            MessageBox.Show("API Key has not been set.", "API Key", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        client.Headers.Set("x-api-key", apiKey);
        var url =
            $"https://api.curseforge.com/v1/mods/search?gameId=432&searchFilter={WebUtility.UrlEncode(query)}&gameVersion={_profileVersion}&categoryId=6";
        MessageBox.Show(url);
        string response;
        try
        {
            response = client.DownloadString(new Uri(url));
        }
        catch (WebException e)
        {
            MessageBox.Show(e.ToString(), "Web Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Parse the JSON string into a JArray object
        dynamic jsonObject = JsonConvert.DeserializeObject(response);
        MessageBox.Show(jsonObject.ToString());
        var results = JArray.Parse(jsonObject["data"].ToString());

        // Iterate through the array and add each mod to the ListView
        foreach (var result in results)
        {
            var latestFile = result["latestFiles"][0];
            var downloadCount = latestFile["downloadCount"].ToString();
            var downloadUrl = latestFile["downloadUrl"].ToString();
            var modFileName = latestFile["fileName"].ToString();
            var installed = _modList.ContainsKey(result["slug"].ToString());
            var needsUpdate = IsUpdated(installed, result, downloadUrl, modFileName);

            // Create a new ModItem object with the information from the API
            var mod = new CurseForgeMod
            {
                ModName = result["name"].ToString(),
                ImageUrl = result["logo"]["thumbnailUrl"].ToString(),
                ModUrl = result["links"]["websiteUrl"].ToString(),
                Downloads = downloadCount,
                DownloadUrl = downloadUrl,
                ModFileName = modFileName,
                Installed = installed,
                NeedsUpdate = needsUpdate
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
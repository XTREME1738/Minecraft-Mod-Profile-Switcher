using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Ionic.Zip;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Minecraft_Profile_Switcher.Utilities;

namespace Minecraft_Profile_Switcher;

public class ModItem : INotifyPropertyChanged
{
    private bool _enabled;
    private string _modGameVersion;
    private string _modName;
    private string _modPath;
    private string _modVersion;

    public string ModPath
    {
        get => _modPath;
        set
        {
            _modPath = value;
            OnPropertyChanged("ModPath");
        }
    }

    public string ModName
    {
        get => _modName;
        set
        {
            _modName = value;
            OnPropertyChanged("ModName");
        }
    }

    public string ModVersion
    {
        get => _modVersion;
        set
        {
            _modVersion = value;
            OnPropertyChanged("ModVersion");
        }
    }

    public string ModGameVersion
    {
        get => _modGameVersion;
        set
        {
            _modGameVersion = value;
            OnPropertyChanged("ModGameVersion");
        }
    }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            OnPropertyChanged("Enabled");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public partial class ModManagerWindow
{
    private readonly Dictionary<string, (dynamic, string)> _modList;
    private readonly string _profilePath;
    private readonly string _profileVersion;
    private readonly string _profileVersionMain;
    private readonly string _tmpPath;
    private readonly MainWindow _window;
    private ModItem _selectedItem;

    public ModManagerWindow(string profilePath, string profileName, MainWindow window)
    {
        InitializeComponent();
        _profilePath = profilePath;
        _tmpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ModProfileSwitcher", "tmp", profileName);
        _modList = new Dictionary<string, (dynamic, string)>();
        _profileVersion = File.ReadAllText(Path.Combine(profilePath, ".version"));
        var lastPeriodIndex = _profileVersion.LastIndexOf('.');
        if (lastPeriodIndex >= 0) _profileVersionMain = _profileVersion.Remove(lastPeriodIndex);
        ProfileGameVersionTextBox.Text =
            "Profile Game Version: " + _profileVersion;
        ProfileNameTextBox.Text =
            "Profile Name: " + profileName;
        _window = window;
        window.ManageModsButton.IsEnabled = false;
        LoadMods();
    }

    private void ModListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedItem = (ModItem)ModListView.SelectedItem;
        DeleteModButton.IsEnabled = ModListView.SelectedItem != null;
        EnableModButton.IsEnabled = ModListView.SelectedItem != null && !_selectedItem.Enabled;
        DisableModButton.IsEnabled = ModListView.SelectedItem != null && _selectedItem.Enabled;
    }

    private static string ReadZip(string modFile, string modTmpDir)
    {
        var jsonString = "";
        var zip = ZipFile.Read(modFile);
        foreach (var item in zip.Entries)
        {
            if (!item.FileName.Contains("mcmod.info")) continue;
            item.Extract(modTmpDir);
            jsonString = File.ReadAllText(Path.Combine(modTmpDir, item.FileName));
        }

        zip.Dispose();
        return jsonString;
    }

    private List<string> ReadZipVerAlt(string modFile, string modTmpDir)
    {
        var zip = ZipFile.Read(modFile);
        var version = "";
        var gameVersion = "";
        var returnInfo = new List<string>();
        foreach (var item in zip.Entries)
        {
            if (!item.FileName.Contains("fml_cache_annotation.json")) continue;
            item.Extract(modTmpDir);
            var jsonString = File.ReadAllText(Path.Combine(modTmpDir, item.FileName));
            dynamic jsonObject = JsonConvert.DeserializeObject(jsonString);
            if (jsonObject == null)
            {
                version = "???";
                gameVersion = "???";
                returnInfo.Add(version);
                returnInfo.Add(gameVersion);
                zip.Dispose();
                return returnInfo;
            }

            var obj = JObject.Parse(jsonString);
            foreach (var property in obj)
            {
                MessageBox.Show(_profileVersion);
                version = property.Value != null
                    ? jsonObject[property.Key]["annotations"][0]["values"]["version"]["value"].ToString()
                    : "???";
                gameVersion = property.Value != null
                    ? jsonObject[property.Key]["annotations"][0]["values"]["acceptedMinecraftVersions"]["value"]
                        .ToString()
                    : "???";
                if (version == null) continue;
                returnInfo.Add(version);
                returnInfo.Add(gameVersion);
                break;
            }
        }

        zip.Dispose();
        return returnInfo;
    }

    private void LoadMods()
    {
        ReloadModsButton.IsEnabled = false;
        ModListView.Items.Clear();
        _modList.Clear();
        MessageBox.Show("Please wait..\nMods list is loading. This may take a few minutes.", "Loading Mods",
            MessageBoxButton.OK, MessageBoxImage.Information);
        var modFiles = Directory.GetFiles(_profilePath);
        foreach (var modFile in modFiles)
        {
            if (!modFile.EndsWith(".jar") && !modFile.EndsWith(".jar.disabled") && !modFile.EndsWith(".zip") &&
                !modFile.EndsWith(".zip.disabled")) continue;
            var disabled = modFile.EndsWith(".disabled");
            var modFileName = Path.GetFileName(modFile);
            var modTmpDir = Path.Combine(_tmpPath, modFileName);
            if (Directory.Exists(modTmpDir)) Directory.Delete(modTmpDir, true);
            Directory.CreateDirectory(modTmpDir);
            var jsonString = ReadZip(modFile, modTmpDir);
            dynamic jsonObject = JsonConvert.DeserializeObject(jsonString);
            if (jsonObject == null)
            {
                MessageBox.Show("Mod: " + modFileName +
                                ".\nDoes not contain a valid \"mcmod.info\".\nThe mod has not been added to the list, it may be incompatible.",
                    "Incompatible Mod", MessageBoxButton.OK, MessageBoxImage.Error);
                continue;
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
            var modId = jsonArray["modid"].ToString();
            var modVersion = jsonArray.ToString().Contains("version") ? jsonArray["version"].ToString() : "???";
            var modGameVersion = jsonArray.ToString().Contains("mcversion")
                ? jsonArray["mcversion"].ToString()
                : "???";
            if (modVersion == "${version}")
            {
                var modVersions = ReadZipVerAlt(modFile, modTmpDir);
                modVersion = modVersions[0];
                modGameVersion = modVersions[1].Replace("[", "").Replace("]", "");
            }
            modVersion = modVersion.Replace(_profileVersion + "-", "");
            _modList.Add(modId, (modVersion, modFile));
            var modItem = new ModItem
            {
                ModName = jsonArray["name"].ToString(),
                ModVersion = modVersion,
                ModGameVersion = modGameVersion,
                ModPath = modFile,
                Enabled = !disabled
            };
            ModListView.Items.Add(modItem);
            DeleteDir(modTmpDir, true);
        }

        ReloadModsButton.IsEnabled = true;
    }

    private void AddMod(string modFilePath)
    {
        var modFileName = Path.GetFileName(modFilePath);
        var modTmpDir = Path.Combine(_tmpPath, modFileName);
        if (Directory.Exists(modTmpDir)) Directory.Delete(modTmpDir, true);
        Directory.CreateDirectory(modTmpDir);
        var jsonString = ReadZip(modFilePath, modTmpDir);
        dynamic jsonObject = JsonConvert.DeserializeObject(jsonString);
        if (jsonObject == null)
        {
            MessageBox.Show("The mod does not contain a valid \"mcmod.info\" file.", "Invalid Mod Archive",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var jsonArray = jsonObject[0];
        if (jsonArray.ToString().Contains("modList")) jsonArray = jsonArray["modList"][0];
        var modVersion = jsonArray.ToString().Contains("version") ? jsonArray["version"].ToString() : "???";
        if (modVersion == "${version}") modVersion = ReadZipVerAlt(modFilePath, modTmpDir);
        var modGameVersion = jsonArray.ToString().Contains("mcversion")
            ? jsonArray["mcversion"].ToString()
            : "???";
        var modId = jsonArray["modid"].ToString();
        if (_modList.ContainsKey(modId))
        {
            var values = _modList[modId];
            var result = MessageBox.Show(
                "A mod with that id already exists would you like to overwrite it?\nCurrent Mod Version: " +
                values.Item1 + ".\nNew Mod Version: " + modVersion, "Continue?",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;
            File.Delete(values.Item2);
        }

        if (!modGameVersion.Contains(_profileVersionMain))
        {
            var result = MessageBox.Show(
                "Mod: " + modFileName +
                " does not have the same game version it may be incompatible.\nMod Game Version: " +
                modGameVersion + ".\nProfile Version: " + _profileVersion +
                "Would you like to continue adding the mod?", "Incompatible Mod",
                MessageBoxButton.YesNo, MessageBoxImage.Hand);
            if (result == MessageBoxResult.No) return;
        }

        try
        {
            var modFileNewPath = Path.Combine(_profilePath, modFileName);
            if (File.Exists(modFileNewPath)) File.Delete(modFileNewPath);
            File.Move(modFilePath, modFileNewPath);
            MessageBox.Show("Mod has been added.\nReloading mods list this may take a few minutes.", "Please wait.",
                MessageBoxButton.OK, MessageBoxImage.Information);
            DeleteDir(modTmpDir, true);
            LoadMods();
        }
        catch (IOException)
        {
            MessageBox.Show("The file is open in another program.", "No Permission", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddModButton_Click(object sender, RoutedEventArgs e)
    {
        // Show file open dialog to select mod file
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Mod Files|*.jar;*.zip"
        };
        if (openFileDialog.ShowDialog() != true) return;
        var modFilePath = openFileDialog.FileName;
        AddMod(modFilePath);
    }

    private void DeleteModButton_Click(object sender, RoutedEventArgs e)
    {
        var modPath = _selectedItem.ModPath;
        if (File.Exists(modPath))
        {
            File.Delete(modPath);
            ModListView.Items.Remove(_selectedItem);
            return;
        }

        MessageBox.Show("The mod file no longer exists and mods will be reloaded.", "Invalid Mod", MessageBoxButton.OK,
            MessageBoxImage.Error);
        LoadMods();
    }


    private void ReloadModsButton_Click(object sender, RoutedEventArgs e)
    {
        LoadMods();
    }

    private void OpenProfileDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(_profilePath);
    }

    private void EnableModButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            File.Move(_selectedItem.ModPath, _selectedItem.ModPath.Replace(".disabled", ""));
        }
        catch (IOException)
        {
            MessageBox.Show("The file is open in another program.", "No Permission", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        _selectedItem.Enabled = true;
        _selectedItem.ModPath = _selectedItem.ModPath.Replace(".disabled", "");
        EnableModButton.IsEnabled = false;
        DisableModButton.IsEnabled = true;
    }

    private void DisableModButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            File.Move(_selectedItem.ModPath, _selectedItem.ModPath + ".disabled");
        }
        catch (IOException)
        {
            MessageBox.Show("The file is open in another program.", "No Permission", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        _selectedItem.ModPath += ".disabled";
        _selectedItem.Enabled = false;
        DisableModButton.IsEnabled = false;
        EnableModButton.IsEnabled = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ModManagerWindow_Closing(object sender, CancelEventArgs e)
    {
        _window.ManageModsButton.IsEnabled = true;
    }

    private void CurseDownloader_Click(object sender, RoutedEventArgs e)
    {
        new ModDownloaderWindow(_profilePath).Show();
    }
}
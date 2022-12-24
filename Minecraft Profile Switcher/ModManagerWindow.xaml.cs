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
using static Minecraft_Profile_Switcher.Utilities;

namespace Minecraft_Profile_Switcher;

public class ModItem : INotifyPropertyChanged
{
    private string _modName;
    private string _modVersion;
    private string _modGameVersion;
    private string _modPath;
    private bool _enabled;

    public event PropertyChangedEventHandler PropertyChanged;

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
    private readonly string _tmpPath;

    public ModManagerWindow(string profilePath, string profileName)
    {
        InitializeComponent();
        _profilePath = profilePath;
        _tmpPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ModProfileSwitcher", "tmp", profileName);
        _modList = new Dictionary<string, (dynamic, string)>();
        LoadMods();
        _profileVersion = File.ReadAllText(Path.Combine(profilePath, ".version"));
        ProfileGameVersionTextBox.Text =
            "Profile Game Version: " + _profileVersion;
        ProfileNameTextBox.Text =
            "Profile Name: " + profileName;
    }

    private void ModListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DeleteModButton.IsEnabled = ModListView.SelectedItem != null;
        EnableModButton.IsEnabled = ModListView.SelectedItem != null && !((ModItem)ModListView.SelectedItem).Enabled;
        DisableModButton.IsEnabled = ModListView.SelectedItem != null && ((ModItem)ModListView.SelectedItem).Enabled;
    }

    private static string ReadZip(string modFile, string modTmpDir)
    {
        var jsonString = "";
        var zip = ZipFile.Read(modFile);
        foreach (var item in zip.Entries)
        {
            if (item.FileName != "mcmod.info") continue;
            item.Extract(modTmpDir);
            jsonString = File.ReadAllText(Path.Combine(modTmpDir, item.FileName));
        }
        zip.Dispose();
        return jsonString;
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
            if (!modFile.EndsWith(".jar") && !modFile.EndsWith(".jar.disabled")) continue;
            var disabled = modFile.EndsWith(".disabled");
            var modFileName = Path.GetFileName(modFile);
            var modTmpDir = Path.Combine(_tmpPath, modFileName);
            if (Directory.Exists(modTmpDir)) Directory.Delete(modTmpDir, true);
            Directory.CreateDirectory(modTmpDir);
            var jsonString = ReadZip(modFile, modTmpDir);
            dynamic jsonObject = JsonConvert.DeserializeObject(jsonString);
            if (jsonObject == null) continue;
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
        var modGameVersion = jsonArray.ToString().Contains("mcversion")
            ? jsonArray["mcversion"].ToString()
            : "???";
        var modId = jsonArray["modid"].ToString();
        if (_modList.ContainsKey(modId))
        {
            var values = _modList[modId];
            MessageBox.Show(values.Item1, "x");
            MessageBox.Show(values.Item2, "y");
            MessageBox.Show(modFilePath, "z");
            var result = MessageBox.Show("A mod with that id already exists would you like to overwrite it?\nCurrent Mod Version: " + values.Item1 + ".\nNew Mod Version: " + modVersion, "Continue?",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;
            File.Delete(values.Item2);
        }
        if (File.ReadAllText(Path.Combine(_profilePath, ".version")) != modGameVersion)
        {
            var result = MessageBox.Show(
                "Mod: " + modFileName + " does not have the same game version it may be incompatible.\nMod Game Version: " +
                modGameVersion + ".\nProfile Version: " + _profileVersion +  "Would you like to continue adding the mod?", "Incompatible Mod",
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
            MessageBox.Show("The file is open in another program.", "No Permission", MessageBoxButton.OK, MessageBoxImage.Error);
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
        var modItem = (ModItem)ModListView.SelectedItem;
        var modPath = modItem.ModPath;
        if (File.Exists(modPath))
        {
            File.Delete(modPath);
            ModListView.Items.Remove(modItem);
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
        var mod = (ModItem)ModListView.SelectedItem;
        try
        {
            File.Move(mod.ModPath, mod.ModPath.Replace(".disabled", ""));
        }
        catch (IOException)
        {
            MessageBox.Show("The file is open in another program.", "No Permission", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        mod.Enabled = true;
        mod.ModPath = mod.ModPath.Replace(".disabled", "");
        EnableModButton.IsEnabled = false;
        DisableModButton.IsEnabled = true;
    }
    
    private void DisableModButton_Click(object sender, RoutedEventArgs e)
    {
        var mod = (ModItem)ModListView.SelectedItem;
        try
        {
            File.Move(mod.ModPath, mod.ModPath + ".disabled");
        }
        catch (IOException)
        {
            MessageBox.Show("The file is open in another program.", "No Permission", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
        mod.ModPath += ".disabled";
        mod.Enabled = false;
        DisableModButton.IsEnabled = false;
        EnableModButton.IsEnabled = true;
    }
}
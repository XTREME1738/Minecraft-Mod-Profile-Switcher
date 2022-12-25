using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Minecraft_Profile_Switcher;

public class CurseForgeMod
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Downloads { get; set; }
    public string Version { get; set; }
    public string ImageUrl { get; set; }
    public string ModUrl { private get; set; }
    
    public string ModFileName { private get; set; }

    public void Install(string profileDirectory)
    {
        var wc = new WebClient();
        wc.DownloadFile(ModUrl, Path.Combine(profileDirectory, ModFileName));
    }
}

public partial class ModDownloaderWindow
{

    private CurseForgeMod _selectedMod;
    private readonly string _profileDirectory;

    public ModDownloaderWindow(string profileDirectory)
    {
        InitializeComponent();
        _profileDirectory = profileDirectory;
        ModListView.Items.Add(new CurseForgeMod
        {
            Name = "Just Nerds Modpack-1.0.0",
            Description = "A pack for a group of friends",
            Downloads = "124",
            Version = "1.0.0",
            ImageUrl = "https://media.forgecdn.net/avatars/thumbnails/232/730/256/256/637067950570345191.png",
            ModUrl = "https://edge.forgecdn.net/files/2810/282/Just Nerds Modpack-1.0.0.zip",
            ModFileName = "Just Nerds Modpack-1.0.0.zip"
        });
    }

    private void ModListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedMod = (CurseForgeMod)ModListView.SelectedItem;
    }

    private void InstallButton_Click(object sender, RoutedEventArgs e)
    {
        _selectedMod.Install(_profileDirectory);
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Linq;
using static Minecraft_Profile_Switcher.Utilities;

namespace Minecraft_Profile_Switcher
{
    public partial class MainWindow
    {
        private readonly string _profilesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ModProfileSwitcher", "Profiles");
        private static readonly string MinecraftDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
        private static readonly string MinecraftModsDirectory = Path.Combine(MinecraftDirectory, "mods");
        private static string _activeProfileLinkPath;
        private static string _activeProfilePath;
        private List<string> _profileNames;
        private const string Version = "1.0.0";

        public MainWindow()
        {
            InitializeComponent();
            var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            if (!isAdmin)
            {
                MessageBox.Show("This program must be run as an administrator.");
                Environment.Exit(1);
            }
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ModProfileSwitcher", "tmp")))
                Directory.CreateDirectory(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ModProfileSwitcher", "tmp"));
            UpdateCheck();
            LoadProfiles();
            CheckExists();
            ButtonsEnabled(false);
        }

        private void CheckExists()
        {
            if (Directory.Exists(MinecraftModsDirectory) && !IsSymlink(MinecraftModsDirectory) && Directory.GetFiles(MinecraftModsDirectory).Length != 0) HandleExisting();
        }

        private void ManageModsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = (string)ProfileComboBox.SelectedItem;
            var selectedProfilePath = Path.Combine(_profilesDirectory, selectedProfile);
            var modManagerWindow = new ModManagerWindow(selectedProfilePath, selectedProfile, this);
            modManagerWindow.Show();
        }
        
        private void HandleExisting()
        {
            var result =
                MessageBox.Show(
                    "Existing mods were found in the minecraft directory.\nWould you like to move them to a new profile?\nClicking \"No\" will delete them.", "Existing Mods", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                DeleteDir(MinecraftModsDirectory, true);
                return;
            }
            var profileName = "";
            var newProfileVersion = "";
            var inputWindow = new EditWindow("Enter a name for the new profile:", "Add Profile");
            inputWindow.ShowDialog();
            if (inputWindow.DialogResult == true)
            {
                profileName = inputWindow.ResponseText;
                newProfileVersion = inputWindow.MinecraftVersion;
            }

            if (profileName == "")
            {
                return;
            }

            var newProfilePath = Path.Combine(_profilesDirectory, profileName);
            if (Directory.Exists(newProfilePath))
            {
                MessageBox.Show("A profile with that name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Directory.Move(MinecraftModsDirectory, newProfilePath);
            }
            catch (IOException)
            {
                MessageBox.Show("The directory is open in another program.", "No Permission", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            File.WriteAllText(newProfilePath + "\\.version", newProfileVersion);
            LoadProfiles();
            CheckActive();
        }

        private static bool UpdateCheck()
        {
            var wc = new WebClient();
            wc.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:100.0) Gecko/20100101 Firefox/100.0");
            var response = wc.DownloadString("https://api.github.com/repos/00-XPRT-00/Minecraft-Mod-Profile-Switcher/releases");
            var latestRelease = JArray.Parse(response)[0];
            if (latestRelease["tag_name"]!.ToString() == $"v{Version}") return false;
            var result = MessageBox.Show("A new version is available.\nWould you like to update?", "Update Available",
                MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.No) return true;
            Process.Start("https://github.com/00-XPRT-00/Minecraft-Mod-Profile-Switcher/releases/latest");
            Environment.Exit(0);
            return true;
        }
        
        private void ButtonsEnabled(bool enabled)
        {
            DeleteButton.IsEnabled = enabled;
            EditButton.IsEnabled = enabled;
            ActivateButton.IsEnabled = enabled;
            ManageModsButton.IsEnabled = enabled;
            OpenProfileDirectoryButton.IsEnabled = enabled;
        }
        
        private void CheckActive()
        {
            var activeProfilePath = GetSymlinkTarget(MinecraftModsDirectory);
            if (activeProfilePath == null) return;
            var activeProfileName = Path.GetFileName(activeProfilePath);
            foreach (string profileName in ProfileComboBox.Items)
            {
                if (profileName != activeProfileName) continue;
                ProfileComboBox.SelectedItem = profileName;
                break;
            }

            ProfileComboBox.SelectedItem = activeProfileName;
            _activeProfileLinkPath = activeProfilePath;
            _activeProfilePath = Path.GetFullPath(_activeProfileLinkPath);
            ActiveProfileLabel.Content = "Active Profile: " + activeProfileName;
            ActivateButtonLabel.Text = "Deactivate";
            ButtonsEnabled(true);
        }
        
        private static bool IsSymlink(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
        }

        private static void RemoveSymlink(string path)
        {
            if (IsSymlink(path))
            {
                DeleteDir(path);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        private enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }
        
        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr securityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetFinalPathNameByHandle([In] SafeFileHandle hFile, [Out] StringBuilder lpszFilePath, [In] int cchFilePath, [In] int dwFlags);

        private const int CreationDispositionOpenExisting = 3;
        private const int FileFlagBackupSemantics = 0x02000000;


        private static string GetSymlinkTarget(string path)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                return null;
            }

            var directoryHandle = CreateFile(path, 0, 2, IntPtr.Zero, CreationDispositionOpenExisting, FileFlagBackupSemantics, IntPtr.Zero); //Handle file / folder

            if (directoryHandle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var result = new StringBuilder(512);
            var mResult = GetFinalPathNameByHandle(directoryHandle, result, result.Capacity, 0);

            if (mResult < 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (result.Length >= 4 && result[0] == '\\' && result[1] == '\\' && result[2] == '?' && result[3] == '\\')
            {
                return result.ToString().Substring(4); // "\\?\" remove
            }
            return result.ToString();
        }

        private void LoadProfiles()
        {
            if (!Directory.Exists(_profilesDirectory))
            {
                Directory.CreateDirectory(_profilesDirectory);
            }

            _profileNames = Directory.GetDirectories(_profilesDirectory).Select(Path.GetFileName).ToList();
            ProfileComboBox.ItemsSource = _profileNames;
        }

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedProfile = (string)ProfileComboBox.SelectedItem;
            if (selectedProfile == null)
            {
                ButtonsEnabled(false);
                return;
            }
            ButtonsEnabled(true);
            var selectedProfilePath = Path.Combine(_profilesDirectory, selectedProfile);
            ActivateButtonLabel.Text = _activeProfilePath == selectedProfilePath ? "Deactivate" : "Activate";
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = (string)ProfileComboBox.SelectedItem;
            if (selectedProfile == null)
            {
                return;
            }

            var selectedProfilePath = Path.Combine(_profilesDirectory, selectedProfile);
            if (_activeProfilePath == selectedProfilePath)
            {
                RemoveSymlink(MinecraftModsDirectory);
                ActivateButtonLabel.Text = "Activate";
                _activeProfilePath = null;
                ActiveProfileLabel.Content = "Active Profile: None";
            }
            else
            {
                if (Directory.Exists(MinecraftModsDirectory))
                {
                    if (IsSymlink(MinecraftModsDirectory)) DeleteDir(MinecraftModsDirectory);
                    else DeleteDir(MinecraftModsDirectory, true);
                }
                CreateSymbolicLink(MinecraftModsDirectory, selectedProfilePath, SymbolicLink.Directory);
                ActivateButtonLabel.Text = "Deactivate";
                _activeProfilePath = selectedProfilePath;
                ActiveProfileLabel.Content = "Active Profile: " + Path.GetFileName(_activeProfilePath);
            }
        }
        
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = (string)ProfileComboBox.SelectedItem;
            if (selectedProfile == null)
            {
                return;
            }
            var selectedProfilePath = Path.Combine(_profilesDirectory, selectedProfile);
            var profileVersion = File.Exists(Path.Combine(selectedProfilePath, ".version"))
                ? File.ReadAllText(Path.Combine(selectedProfilePath, ".version")) : "";
            var newProfileName = "";
            var newProfileVersion = "";
            var inputWindow = new EditWindow("Edit the profile below (Name/Version):", "Edit Profile", selectedProfile, profileVersion);
            inputWindow.ShowDialog();
            if (inputWindow.DialogResult == true)
            {
                newProfileName = inputWindow.ResponseText;
                newProfileVersion = inputWindow.MinecraftVersion;
            }
            if (newProfileName == "")
            {
                return;
            }
            var newProfilePath = Path.Combine(_profilesDirectory, newProfileName);
            if (selectedProfile != newProfileName)
            {
                if (Directory.Exists(newProfilePath))
                {
                    MessageBox.Show("A profile with that name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Directory.Move(selectedProfilePath, newProfilePath);
            }
            if (newProfileVersion != profileVersion) File.WriteAllText(selectedProfilePath + "\\.version", newProfileVersion);
            LoadProfiles();
            ProfileComboBox.SelectedItem = newProfileName;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = (string)ProfileComboBox.SelectedItem;
            if (selectedProfile == null)
            {
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this profile?", "Delete Profile", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                return;
            }

            var selectedProfilePath = Path.Combine(_profilesDirectory, selectedProfile);
            if (_activeProfilePath == selectedProfilePath)
            {
                RemoveSymlink(MinecraftModsDirectory);
                _activeProfilePath = null;
            }

            DeleteDir(selectedProfilePath, true);
            LoadProfiles();
        }
        
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "ZIP Archives|*.zip",
                Title = "Select a ZIP archive of mods to add as a profile",
                CheckFileExists = true,
                Multiselect = false
            };
            if (openFileDialog.ShowDialog() != true) return;
            var filePath = openFileDialog.FileName;
            var fileName = Path.GetFileName(filePath);
            var profileName = "";
            var newProfileVersion = "";
            var inputWindow = new EditWindow("Enter a name for the new profile:", "Add Profile", fileName);
            inputWindow.ShowDialog();
            if (inputWindow.DialogResult == true)
            {
                profileName = inputWindow.ResponseText;
                newProfileVersion = inputWindow.MinecraftVersion;
            }

            if (profileName == "")
            {
                return;
            }

            var newProfilePath = Path.Combine(_profilesDirectory, profileName);
            if (Directory.Exists(newProfilePath))
            {
                MessageBox.Show("A profile with that name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Directory.CreateDirectory(newProfilePath);
            if (Path.GetExtension(filePath) == ".zip")
            {
                ZipFile.ExtractToDirectory(filePath, newProfilePath);
                File.Delete(filePath);
            }
            File.WriteAllText(newProfilePath + "\\.version", newProfileVersion);
            LoadProfiles();
            ProfileComboBox.SelectedItem = profileName;
        }

        private void OpenProfileDirectory_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = (string)ProfileComboBox.SelectedItem;
            if (selectedProfile == null)
            {
                return;
            }
            var selectedProfilePath = Path.Combine(_profilesDirectory, selectedProfile);
            Process.Start(selectedProfilePath);
        }

        private void CheckForUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateCheck() == false)
            {
                MessageBox.Show("No updates available", "Up to date!", MessageBoxButton.OK, MessageBoxImage.Information);
            } 
        }
    }
}
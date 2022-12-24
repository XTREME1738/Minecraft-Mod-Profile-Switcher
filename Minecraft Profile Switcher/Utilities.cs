using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Minecraft_Profile_Switcher;

internal static class Utilities
{
    private static bool DeleteDirResult(string path, bool recursive)
    {
        try
        {
            Directory.Delete(path, recursive);
            return true;
        }
        catch (IOException e)
        {
            return false;
            //MessageBox.Show("The directory is open in another program.\n" + e, "No Permission", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    internal static async void DeleteDir(string path, bool recursive = false)
    {
        var result = DeleteDirResult(path, recursive);
        var count = 0;
        while (!result)
        {
            await Task.Delay(1);
            result = DeleteDirResult(path, recursive);
            count++;
            if (count is 5 or 10 or 20 or 25)
            {
                if (count == 25)
                {
                    MessageBox.Show("Failed removing directory 25 times.\nStopping trying to remove the directory.\n\nPath: " + path, "Cancelling", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                }
                var res = MessageBox.Show("Failed removing directory " + count + " times.\nWould you like to cancel?\n\nPath: " + path, "Cancel?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    break;
                }
            }
            if (result) break;
        }
    }
}
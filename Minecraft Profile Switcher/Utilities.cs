using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    public static string ReadZip(string modFile, string modTmpDir)
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

    public static List<string> ReadZipVerAlt(string modFile, string modTmpDir)
    {
        var zip = ZipFile.Read(modFile);
        var returnInfo = new List<string>();
        foreach (var item in zip.Entries)
        {
            if (!item.FileName.Contains("fml_cache_annotation.json")) continue;
            item.Extract(modTmpDir);
            var jsonString = File.ReadAllText(Path.Combine(modTmpDir, item.FileName));
            dynamic jsonObject = JsonConvert.DeserializeObject(jsonString);
            string version;
            string gameVersion;
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
}
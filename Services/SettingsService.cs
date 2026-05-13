using System;
using System.IO;
using System.Text.Json;

namespace datHazy_Moonlauncher.Services
{
    public class AppSettings
    {
        public string MoonlightPath { get; set; } =
            @"C:\Program Files\Moonlight Game Streaming\Moonlight.exe";
    }

    public static class SettingsService
    {
        private static readonly string Folder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "datHazy Moonlauncher");

        private static readonly string FilePath =
            Path.Combine(Folder, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new AppSettings();

                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            Directory.CreateDirectory(Folder);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
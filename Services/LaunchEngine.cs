using System.Diagnostics;
using System.Windows;

namespace datHazy_Moonlauncher.Services
{
    public class LaunchEngine
    {
        public string MoonlightPath { get; set; } =
            @"C:\Program Files\Moonlight Game Streaming\Moonlight.exe";

        public string AppName { get; set; } = "UMVC3";

        public void KillMoonlight()
        {
            foreach (var p in Process.GetProcessesByName("Moonlight"))
            {
                try { p.Kill(true); } catch { }
            }
        }

        public void Pair(string host)
        {
            Start($"pair {host}");
        }

        public void Stream(string host, string extraArgs = "")
        {
            // Build CLI EXACTLY how you expect it
            var args = string.IsNullOrWhiteSpace(extraArgs)
                ? $"stream {host} \"{AppName}\""
                : $"stream {extraArgs} {host} \"{AppName}\"";

            Start(args);
        }

        private void Start(string args)
        {
            var fullCommand = $"{MoonlightPath} {args}";

            // =========================
            // DEBUG WINDOW
            // =========================
            // MessageBox.Show(fullCommand, "Moonlight CLI Debug");

            // =========================
            // COPY TO CLIPBOARD
            // =========================
            // try
            // {
            //     Clipboard.SetText(fullCommand);
            // }
            // catch { }

            // =========================
            // ACTUAL EXECUTION
            // =========================
            Process.Start(new ProcessStartInfo
            {
                FileName = MoonlightPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }
}
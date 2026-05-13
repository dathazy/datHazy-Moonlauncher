using System.Diagnostics;

namespace datHazy_Moonlauncher.Services
{
    public class MoonlightService
    {
        public string MoonlightPath { get; set; } =
            @"C:\Program Files\Moonlight Game Streaming\Moonlight.exe";

        public void Kill()
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

        public void Stream(string host, string args = "")
        {
            Start(string.IsNullOrWhiteSpace(args)
                ? $"stream {host}"
                : $"stream {args} {host}");
        }

        private void Start(string args)
        {
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
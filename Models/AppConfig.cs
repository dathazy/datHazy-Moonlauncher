using System.Collections.Generic;

namespace datHazy_Moonlauncher.Models
{
    public class AppConfig
    {
        public Dictionary<string, string> PairServers { get; set; } = new();

        public int PairDelayMs { get; set; }

        public int PostLaunchLockMs { get; set; }

        public string RecommendedArgs { get; set; } = "";

        public Dictionary<string, List<Station>> Stations { get; set; } = new();

        public string Version { get; set; } = "1.0.0";

        public string Status { get; set; } = "Offline";

        public string App { get; set; } = "UMVC3";

        // =========================
        // SAFE ACCESS HELPERS
        // =========================
        public List<Station> GetStations(string mode)
        {
            return Stations.TryGetValue(mode, out var list)
                ? list
                : new List<Station>();
        }

        public string GetPairServer(string mode)
        {
            return PairServers.TryGetValue(mode, out var host)
                ? host
                : "";
        }
    }
}
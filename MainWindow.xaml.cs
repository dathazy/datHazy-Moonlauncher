using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;
using datHazy_Moonlauncher.Models;
using datHazy_Moonlauncher.Services;

namespace datHazy_Moonlauncher
{
    public partial class MainWindow : Window
    {
        private AppConfig _config = new();
        private AppSettings _settings = new();
        private string _currentMode = "";

        private readonly LaunchEngine _engine = new();

        private string? _pendingUri = null;

        public MainWindow()
        {
            InitializeComponent();

            this.Foreground = Brushes.White;

            _settings = SettingsService.Load();

            _engine.MoonlightPath = _settings.MoonlightPath;

            FloatingPathButton.Visibility = Visibility.Collapsed;

            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                _pendingUri = args[1];
            }

            _ = LoadConfig();
        }

        // =========================
        // CONFIG
        // =========================

        private async Task LoadConfig()
        {
            try
            {
                _config = await ConfigService.LoadAsync("https://dathazy.com/config.json");

                StatusText.Text = "Config loaded from server";
            }
            catch (Exception ex)
            {
                _config = GetFallbackConfig();

                StatusText.Text = $"Offline config ({ex.Message})";
            }

            BuildModeSelection();

            if (!string.IsNullOrWhiteSpace(_pendingUri))
            {
                HandleUri(_pendingUri);
            }
        }

        // =========================
        // URI HANDLING
        // =========================

        private async void HandleUri(string uriString)
        {
            try
            {
                var uri = new Uri(uriString);

                if (uri.Scheme != "moonlaunch")
                    return;

                var query = ParseQuery(uri.Query);

                string mode =
                    query.ContainsKey("mode")
                    ? query["mode"]
                    : "lobby";

                string stationName =
                    query.ContainsKey("station")
                    ? query["station"]
                    : "";

                bool recommended =
                    query.ContainsKey("recommended")
                    && query["recommended"].ToLower() == "true";

                if (!_config.Stations.ContainsKey(mode))
                    return;

                var station = _config.Stations[mode]
                    .Find(s =>
                        s.Name.Replace(" ", "")
                        .ToLower()
                        .Contains(
                            stationName.Replace(" ", "")
                            .ToLower()));

                if (station == null)
                    return;

                BuildMode(mode);

                await RunFlow(
                    station.Host,
                    station.Host,
                    recommended);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "URI ERROR");
            }
        }

        // =========================
        // QUERY PARSER
        // =========================

        private Dictionary<string, string> ParseQuery(string query)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(query))
                return result;

            query = query.TrimStart('?');

            var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                var parts = pair.Split('=', 2);

                var key = Uri.UnescapeDataString(parts[0]);

                var value =
                    parts.Length > 1
                    ? Uri.UnescapeDataString(parts[1])
                    : "";

                result[key] = value;
            }

            return result;
        }

        // =========================
        // LOADING UI
        // =========================

        private void ShowLoading(string text)
        {
            LoadingTitle.Text = text;

            LoadingOverlay.Visibility = Visibility.Visible;
        }

        private void HideLoading()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        // =========================
        // HOME UI
        // =========================

        private void BuildModeSelection()
        {
            Root.Children.Clear();

            FloatingPathButton.Visibility = Visibility.Visible;

            FloatingPathButton.Click -= FloatingPathButton_Click;
            FloatingPathButton.Click += FloatingPathButton_Click;

            var title = new TextBlock
            {
                Text = "datHazy Moonlauncher",
                FontSize = 26,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var meta = new TextBlock
            {
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            meta.Inlines.Add(
                new Run($"Version {_config.Version} | Status: "));

            meta.Inlines.Add(
                new Run(_config.Status)
                {
                    Foreground =
                        _config.Status == "Online"
                        ? Brushes.LightGreen
                        : Brushes.IndianRed
                });

            var lobbyBtn = new Button
            {
                Content = "Lobby Mode",
                Width = 320,
                Height = 40,
                Margin = new Thickness(0, 5, 0, 5)
            };

            lobbyBtn.Click += (_, __) => BuildMode("lobby");

            var tournamentBtn = new Button
            {
                Content = "Tournament Mode",
                Width = 320,
                Height = 40,
                Margin = new Thickness(0, 5, 0, 5)
            };

            tournamentBtn.Click += (_, __) => BuildMode("tournament");

            Root.Children.Add(title);
            Root.Children.Add(meta);
            Root.Children.Add(lobbyBtn);
            Root.Children.Add(tournamentBtn);
        }

        // =========================
        // MODE VIEW
        // =========================

        private void BuildMode(string mode)
        {
            _currentMode = mode;

            FloatingPathButton.Visibility = Visibility.Collapsed;

            Root.Children.Clear();

            var backBtn = new Button
            {
                Content = "← Back",
                Width = 320,
                Height = 35,
                Margin = new Thickness(0, 0, 0, 10)
            };

            backBtn.Click += (_, __) => BuildModeSelection();

            var pairBtn = new Button
            {
                Content = "Initial Pair / Re-Pair",
                Width = 320,
                Height = 40,
                Margin = new Thickness(0, 0, 0, 20)
            };

            pairBtn.Click += async (_, __) =>
            {
                await RunFlow(
                    _config.PairServers[_currentMode],
                    null,
                    true);
            };

            Root.Children.Add(backBtn);
            Root.Children.Add(pairBtn);

            foreach (var station in _config.Stations[_currentMode])
            {
                Root.Children.Add(BuildStationCard(station));
            }
        }

        // =========================
        // STATION CARD
        // =========================

        private UIElement BuildStationCard(Station station)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.DimGray,
                Background = new SolidColorBrush(Color.FromRgb(25, 25, 25)),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 8, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var panel = new StackPanel
            {
                Width = 320
            };

            var name = new TextBlock
            {
                Text = station.Name,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = Brushes.White
            };

            var btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var userBtn = new Button
            {
                Content = "User Settings",
                Width = 140,
                Margin = new Thickness(0, 0, 8, 0)
            };

            var recBtn = new Button
            {
                Content = "Recommended Settings",
                Width = 170,
                Style = (Style)Application.Current.Resources["RecommendedButtonStyle"]
            };

            // =========================
            // UPDATED LOGIC
            // Pair directly to station host
            // =========================

            userBtn.Click += async (_, __) =>
            {
                await RunFlow(
                    station.Host,
                    station.Host,
                    false);
            };

            recBtn.Click += async (_, __) =>
            {
                await RunFlow(
                    station.Host,
                    station.Host,
                    true);
            };

            btnRow.Children.Add(userBtn);
            btnRow.Children.Add(recBtn);

            panel.Children.Add(name);
            panel.Children.Add(btnRow);

            border.Child = panel;

            return border;
        }

        // =========================
        // LAUNCH FLOW
        // =========================

        private async Task RunFlow(
            string pairHost,
            string? streamHost,
            bool useRecommended)
        {
            try
            {
                SetUIEnabled(false);

                ShowLoading("Verifying pairing...");

                StatusText.Text = "Verifying pairing...";

                await Task.Delay(250);

                _engine.KillMoonlight();

                await Task.Delay(500);

                _engine.Pair(pairHost);

                await Task.Delay(_config.PairDelayMs);

                if (!string.IsNullOrEmpty(streamHost))
                {
                    ShowLoading("Connecting to server...");

                    StatusText.Text = "Connecting to server...";

                    if (useRecommended)
                    {
                        _engine.Stream(
                            streamHost,
                            _config.RecommendedArgs);
                    }
                    else
                    {
                        _engine.Stream(streamHost, "");
                    }

                    await Task.Delay(
                        _config.PostLaunchLockMs);
                }
            }
            finally
            {
                HideLoading();

                SetUIEnabled(true);

                StatusText.Text = "Ready";
            }
        }

        private void SetUIEnabled(bool enabled)
        {
            foreach (UIElement child in Root.Children)
            {
                child.IsEnabled = enabled;
            }
        }

        // =========================
        // FLOATING BUTTON
        // =========================

        private void FloatingPathButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SetMoonlightPath();
        }

        // =========================
        // SETTINGS
        // =========================

        private void SetMoonlightPath()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Moonlight Executable (*.exe)|*.exe",
                Title = "Select Moonlight.exe"
            };

            if (dialog.ShowDialog() == true)
            {
                _settings.MoonlightPath = dialog.FileName;

                SettingsService.Save(_settings);

                _engine.MoonlightPath = _settings.MoonlightPath;

                StatusText.Text = "Moonlight path updated";
            }
        }

        // =========================
        // FALLBACK CONFIG
        // =========================

        private AppConfig GetFallbackConfig()
        {
            return new AppConfig
            {
                Version = "1.0.0",
                Status = "Offline",

                PairServers = new Dictionary<string, string>
                {
                    { "lobby", "stream.dathazy.com" },
                    { "tournament", "pair.dathazy.com" }
                },

                PairDelayMs = 5000,

                PostLaunchLockMs = 3000,

                RecommendedArgs =
                    "--720 --fps 60 --bitrate 15000 --video-codec hevc --no-vsync --video-decoder hardware",

                Stations = new Dictionary<string, List<Station>>
                {
                    {
                        "lobby",
                        new List<Station>
                        {
                            new Station
                            {
                                Name = "Stream (Hazy WC)",
                                Host = "stream.dathazy.com"
                            },

                            new Station
                            {
                                Name = "Station 2 (Hazy WC2)",
                                Host = "station2.dathazy.com"
                            }
                        }
                    },

                    {
                        "tournament",
                        new List<Station>
                        {
                            new Station
                            {
                                Name = "Stream Station",
                                Host = "stream.dathazy.com"
                            },

                            new Station
                            {
                                Name = "Station 2",
                                Host = "station2.dathazy.com"
                            },

                            new Station
                            {
                                Name = "Station 3",
                                Host = "station3.dathazy.com"
                            },

                            new Station
                            {
                                Name = "Station 4",
                                Host = "station4.dathazy.com"
                            }
                        }
                    }
                }
            };
        }
    }
}
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Propagation;
using SGPdotNET.TLE;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SatelliteTracker
{
    public partial class MainWindow : Window
    {
        TLESett tLESett = new();

        string destenation = "TLEList.txt";

        public ObservableCollection<Satellite> Satellites { get; set; }

        public string[,] sattTle = null;

        // =========================
        // ACTIVE SATELLITES
        // =========================

        private List<SatelliteRuntime> activeSatellites = new();

        private DispatcherTimer satelliteTimer;

        public class Satellite
        {
            public string Name { get; set; }

            public Brush TrackColor { get; set; }
        }

        // runtime object
        private class SatelliteRuntime
        {
            public string Name { get; set; }

            public Sgp4 Propagator { get; set; }

            public Color Color { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;

            Satellites = TLEFile.Read(destenation);

            Loaded += MainWindow_Loaded;

            DataContext = this;
        }

        // =========================
        // ADD TLE
        // =========================

        private void AddTLE_Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TLEnameInput_textBox.Text))
            {
                MessageBox.Show(
                    "Введите название TLE-пакета",
                    "Внимание!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(TLE1input_textBox.Text) ||
                string.IsNullOrWhiteSpace(TLE2input_textBox.Text))
            {
                MessageBox.Show(
                    "Вставьте TLE-пакет",
                    "Внимание!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                Brush trackColor = Brushes.Gray;

                System.Windows.Media.Color selectedColor =
                    Colors.Coral;

                if (MyColorPicker.SelectedColor != null)
                {
                    selectedColor =
                        (Color)MyColorPicker.SelectedColor;

                    trackColor =
                        new SolidColorBrush(selectedColor);
                }

                tLESett.TLEname =
                    TLEnameInput_textBox.Text;

                tLESett.TLEcolor =
                    $"{selectedColor.R}, {selectedColor.G}, {selectedColor.B}";

                tLESett.TLE1 =
                    TLE1input_textBox.Text;

                tLESett.TLE2 =
                    TLE2input_textBox.Text;

                tLESett.SaveToFile(destenation);

                var newSatellite = new Satellite
                {
                    Name = TLEnameInput_textBox.Text,
                    TrackColor = trackColor
                };

                Satellites.Add(newSatellite);

                TLEnameInput_textBox.Clear();
                TLE1input_textBox.Clear();
                TLE2input_textBox.Clear();

                MyColorPicker.SelectedColor = Colors.Gray;

                MessageBox.Show(
                    "Спутник успешно добавлен!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при добавлении: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================
        // DRAW ORBIT
        // =========================

    public async Task DrawOrbit(string satName, Color color, string line1, string line2)
        {
            try
            {
                var tle = new Tle(
                    satName,
                    line1,
                    line2);

                var sgp4 = new Sgp4(tle);

                // сохраняем спутник
                activeSatellites.Add(new SatelliteRuntime
                {
                    Name = satName,
                    Propagator = sgp4,
                    Color = color
                });

                List<double[]> points = new();

                DateTime start = DateTime.UtcNow;

                for (int i = 0; i < 120; i++)
                {
                    DateTime time =
                        start.AddMinutes(i);

                    EciCoordinate eci =
                        sgp4.FindPosition(time);

                    GeodeticCoordinate geo =
                        eci.ToGeodetic();

                    double lat =
                        geo.Latitude.Degrees;

                    double lon =
                        NormalizeLon(
                            geo.Longitude.Degrees);

                    points.Add(new[]
                    {
                lat,
                lon
            });
                }

                string json =
                    JsonSerializer.Serialize(points);

                string hexColor =
                    $"#{color.R:X2}{color.G:X2}{color.B:X2}";

                // рисуем орбиту
                await MapView.CoreWebView2.ExecuteScriptAsync(
                    $"drawTrack({json}, '{hexColor}');");

                // текущая позиция
                await UpdateSatellitePosition(
                    satName,
                    sgp4,
                    color);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        // =========================
        // UPDATE SAT POSITION
        // =========================

        private async Task UpdateSatellitePosition(string satName, Sgp4 sgp4, Color color)
        {
            EciCoordinate eci =
                sgp4.FindPosition(DateTime.UtcNow);

            GeodeticCoordinate geo =
                eci.ToGeodetic();

            double lat =
                geo.Latitude.Degrees;

            double lon =
                NormalizeLon(
                    geo.Longitude.Degrees);

            string hexColor =
                $"#{color.R:X2}{color.G:X2}{color.B:X2}";

            // безопасный JS object
            var satData = new
            {
                name = satName,
                lat,
                lon,
                color = hexColor
            };

            string json =
                JsonSerializer.Serialize(satData);

            await MapView.CoreWebView2.ExecuteScriptAsync(
                $"updateSatellite({json});");
        }


        // =========================
        // TIMER UPDATE
        // =========================

        private async void SatelliteTimer_Tick(object sender, EventArgs e)
        {
            foreach (var sat in activeSatellites)
            {
                await UpdateSatellitePosition(
                    sat.Name,
                    sat.Propagator,
                    sat.Color);
            }
        }

        // =========================
        // NORMALIZE LONGITUDE
        // =========================

        private double NormalizeLon(double lon)
        {
            while (lon > 180)
                lon -= 360;

            while (lon < -180)
                lon += 360;

            return lon;
        }

        // =========================
        // WINDOW LOADED
        // =========================

        private async void MainWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await InitMap();

            // ждём загрузку JS
            await Task.Delay(1000);

            WindowState =
                WindowState.Maximized;

            Satellites = TLEFile.Read(destenation);

            var tleData = TLEFile.ReadTle(destenation);

            for (int i = 0; i < tleData.Length; i++)
            {
                await DrawOrbit(
                    Satellites[i].Name,
                    tleData[i].Color,
                    tleData[i].Line1,
                    tleData[i].Line2);
            }

            // realtime timer
            satelliteTimer = new DispatcherTimer();

            satelliteTimer.Interval =
                TimeSpan.FromSeconds(1);

            satelliteTimer.Tick +=
                SatelliteTimer_Tick;

            satelliteTimer.Start();

            DataContext = this;
        }

        // =========================
        // INIT MAP
        // =========================

        private async Task InitMap()
        {
            await MapView.EnsureCoreWebView2Async();

            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "map.html");

            MapView.Source =
                new Uri(path);
        }

        private async void Button_Click(object sender,RoutedEventArgs e) { }

        // =========================
        // VISUAL PROCESSOR
        // =========================

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
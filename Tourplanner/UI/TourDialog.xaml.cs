using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Tourplanner.DAL.Entities;

namespace TourPlanner.UI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class TourDialog : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TourDialog()
        {
            InitializeComponent();
            Result = new Tour();
        }

        public TourDialog(Tour selectedTour)
        {
            InitializeComponent();

            txtName.Text = selectedTour.Name;
            txtDescription.Text = selectedTour.Description;
            txtFrom.Text = selectedTour.From;
            txtTo.Text = selectedTour.To;
            txtTransportType.Text = selectedTour.TransportType;

            Result = selectedTour;

            this.Title = "Edit Tour";
            ((Button)FindName("AddButton")).Content = "Save Changes";
        }

        public Tour Result { get; private set; }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtDescription.Text) ||
                string.IsNullOrWhiteSpace(txtFrom.Text) ||
                string.IsNullOrWhiteSpace(txtTo.Text) ||
                string.IsNullOrWhiteSpace(txtTransportType.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success;

                bool fromToChanged = Result == null || txtFrom.Text != Result.From || txtTo.Text != Result.To;
                bool transportTypeChanged = Result == null || txtTransportType.Text != Result.TransportType;

                if (fromToChanged || transportTypeChanged)
                {
                    var (distance, estimatedTime, success1) = await FetchTourData(txtFrom.Text, txtTo.Text, txtTransportType.Text);

                    if (success1)
                    {
                        Result.Distance = distance;
                        Result.EstimatedTime = estimatedTime;
                    }
                    else
                    {
                        Result.Distance = 0;
                        Result.EstimatedTime = TimeSpan.Zero;
                        MessageBox.Show("Failed to fetch tour data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.DialogResult = false;
                        this.Close();
                        return;
                    }
                }

                if (fromToChanged)
                {
                    
                    var (path, success2) = await FetchTourImage(txtFrom.Text, txtTo.Text);
                    if (success2)
                    {
                        Result.MapPath = path;
                    }
                    else
                    {
                        Result.MapPath = string.Empty;
                        MessageBox.Show("Failed to fetch tour image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.DialogResult = false;
                        this.Close();
                    }
                }

                Result.Name = txtName.Text;
                Result.Description = txtDescription.Text;
                Result.From = txtFrom.Text;
                Result.To = txtTo.Text;
                Result.TransportType = txtTransportType.Text;

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to retrieve tour details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task<(double Latitude, double Longitude)> GetCoordinatesFromCityName(string city)
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Tourplanner_Project");

            string url = $"https://nominatim.openstreetmap.org/search?city={Uri.EscapeDataString(city)}&format=json&limit=1";
            var response = await client.GetStringAsync(url);
            var locationResults = JsonConvert.DeserializeObject<List<dynamic>>(response);

            if (locationResults.Count > 0)
            {
                double latitude = locationResults[0].lat;
                double longitude = locationResults[0].lon;
                return (latitude, longitude);
            }
            else
            {
                throw new Exception("No location found for specified city.");
            }
        }


        private async Task<(double Distance, TimeSpan EstimatedTime, bool Success)> FetchTourData(string fromCity, string toCity, string transportType)
        {
            try
            {
                var fromCoords = await GetCoordinatesFromCityName(fromCity);
                var toCoords = await GetCoordinatesFromCityName(toCity);

                client.DefaultRequestHeaders.Add("User-Agent", "Tourplanner_Project");
                var apiKey = Environment.GetEnvironmentVariable("API_KEY");

                string url = $"https://api.openrouteservice.org/v2/directions/{Uri.EscapeDataString(transportType)}?api_key={apiKey}&start={fromCoords.Longitude.ToString("F6", CultureInfo.InvariantCulture)},{fromCoords.Latitude.ToString("F6", CultureInfo.InvariantCulture)}&end={toCoords.Longitude.ToString("F6", CultureInfo.InvariantCulture)},{toCoords.Latitude.ToString("F6", CultureInfo.InvariantCulture)}";

                var response = await client.GetStringAsync(url);

                dynamic routeData = JsonConvert.DeserializeObject(response);
                double distance = routeData.features[0].properties.summary.distance;
                double time = routeData.features[0].properties.summary.duration;
                TimeSpan estimatedTime = TimeSpan.FromSeconds(time);

                return (distance, estimatedTime, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to fetch tour data for {fromCity} to {toCity} using {transportType}");
                return (0, TimeSpan.Zero, false);
            }
        }

        private async Task<(string FilePath, bool Success)> FetchTourImage(string fromCity, string toCity)
        {
            try
            {
                var fromCoords = await GetCoordinatesFromCityName(fromCity);
                var toCoords = await GetCoordinatesFromCityName(toCity);

                var (tileX, tileY, zoomLevel) = CalculateTileCoverage(fromCoords, toCoords);
                string tileUrl = $"https://tile.openstreetmap.org/{zoomLevel}/{tileX}/{tileY}.png";

                return (await DownloadAndSaveTile(tileUrl), true);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to fetch tour image for {fromCity} to {toCity}");
                return (string.Empty, false);
            }
        }


        private async Task<string> DownloadAndSaveTile(string tileUrl)
        {
            string baseDir = Environment.GetEnvironmentVariable("BASE_DIR");
            string tilesDir = System.IO.Path.Combine(baseDir, "tiles");
            Directory.CreateDirectory(tilesDir); 

            string localFilename = System.IO.Path.Combine(tilesDir, System.IO.Path.GetFileName(tileUrl));

            // Check if the file already exists
            if (File.Exists(localFilename))
            {
                return localFilename;
            }

            using (var response = await client.GetAsync(tileUrl))
            {
                if (response.IsSuccessStatusCode)
                {
                    byte[] imageData = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(localFilename, imageData);
                    return localFilename;
                }
                else
                {
                    throw new Exception("Failed to download image.");
                }
            }
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }


        private (int tileX, int tileY, int zoomLevel) CalculateTileCoverage(
            (double Latitude, double Longitude) fromCoords, (double Latitude, double Longitude) toCoords)
        {
            int zoomLevel = 18;

            while (zoomLevel > 0)
            {
                PointF fromTilePoint = WorldToTilePos(fromCoords.Longitude, fromCoords.Latitude, zoomLevel);
                PointF toTilePoint = WorldToTilePos(toCoords.Longitude, toCoords.Latitude, zoomLevel);

                if ((int)fromTilePoint.X == (int)toTilePoint.X && (int)fromTilePoint.Y == (int)toTilePoint.Y)
                {
                    int tileX = (int)fromTilePoint.X;
                    int tileY = (int)fromTilePoint.Y;
                    return (tileX, tileY, zoomLevel);
                }
                zoomLevel--;
            }

            return (-1, -1, -1);
        }


        public PointF WorldToTilePos(double lon, double lat, int zoom)
        {
            PointF p = new PointF();
            p.X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                        1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));
            return p;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

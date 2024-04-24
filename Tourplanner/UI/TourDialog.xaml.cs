using Newtonsoft.Json;
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

        public TourDialog()
        {
            InitializeComponent();
        }

        public TourDialog(Tour selectedTour)
        {
            txtName.Text = selectedTour.Name;
            txtDescription.Text = selectedTour.Description;
            txtFrom.Text = selectedTour.From;
            txtTo.Text = selectedTour.To;
            txtTransportType.Text = selectedTour.TransportType;

            this.Title = "Edit Tour";
            ((Button)FindName("AddButton")).Content = "Save Changes";
        }

        public Tour Result { get; private set; }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if all required fields are filled out
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
                // Fetch additional details from the API
                var apiData = await FetchTourDetailsFromAPI(txtFrom.Text, txtTo.Text, txtTransportType.Text);

                // Create a new Tour object with the input data and the fetched API data
                Result = new Tour
                {
                    Name = txtName.Text,
                    Description = txtDescription.Text,
                    From = txtFrom.Text,
                    To = txtTo.Text,
                    TransportType = txtTransportType.Text,
                    Distance = apiData.Distance,
                    EstimatedTime = apiData.EstimatedTime,
                    MapPath = apiData.MapPath
                };

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to retrieve route details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async Task<(double Distance, TimeSpan EstimatedTime, string MapPath)> FetchTourDetailsFromAPI(string fromCity, string toCity, string transportType)
        {
            var fromCoords = await GetCoordinatesFromCityName(fromCity);
            var toCoords = await GetCoordinatesFromCityName(toCity);

            client.DefaultRequestHeaders.Add("User-Agent", "Tourplanner_Project");

            string url = $"https://api.openrouteservice.org/v2/directions/{Uri.EscapeDataString(transportType)}?api_key=5b3ce3597851110001cf6248fe33517ac22446b78d99e5c4fa6adc09&start={fromCoords.Longitude.ToString("F6", CultureInfo.InvariantCulture)},{fromCoords.Latitude.ToString("F6", CultureInfo.InvariantCulture)}&end={toCoords.Longitude.ToString("F6", CultureInfo.InvariantCulture)},{toCoords.Latitude.ToString("F6", CultureInfo.InvariantCulture)}";
            var response = await client.GetStringAsync(url);

            dynamic routeData = JsonConvert.DeserializeObject(response);

            double distance = routeData.features[0].properties.summary.distance;
            double time = routeData.features[0].properties.summary.duration;
            TimeSpan estimatedTime = TimeSpan.FromSeconds(time);

            // Calculate the map tile to cover the whole route
            var (tileX, tileY, zoomLevel) = CalculateTileCoverage(fromCoords, toCoords);
            string tileUrl = $"https://tile.openstreetmap.org/{zoomLevel}/{tileX}/{tileY}.png";

            // Download the tile and save locally
            string localPath = await DownloadAndSaveTile(tileUrl);

            return (distance, estimatedTime, localPath);
        }

        private async Task<string> DownloadAndSaveTile(string tileUrl)
        {
            string baseDir = Environment.GetEnvironmentVariable("BASE_DIR");
            string tilesDir = System.IO.Path.Combine(baseDir, "tiles");
            Directory.CreateDirectory(tilesDir);  // Ensure directory exists

            string localFilename = System.IO.Path.Combine(tilesDir, System.IO.Path.GetFileName(tileUrl));
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

        private (int tileX, int tileY, int zoomLevel) CalculateTileCoverage((double Latitude, double Longitude) fromCoords, (double Latitude, double Longitude) toCoords)
        {
            int zoomLevel = CalculateDynamicZoomLevel(fromCoords, toCoords);
            var midpoint = CalculateGeographicMidpoint(fromCoords, toCoords);
            PointF tilePoint = WorldToTilePos(midpoint.Longitude, midpoint.Latitude, zoomLevel);

            // Convert PointF to integer tile coordinates
            int tileX = (int)Math.Floor(tilePoint.X);
            int tileY = (int)Math.Floor(tilePoint.Y);

            return (tileX, tileY, zoomLevel);
        }

        private (double Latitude, double Longitude) CalculateGeographicMidpoint((double Latitude, double Longitude) fromCoords, (double Latitude, double Longitude) toCoords)
        {
            double midLatitude = (fromCoords.Latitude + toCoords.Latitude) / 2;
            double midLongitude = (fromCoords.Longitude + toCoords.Longitude) / 2;
            return (midLatitude, midLongitude);
        }

        private int CalculateDynamicZoomLevel((double Latitude, double Longitude) fromCoords, (double Latitude, double Longitude) toCoords)
        {
            // Earth's radius in meters at the equator
            const double earthRadiusAtEquator = 6378137.0;
            // Circumference in meters at the equator
            const double earthCircumference = 2 * Math.PI * earthRadiusAtEquator;

            // Calculate the distance in meters at the equator for one degree of longitude
            double metersPerDegree = earthCircumference / 360.0;

            // Calculate the differences in degrees
            double latDiffDegrees = Math.Abs(fromCoords.Latitude - toCoords.Latitude);
            double lonDiffDegrees = Math.Abs(fromCoords.Longitude - toCoords.Longitude);

            // Calculate the differences in meters at the equator
            double maxDiffMeters = Math.Max(latDiffDegrees, lonDiffDegrees) * metersPerDegree;

            // Start at zoom level 0 and increase until we find the appropriate zoom level
            int zoomLevel = 0;
            double mpp = earthCircumference / 256; // meters per pixel at zoom level 0 for 256px tiles
            while (mpp * 256 > maxDiffMeters && zoomLevel < 18)
            {
                zoomLevel++;
                mpp /= 2;
            }

            // Ensure we don't go below zoom level 0
            return Math.Max(0, zoomLevel - 1); // Subtract 1 to fit the entire area
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

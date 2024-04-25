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
                bool fromToChanged = Result == null || txtFrom.Text != Result.From || txtTo.Text != Result.To;
                bool transportTypeChanged = Result == null || txtTransportType.Text != Result.TransportType;

                if (fromToChanged || transportTypeChanged)
                {
                    var (distance, estimatedTime) = await FetchTourData(txtFrom.Text, txtTo.Text, txtTransportType.Text);
                    Result.Distance = distance;
                    Result.EstimatedTime = estimatedTime;
                }

                if (fromToChanged)
                {
                    Result.MapPath = await FetchTourImage(txtFrom.Text, txtTo.Text);
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

        private async Task<(double Distance, TimeSpan EstimatedTime)> FetchTourData(string fromCity, string toCity, string transportType)
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

            return (distance, estimatedTime);
        }

        private async Task<string> FetchTourImage(string fromCity, string toCity)
        {
            var fromCoords = await GetCoordinatesFromCityName(fromCity);
            var toCoords = await GetCoordinatesFromCityName(toCity);

            var (tileX, tileY, zoomLevel) = CalculateTileCoverage(fromCoords, toCoords);
            string tileUrl = $"https://tile.openstreetmap.org/{zoomLevel}/{tileX}/{tileY}.png";

            return await DownloadAndSaveTile(tileUrl);
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


        private int CalculateZoomLevel((double Latitude, double Longitude) fromCoords, (double Latitude, double Longitude) toCoords)
        {
            double earthRadiusKm = 6371.0; // Radius of the Earth in kilometers
            double dLat = DegreesToRadians(toCoords.Latitude - fromCoords.Latitude);
            double dLon = DegreesToRadians(toCoords.Longitude - fromCoords.Longitude);
            double lat1 = DegreesToRadians(fromCoords.Latitude);
            double lat2 = DegreesToRadians(toCoords.Latitude);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = earthRadiusKm * c;

            return DistanceToZoomLevel(distance);
        }

        private int DistanceToZoomLevel(double distance)
        {
            //Does not work well
            if (distance < 30) return 10;
            else if (distance < 100) return 9;
            else if (distance < 300) return 7;
            else if (distance < 600) return 6;
            else if (distance < 1200) return 5;
            else if (distance < 2400) return 4;
            else if (distance < 4800) return 3;
            else return 2;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }


        private (int tileX, int tileY, int zoomLevel) CalculateTileCoverage(
            (double Latitude, double Longitude) fromCoords,
            (double Latitude, double Longitude) toCoords)
        {
            // Calculate dynamic zoom level based on the distance between the points
            int zoomLevel = CalculateZoomLevel(fromCoords, toCoords);

            var midpoint = CalculateGeographicMidpoint(fromCoords, toCoords);
            PointF tilePoint = WorldToTilePos(midpoint.Longitude, midpoint.Latitude, zoomLevel);

            // Convert PointF to integer tile coordinates using rounding to improve accuracy
            int tileX = (int)Math.Round(tilePoint.X);
            int tileY = (int)Math.Round(tilePoint.Y);

            return (tileX, tileY, zoomLevel);
        }


        private (double Latitude, double Longitude) CalculateGeographicMidpoint((double Latitude, double Longitude) fromCoords, (double Latitude, double Longitude) toCoords)
        {
            double midLatitude = (fromCoords.Latitude + toCoords.Latitude) / 2;
            double midLongitude = (fromCoords.Longitude + toCoords.Longitude) / 2;
            return (midLatitude, midLongitude);
        }

        private double CalculateGeographicalSpanInMeters((double Latitude, double Longitude) fromCoords, (double Latitude, double Longitude) toCoords, double latitudeRad)
        {
            // Average Earth circumference in meters
            const double earthCircumference = 40075016.686;

            // Differences in degrees
            double latDiff = Math.Abs(fromCoords.Latitude - toCoords.Latitude);
            double lonDiff = Math.Abs(fromCoords.Longitude - toCoords.Longitude);

            // Calculate span for the widest dimension
            double latSpan = latDiff * earthCircumference / 360.0;
            double lonSpan = lonDiff * earthCircumference * Math.Cos(latitudeRad) / 360.0;

            // Return the maximum span in meters
            return Math.Max(latSpan, lonSpan);
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

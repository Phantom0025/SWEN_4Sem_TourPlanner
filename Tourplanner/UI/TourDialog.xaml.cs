using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            client.DefaultRequestHeaders.Add("User-Agent", "Brave");

            string url = $"https://nominatim.openstreetmap.org/search?city={Uri.EscapeDataString(city)}&format=json&limit=1";
            var response = await client.GetStringAsync(url);
            var locationResults = JsonConvert.DeserializeObject<List<dynamic>>(response);

            if (locationResults.Count > 0)
            {
                double latitude = (double)locationResults[0].lat;
                double longitude = (double)locationResults[0].lon;
                return (latitude, longitude);
            }
            else
            {
                throw new Exception("No location found for specified city.");
            }
        }

        private async Task<(double Distance, string EstimatedTime, string MapPath)> FetchTourDetailsFromAPI(string fromCity, string toCity, string transportType)
        {
            var fromCoords = await GetCoordinatesFromCityName(fromCity);
            var toCoords = await GetCoordinatesFromCityName(toCity);

            string routeApiResponse = await client.GetStringAsync($"https://api.openrouteservice.org/v2/directions/{transportType}/geojson?api_key=YOUR_API_KEY&start={fromCoords.Longitude},{fromCoords.Latitude}&end={toCoords.Longitude},{toCoords.Latitude}");
            dynamic routeData = JsonConvert.DeserializeObject(routeApiResponse);

            double distance = routeData.features[0].properties.summary.distance;
            string estimatedTime = ConvertTime(routeData.features[0].properties.summary.duration);
            //string mapPath = $"https://tile.openstreetmap.org/{/* appropriate parameters */}.png";  // Example placeholder
            string mapPath = "/image.png";

            return (distance, estimatedTime, mapPath);
        }

        private string ConvertTime(double seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.ToString(@"hh\:mm\:ss");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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

            this.Title = "Edit Tour";
            ((Button)FindName("AddButton")).Content = "Save Changes";
        }

        public Tour Result { get; private set; }

        private void AddButton_Click(object sender, RoutedEventArgs e)
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

            // Update or create a new Tour object with the input data
            Result = new Tour
            {
                Name = txtName.Text,
                Description = txtDescription.Text,
                From = txtFrom.Text,
                To = txtTo.Text,
                TransportType = txtTransportType.Text
            };

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

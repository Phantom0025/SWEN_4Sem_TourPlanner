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
using System.Xml.Linq;
using Tourplanner.DAL.Entities;

namespace TourPlanner.UI
{
    /// <summary>
    /// Interaction logic for TourLogDialog.xaml
    /// </summary>
    public partial class TourLogDialog : Window
    {
        public TourLogDialog()
        {
            InitializeComponent();
        }

        public TourLogDialog(TourLog selectedTourLog)
        {
            InitializeComponent();

            datePickerDate.SelectedDate = selectedTourLog.DateTime.Date;
            txtTime.Text = selectedTourLog.DateTime.TimeOfDay.ToString();
            txtComment.Text = selectedTourLog.Comment;
            sliderDifficulty.Value = selectedTourLog.Difficulty;
            txtTotalDistance.Text = selectedTourLog.TotalDistance.ToString();
            txtTotalTime.Text = selectedTourLog.TotalTime.ToString();
            sliderRating.Value = selectedTourLog.Rating;

            this.Title = "Edit Tour Log";
            ((Button)FindName("AddButton")).Content = "Save Changes";
        }


        public TourLog Result { get; private set; }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Initialize LanguageFilter
            LanguageFilter languageFilter = new LanguageFilter();

            if (datePickerDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TimeSpan time;
            if (!TimeSpan.TryParse(txtTime.Text, out time))
            {
                MessageBox.Show("Please enter a valid time in format hh:mm:ss.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dateTimeUtc = DateTime.SpecifyKind(datePickerDate.SelectedDate.Value.Date + time, DateTimeKind.Utc);

            if (string.IsNullOrWhiteSpace(txtComment.Text) ||
                string.IsNullOrWhiteSpace(txtTotalDistance.Text) ||
                string.IsNullOrWhiteSpace(txtTotalTime.Text))
            {
                MessageBox.Show("Please fill in all required fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double totalDistance;
            if (!double.TryParse(txtTotalDistance.Text, out totalDistance))
            {
                MessageBox.Show("Please enter a valid number for total distance.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TimeSpan totalTime;
            if (!TimeSpan.TryParse(txtTotalTime.Text, out totalTime))
            {
                MessageBox.Show("Please enter a valid time for total time in format hh:mm:ss.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check for offensive words in the comment
            if (languageFilter.ContainsOffensiveWords(txtComment.Text))
            {
                MessageBox.Show("The comment contains offensive language. Please revise your comment.", "Offensive Language Detected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update or create a new Tour Log object with the input data
            Result = new TourLog
            {
                DateTime = dateTimeUtc,
                Comment = txtComment.Text,
                Difficulty = (int)sliderDifficulty.Value,
                TotalDistance = totalDistance,
                TotalTime = totalTime,
                Rating = sliderRating.Value
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

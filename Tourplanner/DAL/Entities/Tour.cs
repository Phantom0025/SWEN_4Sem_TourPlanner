using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tourplanner.DAL.Entities
{
    public class Tour : INotifyPropertyChanged
    {
        public Tour() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TourId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string From { get; set; }

        [Required]
        public string To { get; set; }

        [Required]
        public string TransportType { get; set; }

        [Required]
        public double Distance { get; set; }

        [Required]
        public TimeSpan EstimatedTime { get; set; }

        [Required]
        public string MapPath { get; set; }

        public ObservableCollection<TourLog> TourLogs = new ObservableCollection<TourLog>();

        [NotMapped]
        public int Popularity => TourLogs.Count;

        [NotMapped]
        public double AverageRating => TourLogs.Any() ? TourLogs.Average(log => log.Rating) : 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}

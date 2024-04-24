using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tourplanner.DAL.Entities
{
    public class Tour
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TourId { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public required string From { get; set; }

        [Required]
        public required string To { get; set; }

        [Required]
        public required string TransportType { get; set; }

        [Required]
        public required double Distance { get; set; }

        [Required]
        public required TimeSpan EstimatedTime { get; set; }

        [Required]
        public required string MapPath { get; set; }

        public virtual List<TourLog> TourLogs { get; set; } = new List<TourLog>();

        // Computed attributes are not stored in the database, so they don't need annotations.
        [NotMapped]
        public int Popularity => TourLogs.Count;

        [NotMapped]
        public double AverageRating => TourLogs.Any() ? TourLogs.Average(log => log.Rating) : 0;
    }

}

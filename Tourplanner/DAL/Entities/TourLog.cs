using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tourplanner.DAL.Entities
{
    public class TourLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TourLogId { get; set; }

        [Required]
        public int TourId { get; set; }

        [ForeignKey("TourId")]
        [Required]
        public virtual Tour Tour { get; set; }

        public DateTime DateTime { get; set; }

        public string? Comment { get; set; }

        public int Difficulty { get; set; }

        public double TotalDistance { get; set; }

        public TimeSpan TotalTime { get; set; }

        public double Rating { get; set; }
    }

}

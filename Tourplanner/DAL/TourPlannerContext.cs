using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tourplanner.DAL.Entities;



namespace TourPlanner.DAL
{
    public class TourPlannerContext : DbContext
    {
        public TourPlannerContext(DbContextOptions<TourPlannerContext> options)
            : base(options)
        {
        }

        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourLog> TourLogs { get; set; }

        // Remainder of the context class...
    }
}


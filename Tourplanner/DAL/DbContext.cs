using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tourplanner;
using Tourplanner.DAL.Entities;

namespace TourPlanner.DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourLog> TourLogs { get; set; }

        private static Logger log = LogManager.GetCurrentClassLogger();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION");
                if (connectionString is null)
                {
                    log.Error("Database connection string not found in environment variables.", new ConfigurationErrorsException());
                    throw new InvalidOperationException("Database connection string not found in environment variables.");
                }
                optionsBuilder.UseNpgsql(connectionString);
                log.Info("Database configured successfully.");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tour>()
                .HasMany(t => t.TourLogs)
                .WithOne(l => l.Tour)
                .HasForeignKey(l => l.TourId);

            //// Seed data for the Tour entity
            //modelBuilder.Entity<Tour>().HasData(
            //    new Tour { TourId = 1, Name = "Vienna City Tour", Description = "Explore the historic city center.", From = "Vienna", To = "Vienna", TransportType = "Bus"},
            //    new Tour { TourId = 2, Name = "Salzburg Historic Tour", Description = "Visit the birthplace of Mozart.", From = "Salzburg", To = "Salzburg", TransportType = "Walking" }
            //);

            //// Seed data for the TourLog entity
            //modelBuilder.Entity<TourLog>().HasData(
            //    new TourLog { TourLogId = 1, TourId = 1, DateTime = DateTime.UtcNow, Comment = "Amazing experience", Difficulty = 2, TotalTime = TimeSpan.FromHours(3), Rating = 4.5 },
            //    new TourLog { TourLogId = 2, TourId = 2, DateTime = DateTime.UtcNow.AddDays(-1), Comment = "Very informative", Difficulty = 1, TotalTime = TimeSpan.FromHours(2), Rating = 5.0 },
            //    new TourLog { TourLogId = 3, TourId = 1, DateTime = DateTime.UtcNow.AddDays(-2), Comment = "Great views", Difficulty = 3, TotalTime = TimeSpan.FromHours(4), Rating = 4.0 },
            //    new TourLog { TourLogId = 4, TourId = 2, DateTime = DateTime.UtcNow.AddDays(-3), Comment = "Interesting history", Difficulty = 2, TotalTime = TimeSpan.FromHours(2.5), Rating = 4.5 }
            //);
        }
    }
} 


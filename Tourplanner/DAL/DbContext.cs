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
                var connectionString = Environment.GetEnvironmentVariable("TOURPLANNER_DB_CONNECTION");
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

            // Define the relationship between Tour and TourLog
            modelBuilder.Entity<Tour>()
                .HasMany(t => t.TourLogs)
                .WithOne(l => l.Tour)
                .HasForeignKey(l => l.TourId);
        }
    }
} 


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tourplanner.DAL.Entities;

namespace TourPlanner.DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourLog> TourLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Replace these with your actual connection string and settings
            optionsBuilder.UseNpgsql("Host=my_host;Database=my_db;Username=my_user;Password=my_password");
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


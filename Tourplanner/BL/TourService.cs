using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tourplanner.DAL.Entities;
using TourPlanner.DAL;

namespace TourPlanner.BL
{
    public class TourService
    {
        private readonly AppDbContext _dbContext;  // Context for Entity Framework

        public TourService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddTour(Tour tour)
        {
            if (tour == null)
                throw new ArgumentNullException(nameof(tour));

            _dbContext.Tours.Add(tour);
            _dbContext.SaveChanges();
        }

        public bool DeleteTour(int tourId)
        {
            var tour = _dbContext.Tours.FirstOrDefault(t => t.TourId == tourId);
            if (tour == null)
                return false;

            _dbContext.Tours.Remove(tour);
            _dbContext.SaveChanges();
            return true;
        }

        public bool ModifyTour(Tour updatedTour)
        {
            var tour = _dbContext.Tours.FirstOrDefault(t => t.TourId == updatedTour.TourId);
            if (tour == null)
                return false;

            tour.Name = updatedTour.Name;
            tour.Description = updatedTour.Description;
            tour.From = updatedTour.From;
            tour.To = updatedTour.To;

            _dbContext.SaveChanges();
            return true;
        }

        public List<Tour> GetAllTours()
        {
            return _dbContext.Tours.ToList();
        }
    }
}

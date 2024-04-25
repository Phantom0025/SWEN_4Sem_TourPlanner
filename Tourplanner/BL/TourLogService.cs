using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tourplanner.DAL.Entities;
using TourPlanner.DAL;

namespace TourPlanner.BL
{
    public interface ITourLogService
    {
        void AddTourLog(TourLog tourLog);
        bool DeleteTourLog(Guid tourLogId);
        bool ModifyTourLog(TourLog tourLog);
        List<TourLog> GetTourLogsByTourId(Guid tourId);
    }

    public class TourLogService : ITourLogService
    {
        private readonly AppDbContext _dbContext;

        public TourLogService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddTourLog(TourLog tourLog)
        {
            if (tourLog == null)
                throw new ArgumentNullException(nameof(tourLog));

            _dbContext.TourLogs.Add(tourLog);
            _dbContext.SaveChanges();
        }

        public bool DeleteTourLog(Guid tourLogId)
        {
            var tourLog = _dbContext.TourLogs.FirstOrDefault(tl => tl.TourLogId == tourLogId);
            if (tourLog == null)
                return false;

            _dbContext.TourLogs.Remove(tourLog);
            _dbContext.SaveChanges();
            return true;
        }

        public bool ModifyTourLog(TourLog updatedTourLog)
        {
            var tourLog = _dbContext.TourLogs.FirstOrDefault(tl => tl.TourLogId == updatedTourLog.TourLogId);
            if (tourLog == null)
                return false;

            tourLog = updatedTourLog;

            _dbContext.SaveChanges();
            return true;
        }

        public List<TourLog> GetTourLogsByTourId(Guid tourId)
        {
            return _dbContext.TourLogs.Where(tl => tl.TourId == tourId).ToList();
        }
    }
}

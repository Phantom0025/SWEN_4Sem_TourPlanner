using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tourplanner.DAL.Entities;
using TourPlanner.DAL;
using iText.Layout;
using iText.Layout.Properties;

namespace TourPlanner.BL
{
    public interface ITourLogService
    {
        void AddTourLog(TourLog tourLog);
        bool DeleteTourLog(Guid tourLogId);
        bool ModifyTourLog(TourLog tourLog);
        List<TourLog> GetTourLogsByTourId(Guid tourId);
        void GenerateTourLogsReport(string fileName);
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

        public List<TourLog> GetAllTourLogs()
        {
            return _dbContext.TourLogs.ToList();
        }

        public List<TourLog> GetTourLogsByTourId(Guid tourId)
        {
            return _dbContext.TourLogs.Where(tl => tl.TourId == tourId).ToList();
        }

        public void GenerateTourLogsReport(string fileName)
        {
            var tourLogs = GetAllTourLogs();

            using (var writer = new PdfWriter(fileName))
            {
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new Document(pdf);

                    var title = new Paragraph("Tour Logs Report")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(20);
                    document.Add(title);

                    document.Add(new LineSeparator(new SolidLine()));

                    foreach (var log in tourLogs)
                    {
                        document.Add(new Paragraph($"Tour ID: {log.TourId}")
                            .SetFontSize(14)
                            .SetBold());
                        document.Add(new Paragraph($"Date: {log.DateTime.ToShortDateString()}")
                            .SetFontSize(12));
                        document.Add(new Paragraph($"Distance: {log.TotalDistance} km")
                            .SetFontSize(12));
                        document.Add(new Paragraph($"Duration: {log.TotalTime}")
                            .SetFontSize(12));
                        document.Add(new Paragraph($"Comment: {log.Comment}")
                            .SetFontSize(12));
                        document.Add(new Paragraph($"Rating: {log.Rating}/5")
                            .SetFontSize(12));
                        document.Add(new Paragraph("\n")); // Add space between tour logs
                    }

                    document.Close();
                }
            }
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Tourplanner.DAL.Entities;
using TourPlanner.DAL;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas.Draw;

namespace TourPlanner.BL
{
    public interface ITourService
    {
        void AddTour(Tour tour);
        bool DeleteTour(Guid tourId);
        bool ModifyTour(Tour updatedTour);
        List<Tour> GetAllTours();
        void ImportTours(string filePath);
        void ExportTours(string filePath);
        void GenerateToursReport(string fileName);
    }

    public class TourService : ITourService
    {
        private readonly AppDbContext _dbContext;

        private static Logger log = LogManager.GetCurrentClassLogger();

        public TourService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddTour(Tour tour)
        {
            if (tour == null)
                throw new ArgumentNullException(nameof(tour));

            bool tourExists = _dbContext.Tours.Any(t => t.Name == tour.Name && t.From == tour.From && t.To == tour.To);

            if (tourExists)
            {
                log.Warn($"Skipping import for duplicate tour: {tour.Name} from {tour.From} to {tour.To}");
            }
            else
            {
                _dbContext.Tours.Add(tour);
                _dbContext.SaveChanges();
                log.Info($"Added new tour: {tour.Name} from {tour.From} to {tour.To}");
            }
        }

        public bool DeleteTour(Guid tourId)
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

            tour = updatedTour;

            _dbContext.SaveChanges();
            return true;
        }

        public List<Tour> GetAllTours()
        {
            return _dbContext.Tours.ToList();
        }

        public void ImportTours(string filePath)
        {
            var jsonData = File.ReadAllText(filePath);
            var tours = JsonConvert.DeserializeObject<List<Tour>>(jsonData);
            foreach (var tour in tours)
            {
                AddTour(tour);
            }
        }

        public void ExportTours(string filePath)
        {
            var tours = GetAllTours();
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ContractResolver = new IgnorePropertiesResolver(new[] { "Popularity", "AverageRating" })
            };
            var jsonData = JsonConvert.SerializeObject(tours, settings);
            File.WriteAllText(filePath, jsonData);
        }


        public void GenerateToursReport(string fileName)
        {
            var tours = GetAllTours();

            using (var writer = new PdfWriter(fileName))
            {
                using (var pdf = new PdfDocument(writer))
                {
                    var document = new Document(pdf);

                    var title = new Paragraph("Tour Report")
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetFontSize(20);
                    document.Add(title);

                    document.Add(new LineSeparator(new SolidLine()));

                    foreach (var tour in tours)
                    {
                        document.Add(new Paragraph($"Tour: {tour.Name}")
                            .SetFontSize(14)
                            .SetBold());
                        document.Add(new Paragraph($"From: {tour.From}")
                            .SetFontSize(12));
                        document.Add(new Paragraph($"To: {tour.To}")
                            .SetFontSize(12));
                        document.Add(new Paragraph($"Distance: {tour.Distance} Meter")
                            .SetFontSize(12));
                        document.Add(new Paragraph($"Description: {tour.Description}")
                            .SetFontSize(12));
                        document.Add(new Paragraph("\n")); // Add space between tours
                    }

                    document.Close();
                }
            }
        }


        private class IgnorePropertiesResolver : DefaultContractResolver
        {
            private HashSet<string> _propsToIgnore;

            public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
            {
                _propsToIgnore = new HashSet<string>(propNamesToIgnore);
            }

            protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (_propsToIgnore.Contains(property.PropertyName))
                {
                    property.ShouldSerialize = instance => false;
                }
                return property;
            }
        }
    }
}

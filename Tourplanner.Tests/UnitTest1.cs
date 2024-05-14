using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework.Legacy;
using Tourplanner.DAL.Entities;
using TourPlanner.BL;
using TourPlanner.DAL;
using TourPlanner.UI;

namespace Tourplanner_test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Tour_CanBeInstantiated()
        {
            var tour = new Tour();
            tour.Name = "moin";
            ClassicAssert.NotNull(tour.Name);
        }

        [Test]
        public void TourLog_CanBeInstantiated()
        {
            var tourlog = new TourLog();
            tourlog.Comment = "Kommentar";
            ClassicAssert.NotNull(tourlog.Comment);
        }

        [Test]
        public void Popularity_ReturnsCorrectValue()
        {
            var tour = new Tour();
            tour.TourLogs.Add(new TourLog());
            tour.TourLogs.Add(new TourLog());

            var popularity = tour.Popularity;

            ClassicAssert.AreEqual(2, popularity);
        }

        [Test]
        public void AverageRating_ReturnsCorrectValue_WhenTourLogsExist()
        {
            var tour = new Tour();
            tour.TourLogs.Add(new TourLog { Rating = 3 });
            tour.TourLogs.Add(new TourLog { Rating = 4 });
            tour.TourLogs.Add(new TourLog { Rating = 5 });

            var averageRating = tour.AverageRating;

            ClassicAssert.AreEqual(4, averageRating);
        }

        [Test]
        public void TourLog_CanBeInstantiated_WithDefaultConstructor()
        {
            var tourLog = new TourLog();
            ClassicAssert.NotNull(tourLog);
        }

        [Test]
        public void TourLog_CanBeInstantiated_WithValues()
        {
            var expectedDateTime = DateTime.Now.Date;
            var tourLog = new TourLog
            {
                DateTime = DateTime.Now
            };

            var actualDateTime = tourLog.DateTime.Date;

            ClassicAssert.AreEqual(expectedDateTime, actualDateTime);
        }

        [Test]
        public void TourLog_HasCorrectDefaults()
        {
            var tourLog = new TourLog();

            ClassicAssert.AreEqual(default(DateTime), tourLog.DateTime);
            ClassicAssert.IsNull(tourLog.Comment);
            ClassicAssert.AreEqual(0, tourLog.Difficulty);
            ClassicAssert.AreEqual(0, tourLog.TotalDistance);
            ClassicAssert.AreEqual(TimeSpan.Zero, tourLog.TotalTime);
            ClassicAssert.AreEqual(0, tourLog.Rating);
        }

        [Test]
        public void TourPopularity_ReturnsZero_WhenNoLogs()
        {
            var tour = new Tour();

            ClassicAssert.AreEqual(0, tour.Popularity);
        }

        [Test]
        public void TourAverageRating_ReturnsZero_WhenNoLogs()
        {
            var tour = new Tour();

            ClassicAssert.AreEqual(0, tour.AverageRating);
        }

        [Test]
        public void Tour_HasCorrectDefaults()
        {
            var tour = new Tour();

            ClassicAssert.AreEqual(default(Guid), tour.TourId);
            ClassicAssert.IsNull(tour.Name);
            ClassicAssert.IsNull(tour.Description);
            ClassicAssert.IsNull(tour.From);
            ClassicAssert.IsNull(tour.To);
            ClassicAssert.IsNull(tour.TransportType);
            ClassicAssert.AreEqual(0, tour.Distance);
            ClassicAssert.AreEqual(TimeSpan.Zero, tour.EstimatedTime);
            ClassicAssert.IsNull(tour.MapPath);
            ClassicAssert.AreEqual(0, tour.Popularity);
            ClassicAssert.AreEqual(0, tour.AverageRating);
        }

        [Test]
        public void TourLog_HasCorrectDefaults2()
        {
            var tourLog = new TourLog();

            ClassicAssert.AreEqual(default(Guid), tourLog.TourLogId);
            ClassicAssert.AreEqual(default(Guid), tourLog.TourId);
            ClassicAssert.IsNull(tourLog.Tour);
            ClassicAssert.AreEqual(default(DateTime), tourLog.DateTime);
            ClassicAssert.IsNull(tourLog.Comment);
            ClassicAssert.AreEqual(0, tourLog.Difficulty);
            ClassicAssert.AreEqual(0, tourLog.TotalDistance);
            ClassicAssert.AreEqual(TimeSpan.Zero, tourLog.TotalTime);
            ClassicAssert.AreEqual(0, tourLog.Rating);
        }

        [Test]
        public void TourLog_HasCorrectDateTime_WithDefaultConstructor()
        {
            var tourLog = new TourLog();

            ClassicAssert.AreEqual(default(DateTime), tourLog.DateTime);
        }

        [Test]
        public void TourLog_HasCorrectValues_WhenSet()
        {
            var dateTime = DateTime.Now;
            var comment = "Test Comment";
            var difficulty = 3;
            var totalDistance = 10.5;
            var totalTime = TimeSpan.FromHours(2);
            var rating = 4.5;

            var tourLog = new TourLog
            {
                DateTime = dateTime,
                Comment = comment,
                Difficulty = difficulty,
                TotalDistance = totalDistance,
                TotalTime = totalTime,
                Rating = rating
            };

            ClassicAssert.AreEqual(dateTime, tourLog.DateTime);
            ClassicAssert.AreEqual(comment, tourLog.Comment);
            ClassicAssert.AreEqual(difficulty, tourLog.Difficulty);
            ClassicAssert.AreEqual(totalDistance, tourLog.TotalDistance);
            ClassicAssert.AreEqual(totalTime, tourLog.TotalTime);
            ClassicAssert.AreEqual(rating, tourLog.Rating);
        }

        [Test]
        public void Tour_HasEmptyTourLogsCollection_WhenInstantiated()
        {
            var tour = new Tour();

            ClassicAssert.IsEmpty(tour.TourLogs);
        }

        [Test]
        public void TourLog_HasZeroTotalTime_WhenInstantiatedWithDefaultConstructor()
        {
            var tourLog = new TourLog();

            ClassicAssert.AreEqual(TimeSpan.Zero, tourLog.TotalTime);
        }

    }
}

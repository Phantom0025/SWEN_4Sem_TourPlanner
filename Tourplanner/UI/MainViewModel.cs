using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tourplanner.DAL.Entities;
using TourPlanner.BL;
using TourPlanner.DAL;
using static TourPlanner.BL.TourService;

namespace TourPlanner.UI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _dbContext;
        private Tour _selectedTour;
        private ObservableCollection<TourLog> _tourLogs;
        private static Logger log = LogManager.GetCurrentClassLogger();

        private TourService _tourService;
        private TourLogService _tourLogService;

        public ICommand AddTourCommand { get; private set; }
        public ICommand DeleteTourCommand { get; private set; }
        public ICommand ModifyTourCommand { get; private set; }

        public MainViewModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            Tours = new ObservableCollection<Tour>(_dbContext.Tours.Include(t => t.TourLogs).ToList());

            _tourService = new TourService(dbContext);
            //_tourLogService = new TourLogService(dbContext);

            AddTourCommand = new RelayCommand(AddTourExecute);
            DeleteTourCommand = new RelayCommand(DeleteTourExecute);
            ModifyTourCommand = new RelayCommand(ModifyTourExecute);
        }

        private void AddTourExecute(object parameter)
        {
            Tour tour = parameter as Tour;
            _tourService.AddTour(tour);
        }

        private void DeleteTourExecute(object parameter)
        {
            int tourId = (int)parameter;
            _tourService.DeleteTour(tourId);
        }

        private void ModifyTourExecute(object parameter)
        {
            Tour updatedTour = parameter as Tour;
            _tourService.ModifyTour(updatedTour);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<Tour> Tours { get; set; }

        public Tour SelectedTour
        {
            get => _selectedTour;
            set
            {
                if (_selectedTour != value)
                {
                    _selectedTour = value;
                    TourLogs = new ObservableCollection<TourLog>(_selectedTour?.TourLogs ?? new List<TourLog>());
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<TourLog> TourLogs
        {
            get => _tourLogs;
            private set
            {
                if (_tourLogs != value)
                {
                    _tourLogs = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}

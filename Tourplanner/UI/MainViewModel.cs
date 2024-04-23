using Microsoft.EntityFrameworkCore;
using NLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tourplanner.DAL.Entities;
using TourPlanner.BL;
using TourPlanner.DAL;

namespace TourPlanner.UI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _dbContext;
        private Tour _selectedTour;
        private ObservableCollection<TourLog> _tourLogs;
        private static Logger log = LogManager.GetCurrentClassLogger();

        private ITourService _tourService;
        private ITourLogService _tourLogService;

        public ICommand AddTourCommand { get; private set; }
        public ICommand DeleteTourCommand { get; private set; }
        public ICommand ModifyTourCommand { get; private set; }

        public ICommand AddTourLogCommand { get; private set; }
        public ICommand DeleteTourLogCommand { get; private set; }
        public ICommand ModifyTourLogCommand { get; private set; }

        public MainViewModel(AppDbContext dbContext, ITourService tourService, ITourLogService tourLogService)
        {
            _dbContext = dbContext;
            Tours = new ObservableCollection<Tour>(_dbContext.Tours.Include(t => t.TourLogs).ToList());

            _tourService = tourService;
            _tourLogService = tourLogService;

            AddTourCommand = new RelayCommand(AddTourExecute);
            DeleteTourCommand = new RelayCommand(DeleteTourExecute);
            ModifyTourCommand = new RelayCommand(ModifyTourExecute);
        }

        private void AddTourExecute(object parameter)
        {
            TourDialog dialog = new TourDialog();
            var result = dialog.ShowDialog();
            if (result == true)
            {
                Tour newTour = dialog.Result;
                _tourService.AddTour(newTour);
                Tours.Add(newTour);
            }
            else
            {
                // Handle dialog close with cancel or close
            }
        }

        private void DeleteTourExecute(object parameter)
        {
            if (SelectedTour != null)
            {
                _tourService.DeleteTour(SelectedTour.TourId);
                Tours.Remove(SelectedTour);
            }
        }

        private void ModifyTourExecute(object parameter)
        {
            if (SelectedTour != null)
            {
                TourDialog dialog = new TourDialog(SelectedTour);
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    _tourService.ModifyTour(dialog.Result);
                    Tours[Tours.IndexOf(SelectedTour)] = dialog.Result;
                }
                else
                {
                    // Handle dialog close with cancel or close
                }
            }
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

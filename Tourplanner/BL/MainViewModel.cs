﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
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
        private TourLog _selectedTourLog;
        private static Logger log = LogManager.GetCurrentClassLogger();

        private ITourService _tourService;
        private ITourLogService _tourLogService;

        public ICommand AddTourCommand { get; private set; }
        public ICommand DeleteTourCommand { get; private set; }
        public ICommand ModifyTourCommand { get; private set; }

        public ICommand AddTourLogCommand { get; private set; }
        public ICommand DeleteTourLogCommand { get; private set; }
        public ICommand ModifyTourLogCommand { get; private set; }

        public ICommand ImportCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }

        public MainViewModel(AppDbContext dbContext, ITourService tourService, ITourLogService tourLogService)
        {
            _dbContext = dbContext;
            Tours = new ObservableCollection<Tour>(_dbContext.Tours.Include(t => t.TourLogs).ToList());

            _tourService = tourService;
            _tourLogService = tourLogService;

            AddTourCommand = new RelayCommand(AddTourExecute);
            DeleteTourCommand = new RelayCommand(DeleteTourExecute);
            ModifyTourCommand = new RelayCommand(ModifyTourExecute);

            AddTourLogCommand = new RelayCommand(AddTourLogExecute);
            DeleteTourLogCommand = new RelayCommand(DeleteTourLogExecute);
            ModifyTourLogCommand = new RelayCommand(ModifyTourLogExecute);

            ImportCommand = new RelayCommand(ImportExecute);
            ExportCommand = new RelayCommand(ExportExecute);
        }

        private void ImportExecute(object parameter)
        {
            log.Info("Importing tours from file");

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import Tours"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _tourService.ImportTours(openFileDialog.FileName);
                Tours = new ObservableCollection<Tour>(_dbContext.Tours.Include(t => t.TourLogs).ToList());
            }
            else
            {
                log.Info("Import cancelled by user.");
            }
        }

        private void ExportExecute(object parameter)
        {
            log.Info("Exporting tours to file");

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Export Tours"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                _tourService.ExportTours(saveFileDialog.FileName);
            }
            else
            {
                log.Info("Export cancelled by user.");
            }
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
                log.Info("Added new tour: {0}", newTour.Name);
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
                log.Info("Deleted tour: {0}", SelectedTour.Name);
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
                    log.Info("Modified tour: {0}", dialog.Result.Name);
                    _tourService.ModifyTour(dialog.Result);
                    Tours[Tours.IndexOf(SelectedTour)] = dialog.Result;
                }
                else
                {
                    // Handle dialog close with cancel or close
                }
            }
        }

        private void AddTourLogExecute(object parameter)
        {
            if (SelectedTour != null)
            {
                TourLogDialog dialog = new TourLogDialog();
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    TourLog newTourLog = dialog.Result;
                    newTourLog.TourId = SelectedTour.TourId;
                    _tourLogService.AddTourLog(newTourLog);
                    SelectedTour.TourLogs.Add(newTourLog);
                    TourLogs.Add(newTourLog);
                    log.Info("Added new tour log to tour: {0}", SelectedTour.Name);
                }
                else
                {
                    // Handle dialog close with cancel or close
                }
            }
        }

        private void DeleteTourLogExecute(object parameter)
        {
            if (SelectedTour != null && SelectedTourLog != null)
            {
                log.Info("Deleted tour log from tour: {0}", SelectedTour.Name);
                _tourLogService.DeleteTourLog(SelectedTourLog.TourLogId);
                SelectedTour.TourLogs.Remove(SelectedTourLog);
                TourLogs.Remove(SelectedTourLog);
            }
        }

        private void ModifyTourLogExecute(object parameter)
        {
            if (SelectedTour != null && SelectedTourLog != null)
            {
                TourLogDialog dialog = new TourLogDialog(SelectedTourLog);
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    log.Info("Modified tour log from tour: {0}", SelectedTour.Name);
                    _tourLogService.ModifyTourLog(dialog.Result);
                    SelectedTour.TourLogs[SelectedTour.TourLogs.IndexOf(SelectedTourLog)] = dialog.Result;
                    TourLogs[TourLogs.IndexOf(SelectedTourLog)] = dialog.Result;
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

        private ObservableCollection<Tour> _tours;
        public ObservableCollection<Tour> Tours
        {
            get { return _tours; }
            set
            {
                _tours = value;
                OnPropertyChanged(nameof(Tours));
            }
        }

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

        public TourLog SelectedTourLog
        {
            get { return _selectedTourLog; }
            set
            {
                if (_selectedTourLog != value)
                {
                    _selectedTourLog = value;
                    OnPropertyChanged(nameof(SelectedTourLog));
                }
            }
        }

        private ObservableCollection<TourLog> _tourLogs;

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
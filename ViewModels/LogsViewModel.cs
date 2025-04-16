using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using VoltBettaManager.Helpers;
using VoltBettaManager.Models;

namespace VoltBettaManager.ViewModels
{
    /// <summary>
    /// ViewModel for the Logs view
    /// </summary>
    public class LogsViewModel : ViewModelBase
    {
        #region Fields
        
        private ObservableCollection<TankLog> _logs;
        private TankLog _selectedLog;
        private TankLog _editableLog;
        private TankLog _newLog;
        private string _searchText;
        private string _selectedCategory;
        private bool _isEditing;
        private bool _isAddingLog;
        private string _timeText;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Collection of all log entries
        /// </summary>
        public ObservableCollection<TankLog> Logs
        {
            get => _logs;
            set => SetProperty(ref _logs, value);
        }
        
        /// <summary>
        /// The currently selected log entry
        /// </summary>
        public TankLog SelectedLog
        {
            get => _selectedLog;
            set
            {
                if (SetProperty(ref _selectedLog, value) && value != null)
                {
                    EditableLog = CloneLog(value);
                    TimeText = EditableLog.LogDateTime.ToString("HH:mm");
                }
                OnPropertyChanged(nameof(EmptyStateVisibility));
            }
        }
        
        /// <summary>
        /// Editable copy of the selected log
        /// </summary>
        public TankLog EditableLog
        {
            get => _editableLog;
            set => SetProperty(ref _editableLog, value);
        }
        
        /// <summary>
        /// New log being created
        /// </summary>
        public TankLog NewLog
        {
            get => _newLog;
            set
            {
                SetProperty(ref _newLog, value);
                OnPropertyChanged(nameof(CanAddLog));
            }
        }
        
        /// <summary>
        /// Search text for filtering logs
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                OnPropertyChanged(nameof(FilteredLogs));
            }
        }
        
        /// <summary>
        /// Selected category for filtering logs
        /// </summary>
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                OnPropertyChanged(nameof(FilteredLogs));
            }
        }
        
        /// <summary>
        /// Indicates whether the selected log is being edited
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                SetProperty(ref _isEditing, value);
                OnPropertyChanged(nameof(SaveButtonVisibility));
                OnPropertyChanged(nameof(EditButtonVisibility));
            }
        }
        
        /// <summary>
        /// Indicates whether a new log is being added
        /// </summary>
        public bool IsAddingLog
        {
            get => _isAddingLog;
            set => SetProperty(ref _isAddingLog, value);
        }
        
        /// <summary>
        /// Time text for the selected log's datetime
        /// </summary>
        public string TimeText
        {
            get => _timeText;
            set
            {
                if (SetProperty(ref _timeText, value) && EditableLog != null)
                {
                    try
                    {
                        var time = TimeSpan.Parse(value);
                        EditableLog.LogDateTime = EditableLog.LogDateTime.Date + time;
                    }
                    catch { /* Ignore parsing errors */ }
                }
            }
        }
        
        /// <summary>
        /// Filtered logs based on search text and selected category
        /// </summary>
        public IEnumerable<TankLog> FilteredLogs
        {
            get
            {
                var filteredLogs = Logs.AsEnumerable();
                
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var search = SearchText.ToLower();
                    filteredLogs = filteredLogs.Where(l => 
                        l.Title.ToLower().Contains(search) || 
                        l.Description.ToLower().Contains(search) ||
                        l.Category.ToLower().Contains(search));
                }
                
                if (!string.IsNullOrWhiteSpace(SelectedCategory))
                {
                    filteredLogs = filteredLogs.Where(l => l.Category == SelectedCategory);
                }
                
                return filteredLogs.OrderByDescending(l => l.LogDateTime);
            }
        }
        
        /// <summary>
        /// List of distinct categories for filtering
        /// </summary>
        public IEnumerable<string> Categories
        {
            get
            {
                return Logs
                    .Select(l => l.Category)
                    .Distinct()
                    .OrderBy(c => c);
            }
        }
        
        /// <summary>
        /// Predefined category options
        /// </summary>
        public List<string> CategoryOptions { get; } = new List<string>
        {
            "General",
            "Water Change",
            "Feeding",
            "Behavior",
            "Health",
            "Tank Maintenance",
            "Water Parameters",
            "Equipment"
        };
        
        /// <summary>
        /// Visibility of the save button
        /// </summary>
        public Visibility SaveButtonVisibility => IsEditing ? Visibility.Visible : Visibility.Collapsed;
        
        /// <summary>
        /// Visibility of the edit button
        /// </summary>
        public Visibility EditButtonVisibility => IsEditing ? Visibility.Collapsed : Visibility.Visible;
        
        /// <summary>
        /// Visibility of the empty state message
        /// </summary>
        public Visibility EmptyStateVisibility => SelectedLog == null ? Visibility.Visible : Visibility.Collapsed;
        
        /// <summary>
        /// Indicates whether a new log can be added (validates required fields)
        /// </summary>
        public bool CanAddLog => NewLog != null && 
                               !string.IsNullOrWhiteSpace(NewLog.Title) && 
                               !string.IsNullOrWhiteSpace(NewLog.Description);
        
        #endregion
        
        #region Commands
        
        /// <summary>
        /// Command to add a new log
        /// </summary>
        public ICommand AddLogCommand { get; }
        
        /// <summary>
        /// Command to save a new log
        /// </summary>
        public ICommand SaveNewLogCommand { get; }
        
        /// <summary>
        /// Command to cancel adding a log
        /// </summary>
        public ICommand CancelAddLogCommand { get; }
        
        /// <summary>
        /// Command to edit the selected log
        /// </summary>
        public ICommand EditLogCommand { get; }
        
        /// <summary>
        /// Command to save changes to the selected log
        /// </summary>
        public ICommand SaveLogCommand { get; }
        
        /// <summary>
        /// Command to cancel editing the selected log
        /// </summary>
        public ICommand CancelEditCommand { get; }
        
        /// <summary>
        /// Command to delete the selected log
        /// </summary>
        public ICommand DeleteLogCommand { get; }
        
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the LogsViewModel class
        /// </summary>
        public LogsViewModel()
        {
            // Initialize collections
            Logs = new ObservableCollection<TankLog>();
            
            // Load initial data
            LoadLogs();
            
            // Initialize commands
            AddLogCommand = new RelayCommand(_ => AddLog());
            SaveNewLogCommand = new RelayCommand(_ => SaveNewLog(), _ => CanAddLog);
            CancelAddLogCommand = new RelayCommand(_ => CancelAddLog());
            EditLogCommand = new RelayCommand(_ => EditLog());
            SaveLogCommand = new RelayCommand(_ => SaveLog());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            DeleteLogCommand = new RelayCommand(_ => DeleteLog());
            
            // Initialize new log
            ResetNewLog();
        }
        
        /// <summary>
        /// Loads all logs from the database
        /// </summary>
        private void LoadLogs()
        {
            Logs.Clear();
            
            var logs = App.DatabaseManager.GetAllTankLogs();
            foreach (var log in logs)
            {
                Logs.Add(log);
            }
            
            OnPropertyChanged(nameof(Categories));
        }
        
        /// <summary>
        /// Creates a deep copy of a log entry
        /// </summary>
        /// <param name="source">The log to clone</param>
        /// <returns>A new log with the same values</returns>
        private TankLog CloneLog(TankLog source)
        {
            return new TankLog
            {
                Id = source.Id,
                Title = source.Title,
                Description = source.Description,
                LogDateTime = source.LogDateTime,
                Category = source.Category
            };
        }
        
        /// <summary>
        /// Shows the dialog for adding a new log
        /// </summary>
        private void AddLog()
        {
            ResetNewLog();
            IsAddingLog = true;
        }
        
        /// <summary>
        /// Saves a new log to the database
        /// </summary>
        private void SaveNewLog()
        {
            if (string.IsNullOrWhiteSpace(NewLog.Category))
            {
                NewLog.Category = "General";
            }
            
            int id = App.DatabaseManager.AddTankLog(NewLog);
            NewLog.Id = id;
            
            Logs.Add(NewLog);
            SelectedLog = NewLog;
            
            OnPropertyChanged(nameof(FilteredLogs));
            OnPropertyChanged(nameof(Categories));
            
            ResetNewLog();
            IsAddingLog = false;
        }
        
        /// <summary>
        /// Cancels adding a new log
        /// </summary>
        private void CancelAddLog()
        {
            IsAddingLog = false;
        }
        
        /// <summary>
        /// Resets the new log to default values
        /// </summary>
        private void ResetNewLog()
        {
            NewLog = new TankLog
            {
                LogDateTime = DateTime.Now,
                Category = "General"
            };
        }
        
        /// <summary>
        /// Enters edit mode for the selected log
        /// </summary>
        private void EditLog()
        {
            if (SelectedLog != null)
            {
                IsEditing = true;
            }
        }
        
        /// <summary>
        /// Saves changes to the selected log
        /// </summary>
        private void SaveLog()
        {
            if (EditableLog != null)
            {
                App.DatabaseManager.UpdateTankLog(EditableLog);
                
                // Update the item in the collection
                int index = Logs.IndexOf(SelectedLog);
                if (index >= 0)
                {
                    Logs[index] = EditableLog;
                    SelectedLog = EditableLog;
                }
                
                IsEditing = false;
                OnPropertyChanged(nameof(FilteredLogs));
                OnPropertyChanged(nameof(Categories));
            }
        }
        
        /// <summary>
        /// Cancels editing the selected log
        /// </summary>
        private void CancelEdit()
        {
            if (SelectedLog != null)
            {
                EditableLog = CloneLog(SelectedLog);
                TimeText = EditableLog.LogDateTime.ToString("HH:mm");
            }
            
            IsEditing = false;
        }
        
        /// <summary>
        /// Deletes the selected log
        /// </summary>
        private void DeleteLog()
        {
            if (SelectedLog != null)
            {
                var confirmResult = NotificationHelper.ShowConfirmation(
                    "Delete Log",
                    $"Are you sure you want to delete the log '{SelectedLog.Title}'?");
                    
                if (confirmResult)
                {
                    App.DatabaseManager.DeleteTankLog(SelectedLog.Id);
                    Logs.Remove(SelectedLog);
                    SelectedLog = null;
                    EditableLog = null;
                    
                    OnPropertyChanged(nameof(FilteredLogs));
                    OnPropertyChanged(nameof(Categories));
                }
            }
        }
    }
}

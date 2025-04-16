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
    /// ViewModel for the Maintenance view
    /// </summary>
    public class MaintenanceViewModel : ViewModelBase
    {
        #region Fields
        
        private ObservableCollection<MaintenanceTask> _tasks;
        private MaintenanceTask _selectedTask;
        private MaintenanceTask _editableTask;
        private MaintenanceTask _newTask;
        private string _searchText;
        private MaintenanceCategory? _selectedCategory;
        private bool _isEditing;
        private bool _isAddingTask;
        private bool _showCompleted;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Collection of all maintenance tasks
        /// </summary>
        public ObservableCollection<MaintenanceTask> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }
        
        /// <summary>
        /// The currently selected maintenance task
        /// </summary>
        public MaintenanceTask SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (SetProperty(ref _selectedTask, value) && value != null)
                {
                    EditableTask = CloneTask(value);
                }
                OnPropertyChanged(nameof(EmptyStateVisibility));
                OnPropertyChanged(nameof(ShowCompleteButtonVisibility));
            }
        }
        
        /// <summary>
        /// Editable copy of the selected task
        /// </summary>
        public MaintenanceTask EditableTask
        {
            get => _editableTask;
            set => SetProperty(ref _editableTask, value);
        }
        
        /// <summary>
        /// New task being created
        /// </summary>
        public MaintenanceTask NewTask
        {
            get => _newTask;
            set
            {
                SetProperty(ref _newTask, value);
                OnPropertyChanged(nameof(CanAddTask));
            }
        }
        
        /// <summary>
        /// Search text for filtering tasks
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                OnPropertyChanged(nameof(FilteredTasks));
            }
        }
        
        /// <summary>
        /// Selected category for filtering tasks
        /// </summary>
        public MaintenanceCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                OnPropertyChanged(nameof(FilteredTasks));
            }
        }
        
        /// <summary>
        /// Indicates whether the selected task is being edited
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
        /// Indicates whether a new task is being added
        /// </summary>
        public bool IsAddingTask
        {
            get => _isAddingTask;
            set => SetProperty(ref _isAddingTask, value);
        }
        
        /// <summary>
        /// Indicates whether completed tasks should be shown
        /// </summary>
        public bool ShowCompleted
        {
            get => _showCompleted;
            set
            {
                SetProperty(ref _showCompleted, value);
                OnPropertyChanged(nameof(FilteredTasks));
            }
        }
        
        /// <summary>
        /// Filtered tasks based on search text, selected category, and completion status
        /// </summary>
        public IEnumerable<MaintenanceTask> FilteredTasks
        {
            get
            {
                var filteredTasks = Tasks.AsEnumerable();
                
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var search = SearchText.ToLower();
                    filteredTasks = filteredTasks.Where(t => 
                        t.Title.ToLower().Contains(search) || 
                        t.Description?.ToLower().Contains(search) == true);
                }
                
                if (SelectedCategory.HasValue)
                {
                    filteredTasks = filteredTasks.Where(t => t.Category == SelectedCategory.Value);
                }
                
                if (!ShowCompleted)
                {
                    filteredTasks = filteredTasks.Where(t => t.Status != MaintenanceStatus.Completed);
                }
                
                // Sort by date and status (overdue tasks first)
                return filteredTasks.OrderBy(t => t.Status == MaintenanceStatus.Overdue ? 0 : 1)
                                   .ThenBy(t => t.GetNextScheduledDateTime());
            }
        }
        
        /// <summary>
        /// Options for maintenance categories
        /// </summary>
        public IEnumerable<MaintenanceCategory> CategoryOptions => Enum.GetValues(typeof(MaintenanceCategory)).Cast<MaintenanceCategory>();
        
        /// <summary>
        /// Visibility of the save button
        /// </summary>
        public Visibility SaveButtonVisibility => IsEditing ? Visibility.Visible : Visibility.Collapsed;
        
        /// <summary>
        /// Visibility of the edit button
        /// </summary>
        public Visibility EditButtonVisibility => IsEditing ? Visibility.Collapsed : Visibility.Visible;
        
        /// <summary>
        /// Visibility of the mark complete button
        /// </summary>
        public Visibility ShowCompleteButtonVisibility => (SelectedTask != null && SelectedTask.Status != MaintenanceStatus.Completed) ? Visibility.Visible : Visibility.Collapsed;
        
        /// <summary>
        /// Visibility of the empty state message
        /// </summary>
        public Visibility EmptyStateVisibility => SelectedTask == null ? Visibility.Visible : Visibility.Collapsed;
        
        /// <summary>
        /// Indicates whether a new task can be added (validates required fields)
        /// </summary>
        public bool CanAddTask => NewTask != null && !string.IsNullOrWhiteSpace(NewTask.Title);
        
        #endregion
        
        #region Commands
        
        /// <summary>
        /// Command to add a new maintenance task
        /// </summary>
        public ICommand AddTaskCommand { get; }
        
        /// <summary>
        /// Command to save a new task
        /// </summary>
        public ICommand SaveNewTaskCommand { get; }
        
        /// <summary>
        /// Command to cancel adding a task
        /// </summary>
        public ICommand CancelAddTaskCommand { get; }
        
        /// <summary>
        /// Command to edit the selected task
        /// </summary>
        public ICommand EditTaskCommand { get; }
        
        /// <summary>
        /// Command to save changes to the selected task
        /// </summary>
        public ICommand SaveTaskCommand { get; }
        
        /// <summary>
        /// Command to cancel editing the selected task
        /// </summary>
        public ICommand CancelEditCommand { get; }
        
        /// <summary>
        /// Command to delete the selected task
        /// </summary>
        public ICommand DeleteTaskCommand { get; }
        
        /// <summary>
        /// Command to mark the selected task as completed
        /// </summary>
        public ICommand CompleteTaskCommand { get; }
        
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the MaintenanceViewModel class
        /// </summary>
        public MaintenanceViewModel()
        {
            // Initialize collections
            Tasks = new ObservableCollection<MaintenanceTask>();
            
            // Load initial data
            LoadTasks();
            
            // Initialize commands
            AddTaskCommand = new RelayCommand(_ => AddTask());
            SaveNewTaskCommand = new RelayCommand(_ => SaveNewTask(), _ => CanAddTask);
            CancelAddTaskCommand = new RelayCommand(_ => CancelAddTask());
            EditTaskCommand = new RelayCommand(_ => EditTask());
            SaveTaskCommand = new RelayCommand(_ => SaveTask());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            DeleteTaskCommand = new RelayCommand(_ => DeleteTask());
            CompleteTaskCommand = new RelayCommand(_ => CompleteTask());
            
            // Initialize new task
            ResetNewTask();
        }
        
        /// <summary>
        /// Loads all maintenance tasks from the database
        /// </summary>
        public void LoadTasks()
        {
            Tasks.Clear();
            
            var tasks = App.DatabaseManager.GetAllMaintenanceTasks();
            foreach (var task in tasks)
            {
                Tasks.Add(task);
            }
            
            OnPropertyChanged(nameof(FilteredTasks));
        }
        
        /// <summary>
        /// Creates a deep copy of a maintenance task
        /// </summary>
        /// <param name="source">The task to clone</param>
        /// <returns>A new task with the same values</returns>
        private MaintenanceTask CloneTask(MaintenanceTask source)
        {
            return new MaintenanceTask
            {
                Id = source.Id,
                Title = source.Title,
                Description = source.Description,
                Category = source.Category,
                ScheduledDateTime = source.ScheduledDateTime,
                IsRecurring = source.IsRecurring,
                RecurrenceFrequencyDays = source.RecurrenceFrequencyDays,
                LastCompletedDateTime = source.LastCompletedDateTime,
                NotificationsEnabled = source.NotificationsEnabled,
                NotificationId = source.NotificationId
            };
        }
        
        /// <summary>
        /// Shows the dialog for adding a new task
        /// </summary>
        private void AddTask()
        {
            ResetNewTask();
            IsAddingTask = true;
        }
        
        /// <summary>
        /// Saves a new task to the database
        /// </summary>
        private void SaveNewTask()
        {
            int id = App.DatabaseManager.AddMaintenanceTask(NewTask);
            NewTask.Id = id;
            
            Tasks.Add(NewTask);
            SelectedTask = NewTask;
            
            // Set up notification for the task
            if (NewTask.NotificationsEnabled)
            {
                App.NotificationService.ScheduleNotification(NewTask);
            }
            
            OnPropertyChanged(nameof(FilteredTasks));
            
            ResetNewTask();
            IsAddingTask = false;
        }
        
        /// <summary>
        /// Cancels adding a new task
        /// </summary>
        private void CancelAddTask()
        {
            IsAddingTask = false;
        }
        
        /// <summary>
        /// Resets the new task to default values
        /// </summary>
        private void ResetNewTask()
        {
            NewTask = new MaintenanceTask
            {
                ScheduledDateTime = DateTime.Now.AddDays(1),
                NotificationsEnabled = true,
                IsRecurring = false,
                RecurrenceFrequencyDays = 7,
                Category = MaintenanceCategory.WaterChange,
                NotificationId = Guid.NewGuid().ToString("N")
            };
        }
        
        /// <summary>
        /// Enters edit mode for the selected task
        /// </summary>
        private void EditTask()
        {
            if (SelectedTask != null)
            {
                IsEditing = true;
            }
        }
        
        /// <summary>
        /// Saves changes to the selected task
        /// </summary>
        private void SaveTask()
        {
            if (EditableTask != null)
            {
                // Check if notification settings changed
                bool notificationsChanged = 
                    SelectedTask.NotificationsEnabled != EditableTask.NotificationsEnabled ||
                    SelectedTask.ScheduledDateTime != EditableTask.ScheduledDateTime;
                
                App.DatabaseManager.UpdateMaintenanceTask(EditableTask);
                
                // Update the item in the collection
                int index = Tasks.IndexOf(SelectedTask);
                if (index >= 0)
                {
                    Tasks[index] = EditableTask;
                    SelectedTask = EditableTask;
                }
                
                // Update notifications if needed
                if (notificationsChanged)
                {
                    if (EditableTask.NotificationsEnabled)
                    {
                        App.NotificationService.ScheduleNotification(EditableTask);
                    }
                    else
                    {
                        App.NotificationService.CancelNotification(EditableTask);
                    }
                }
                
                IsEditing = false;
                OnPropertyChanged(nameof(FilteredTasks));
                OnPropertyChanged(nameof(ShowCompleteButtonVisibility));
            }
        }
        
        /// <summary>
        /// Cancels editing the selected task
        /// </summary>
        private void CancelEdit()
        {
            if (SelectedTask != null)
            {
                EditableTask = CloneTask(SelectedTask);
            }
            
            IsEditing = false;
        }
        
        /// <summary>
        /// Deletes the selected task
        /// </summary>
        private void DeleteTask()
        {
            if (SelectedTask != null)
            {
                var confirmResult = NotificationHelper.ShowConfirmation(
                    "Delete Task",
                    $"Are you sure you want to delete the task '{SelectedTask.Title}'?");
                    
                if (confirmResult)
                {
                    // Cancel any scheduled notifications
                    App.NotificationService.CancelNotification(SelectedTask);
                    
                    App.DatabaseManager.DeleteMaintenanceTask(SelectedTask.Id);
                    Tasks.Remove(SelectedTask);
                    SelectedTask = null;
                    EditableTask = null;
                    
                    OnPropertyChanged(nameof(FilteredTasks));
                }
            }
        }
        
        /// <summary>
        /// Marks the selected task as completed
        /// </summary>
        private void CompleteTask()
        {
            if (SelectedTask != null)
            {
                SelectedTask.MarkCompleted();
                App.DatabaseManager.UpdateMaintenanceTask(SelectedTask);
                
                // Update notification if task is recurring
                if (SelectedTask.IsRecurring && SelectedTask.NotificationsEnabled)
                {
                    App.NotificationService.ScheduleNotification(SelectedTask);
                }
                else
                {
                    App.NotificationService.CancelNotification(SelectedTask);
                }
                
                // Refresh the view
                EditableTask = CloneTask(SelectedTask);
                OnPropertyChanged(nameof(FilteredTasks));
                OnPropertyChanged(nameof(ShowCompleteButtonVisibility));
                
                // Show confirmation notification
                NotificationHelper.ShowInformation(
                    "Task Completed", 
                    $"Task '{SelectedTask.Title}' has been marked as completed.");
            }
        }
    }
}

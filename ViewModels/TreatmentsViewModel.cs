using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using VoltBettaManager.Helpers;
using VoltBettaManager.Models;

namespace VoltBettaManager.ViewModels
{
    /// <summary>
    /// ViewModel for the Treatments view
    /// </summary>
    public class TreatmentsViewModel : ViewModelBase
    {
        #region Fields
        
        private ObservableCollection<TreatmentPlan> _treatments;
        private TreatmentPlan _selectedTreatment;
        private TreatmentPlan _editableTreatment;
        private TreatmentPlan _newTreatment;
        private string _searchText;
        private TreatmentStatus? _selectedStatus;
        private bool _isEditing;
        private bool _isAddingTreatment;
        private bool _isAddingTreatmentLog;
        private bool _isAddingSymptom;
        private bool _isUpdatingSymptom;
        private bool _isAddingPhoto;
        private bool _isCompletingTreatment;
        private TreatmentLog _newTreatmentLog;
        private string _newSymptomName;
        private SymptomSeverity _newSymptomSeverity;
        private string _symptomNotes;
        private int _selectedSymptomIndex;
        private SymptomSeverity _updatedSymptomSeverity;
        private string _photoCaption;
        private ObservableCollection<FishPhoto> _treatmentPhotos;
        private string _treatmentOutcome;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Collection of all treatment plans
        /// </summary>
        public ObservableCollection<TreatmentPlan> Treatments
        {
            get => _treatments;
            set => SetProperty(ref _treatments, value);
        }
        
        /// <summary>
        /// The currently selected treatment plan
        /// </summary>
        public TreatmentPlan SelectedTreatment
        {
            get => _selectedTreatment;
            set
            {
                if (SetProperty(ref _selectedTreatment, value) && value != null)
                {
                    EditableTreatment = CloneTreatment(value);
                    LoadTreatmentPhotos();
                }
                OnPropertyChanged(nameof(EmptyStateVisibility));
                OnPropertyChanged(nameof(CompleteTreatmentButtonVisibility));
                OnPropertyChanged(nameof(TreatmentLogs));
                OnPropertyChanged(nameof(Symptoms));
            }
        }
        
        /// <summary>
        /// Editable copy of the selected treatment plan
        /// </summary>
        public TreatmentPlan EditableTreatment
        {
            get => _editableTreatment;
            set => SetProperty(ref _editableTreatment, value);
        }
        
        /// <summary>
        /// New treatment plan being created
        /// </summary>
        public TreatmentPlan NewTreatment
        {
            get => _newTreatment;
            set
            {
                SetProperty(ref _newTreatment, value);
                OnPropertyChanged(nameof(CanAddTreatment));
            }
        }
        
        /// <summary>
        /// Photos associated with the selected treatment
        /// </summary>
        public ObservableCollection<FishPhoto> TreatmentPhotos
        {
            get => _treatmentPhotos;
            set => SetProperty(ref _treatmentPhotos, value);
        }
        
        /// <summary>
        /// Search text for filtering treatments
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                OnPropertyChanged(nameof(FilteredTreatments));
            }
        }
        
        /// <summary>
        /// Selected status for filtering treatments
        /// </summary>
        public TreatmentStatus? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                SetProperty(ref _selectedStatus, value);
                OnPropertyChanged(nameof(FilteredTreatments));
            }
        }
        
        /// <summary>
        /// Indicates whether the selected treatment is being edited
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
        /// Indicates whether a new treatment is being added
        /// </summary>
        public bool IsAddingTreatment
        {
            get => _isAddingTreatment;
            set => SetProperty(ref _isAddingTreatment, value);
        }
        
        /// <summary>
        /// Indicates whether a new treatment log is being added
        /// </summary>
        public bool IsAddingTreatmentLog
        {
            get => _isAddingTreatmentLog;
            set => SetProperty(ref _isAddingTreatmentLog, value);
        }
        
        /// <summary>
        /// Indicates whether a new symptom is being added
        /// </summary>
        public bool IsAddingSymptom
        {
            get => _isAddingSymptom;
            set => SetProperty(ref _isAddingSymptom, value);
        }
        
        /// <summary>
        /// Indicates whether a symptom is being updated
        /// </summary>
        public bool IsUpdatingSymptom
        {
            get => _isUpdatingSymptom;
            set => SetProperty(ref _isUpdatingSymptom, value);
        }
        
        /// <summary>
        /// Indicates whether a photo is being added
        /// </summary>
        public bool IsAddingPhoto
        {
            get => _isAddingPhoto;
            set => SetProperty(ref _isAddingPhoto, value);
        }
        
        /// <summary>
        /// Indicates whether a treatment is being completed
        /// </summary>
        public bool IsCompletingTreatment
        {
            get => _isCompletingTreatment;
            set => SetProperty(ref _isCompletingTreatment, value);
        }
        
        /// <summary>
        /// New treatment log being created
        /// </summary>
        public TreatmentLog NewTreatmentLog
        {
            get => _newTreatmentLog;
            set
            {
                SetProperty(ref _newTreatmentLog, value);
                OnPropertyChanged(nameof(CanAddTreatmentLog));
            }
        }
        
        /// <summary>
        /// Name for a new symptom
        /// </summary>
        public string NewSymptomName
        {
            get => _newSymptomName;
            set
            {
                SetProperty(ref _newSymptomName, value);
                OnPropertyChanged(nameof(CanAddSymptom));
            }
        }
        
        /// <summary>
        /// Severity for a new symptom
        /// </summary>
        public SymptomSeverity NewSymptomSeverity
        {
            get => _newSymptomSeverity;
            set => SetProperty(ref _newSymptomSeverity, value);
        }
        
        /// <summary>
        /// Notes for a new symptom or symptom update
        /// </summary>
        public string SymptomNotes
        {
            get => _symptomNotes;
            set => SetProperty(ref _symptomNotes, value);
        }
        
        /// <summary>
        /// Index of the selected symptom in the Symptoms list
        /// </summary>
        public int SelectedSymptomIndex
        {
            get => _selectedSymptomIndex;
            set => SetProperty(ref _selectedSymptomIndex, value);
        }
        
        /// <summary>
        /// Updated severity for a symptom
        /// </summary>
        public SymptomSeverity UpdatedSymptomSeverity
        {
            get => _updatedSymptomSeverity;
            set => SetProperty(ref _updatedSymptomSeverity, value);
        }
        
        /// <summary>
        /// Caption for a new photo
        /// </summary>
        public string PhotoCaption
        {
            get => _photoCaption;
            set => SetProperty(ref _photoCaption, value);
        }
        
        /// <summary>
        /// Outcome notes for completing a treatment
        /// </summary>
        public string TreatmentOutcome
        {
            get => _treatmentOutcome;
            set => SetProperty(ref _treatmentOutcome, value);
        }
        
        /// <summary>
        /// Treatment logs sorted by date
        /// </summary>
        public IEnumerable<TreatmentLog> TreatmentLogs
        {
            get
            {
                if (SelectedTreatment == null)
                    return new List<TreatmentLog>();
                    
                return SelectedTreatment.TreatmentLogs.OrderByDescending(tl => tl.Date);
            }
        }
        
        /// <summary>
        /// Symptoms in the selected treatment
        /// </summary>
        public IEnumerable<Symptom> Symptoms
        {
            get
            {
                if (SelectedTreatment == null)
                    return new List<Symptom>();
                    
                return SelectedTreatment.Symptoms;
            }
        }
        
        /// <summary>
        /// Filtered treatments based on search text and selected status
        /// </summary>
        public IEnumerable<TreatmentPlan> FilteredTreatments
        {
            get
            {
                var filteredTreatments = Treatments.AsEnumerable();
                
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var search = SearchText.ToLower();
                    filteredTreatments = filteredTreatments.Where(t => 
                        t.IllnessName.ToLower().Contains(search) || 
                        t.Description?.ToLower().Contains(search) == true ||
                        t.MedicationNotes?.ToLower().Contains(search) == true);
                }
                
                if (SelectedStatus.HasValue)
                {
                    filteredTreatments = filteredTreatments.Where(t => t.Status == SelectedStatus.Value);
                }
                
                return filteredTreatments.OrderByDescending(t => t.StartDate);
            }
        }
        
        /// <summary>
        /// Options for treatment statuses
        /// </summary>
        public IEnumerable<TreatmentStatus> StatusOptions => Enum.GetValues(typeof(TreatmentStatus)).Cast<TreatmentStatus>();
        
        /// <summary>
        /// Options for symptom severities
        /// </summary>
        public IEnumerable<SymptomSeverity> SeverityOptions => Enum.GetValues(typeof(SymptomSeverity)).Cast<SymptomSeverity>();
        
        /// <summary>
        /// Visibility of the save button
        /// </summary>
        public Visibility SaveButtonVisibility => IsEditing ? Visibility.Visible : Visibility.Collapsed;
        
        /// <summary>
        /// Visibility of the edit button
        /// </summary>
        public Visibility EditButtonVisibility => IsEditing ? Visibility.Collapsed : Visibility.Visible;
        
        /// <summary>
        /// Visibility of the complete treatment button
        /// </summary>
        public Visibility CompleteTreatmentButtonVisibility => (SelectedTreatment != null && SelectedTreatment.Status == TreatmentStatus.Active) ? Visibility.Visible : Visibility.Collapsed;
        
        /// <summary>
        /// Visibility of the empty state message
        /// </summary>
        public Visibility EmptyStateVisibility => SelectedTreatment == null ? Visibility.Visible : Visibility.Collapsed;
        
        /// <summary>
        /// Indicates whether a new treatment can be added (validates required fields)
        /// </summary>
        public bool CanAddTreatment => NewTreatment != null && !string.IsNullOrWhiteSpace(NewTreatment.IllnessName);
        
        /// <summary>
        /// Indicates whether a new treatment log can be added (validates required fields)
        /// </summary>
        public bool CanAddTreatmentLog => NewTreatmentLog != null && !string.IsNullOrWhiteSpace(NewTreatmentLog.Actions);
        
        /// <summary>
        /// Indicates whether a new symptom can be added (validates required fields)
        /// </summary>
        public bool CanAddSymptom => !string.IsNullOrWhiteSpace(NewSymptomName);
        
        #endregion
        
        #region Commands
        
        /// <summary>
        /// Command to add a new treatment plan
        /// </summary>
        public ICommand AddTreatmentCommand { get; }
        
        /// <summary>
        /// Command to save a new treatment plan
        /// </summary>
        public ICommand SaveNewTreatmentCommand { get; }
        
        /// <summary>
        /// Command to cancel adding a treatment plan
        /// </summary>
        public ICommand CancelAddTreatmentCommand { get; }
        
        /// <summary>
        /// Command to edit the selected treatment plan
        /// </summary>
        public ICommand EditTreatmentCommand { get; }
        
        /// <summary>
        /// Command to save changes to the selected treatment plan
        /// </summary>
        public ICommand SaveTreatmentCommand { get; }
        
        /// <summary>
        /// Command to cancel editing the selected treatment plan
        /// </summary>
        public ICommand CancelEditCommand { get; }
        
        /// <summary>
        /// Command to delete the selected treatment plan
        /// </summary>
        public ICommand DeleteTreatmentCommand { get; }
        
        /// <summary>
        /// Command to add a treatment log
        /// </summary>
        public ICommand AddTreatmentLogCommand { get; }
        
        /// <summary>
        /// Command to save a new treatment log
        /// </summary>
        public ICommand SaveTreatmentLogCommand { get; }
        
        /// <summary>
        /// Command to cancel adding a treatment log
        /// </summary>
        public ICommand CancelAddTreatmentLogCommand { get; }
        
        /// <summary>
        /// Command to add a symptom
        /// </summary>
        public ICommand AddSymptomCommand { get; }
        
        /// <summary>
        /// Command to save a new symptom
        /// </summary>
        public ICommand SaveSymptomCommand { get; }
        
        /// <summary>
        /// Command to cancel adding a symptom
        /// </summary>
        public ICommand CancelAddSymptomCommand { get; }
        
        /// <summary>
        /// Command to update a symptom
        /// </summary>
        public ICommand UpdateSymptomCommand { get; }
        
        /// <summary>
        /// Command to save a symptom update
        /// </summary>
        public ICommand SaveSymptomUpdateCommand { get; }
        
        /// <summary>
        /// Command to cancel updating a symptom
        /// </summary>
        public ICommand CancelUpdateSymptomCommand { get; }
        
        /// <summary>
        /// Command to add a photo
        /// </summary>
        public ICommand AddPhotoCommand { get; }
        
        /// <summary>
        /// Command to save a new photo
        /// </summary>
        public ICommand SavePhotoCommand { get; }
        
        /// <summary>
        /// Command to cancel adding a photo
        /// </summary>
        public ICommand CancelAddPhotoCommand { get; }
        
        /// <summary>
        /// Command to delete a photo
        /// </summary>
        public ICommand DeletePhotoCommand { get; }
        
        /// <summary>
        /// Command to complete a treatment
        /// </summary>
        public ICommand CompleteTreatmentCommand { get; }
        
        /// <summary>
        /// Command to save treatment completion
        /// </summary>
        public ICommand SaveCompleteTreatmentCommand { get; }
        
        /// <summary>
        /// Command to cancel completing a treatment
        /// </summary>
        public ICommand CancelCompleteTreatmentCommand { get; }
        
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the TreatmentsViewModel class
        /// </summary>
        public TreatmentsViewModel()
        {
            // Initialize collections
            Treatments = new ObservableCollection<TreatmentPlan>();
            TreatmentPhotos = new ObservableCollection<FishPhoto>();
            
            // Load initial data
            LoadTreatments();
            
            // Initialize commands
            AddTreatmentCommand = new RelayCommand(_ => AddTreatment());
            SaveNewTreatmentCommand = new RelayCommand(_ => SaveNewTreatment(), _ => CanAddTreatment);
            CancelAddTreatmentCommand = new RelayCommand(_ => CancelAddTreatment());
            EditTreatmentCommand = new RelayCommand(_ => EditTreatment());
            SaveTreatmentCommand = new RelayCommand(_ => SaveTreatment());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            DeleteTreatmentCommand = new RelayCommand(_ => DeleteTreatment());
            
            AddTreatmentLogCommand = new RelayCommand(_ => AddTreatmentLog());
            SaveTreatmentLogCommand = new RelayCommand(_ => SaveTreatmentLog(), _ => CanAddTreatmentLog);
            CancelAddTreatmentLogCommand = new RelayCommand(_ => CancelAddTreatmentLog());
            
            AddSymptomCommand = new RelayCommand(_ => AddSymptom());
            SaveSymptomCommand = new RelayCommand(_ => SaveSymptom(), _ => CanAddSymptom);
            CancelAddSymptomCommand = new RelayCommand(_ => CancelAddSymptom());
            
            UpdateSymptomCommand = new RelayCommand(param => UpdateSymptom(param));
            SaveSymptomUpdateCommand = new RelayCommand(_ => SaveSymptomUpdate());
            CancelUpdateSymptomCommand = new RelayCommand(_ => CancelUpdateSymptom());
            
            AddPhotoCommand = new RelayCommand(_ => AddPhoto());
            SavePhotoCommand = new RelayCommand(_ => SavePhoto());
            CancelAddPhotoCommand = new RelayCommand(_ => CancelAddPhoto());
            DeletePhotoCommand = new RelayCommand(param => DeletePhoto(param));
            
            CompleteTreatmentCommand = new RelayCommand(_ => CompleteTreatment());
            SaveCompleteTreatmentCommand = new RelayCommand(_ => SaveCompleteTreatment());
            CancelCompleteTreatmentCommand = new RelayCommand(_ => CancelCompleteTreatment());
            
            // Initialize new treatment
            ResetNewTreatment();
            
            // Initialize new log
            ResetNewTreatmentLog();
        }
        
        /// <summary>
        /// Loads all treatments from the database
        /// </summary>
        public void LoadTreatments()
        {
            Treatments.Clear();
            
            var treatments = App.DatabaseManager.GetAllTreatmentPlans();
            foreach (var treatment in treatments)
            {
                Treatments.Add(treatment);
            }
            
            OnPropertyChanged(nameof(FilteredTreatments));
        }
        
        /// <summary>
        /// Loads photos associated with the selected treatment
        /// </summary>
        private void LoadTreatmentPhotos()
        {
            TreatmentPhotos.Clear();
            
            if (SelectedTreatment != null)
            {
                var photos = App.DatabaseManager.GetPhotosByTreatmentPlanId(SelectedTreatment.Id);
                foreach (var photo in photos)
                {
                    TreatmentPhotos.Add(photo);
                }
            }
        }
        
        /// <summary>
        /// Creates a deep copy of a treatment plan
        /// </summary>
        /// <param name="source">The treatment plan to clone</param>
        /// <returns>A new treatment plan with the same values</returns>
        private TreatmentPlan CloneTreatment(TreatmentPlan source)
        {
            var clone = new TreatmentPlan
            {
                Id = source.Id,
                IllnessName = source.IllnessName,
                Description = source.Description,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                Status = source.Status,
                MedicationNotes = source.MedicationNotes,
                TreatmentLogs = new List<TreatmentLog>(),
                Symptoms = new List<Symptom>(),
                ProgressPhotoIds = new List<int>(source.ProgressPhotoIds)
            };
            
            // Clone the treatment logs
            foreach (var log in source.TreatmentLogs)
            {
                clone.TreatmentLogs.Add(new TreatmentLog
                {
                    Date = log.Date,
                    Actions = log.Actions,
                    Notes = log.Notes
                });
            }
            
            // Clone the symptoms and their history
            foreach (var symptom in source.Symptoms)
            {
                var clonedSymptom = new Symptom
                {
                    Name = symptom.Name,
                    Severity = symptom.Severity,
                    History = new List<SymptomHistory>()
                };
                
                foreach (var history in symptom.History)
                {
                    clonedSymptom.History.Add(new SymptomHistory
                    {
                        Date = history.Date,
                        Severity = history.Severity,
                        Notes = history.Notes
                    });
                }
                
                clone.Symptoms.Add(clonedSymptom);
            }
            
            return clone;
        }
        
        /// <summary>
        /// Shows the dialog for adding a new treatment plan
        /// </summary>
        private void AddTreatment()
        {
            ResetNewTreatment();
            IsAddingTreatment = true;
        }
        
        /// <summary>
        /// Saves a new treatment plan to the database
        /// </summary>
        private void SaveNewTreatment()
        {
            int id = App.DatabaseManager.AddTreatmentPlan(NewTreatment);
            NewTreatment.Id = id;
            
            Treatments.Add(NewTreatment);
            SelectedTreatment = NewTreatment;
            
            OnPropertyChanged(nameof(FilteredTreatments));
            
            ResetNewTreatment();
            IsAddingTreatment = false;
        }
        
        /// <summary>
        /// Cancels adding a new treatment plan
        /// </summary>
        private void CancelAddTreatment()
        {
            IsAddingTreatment = false;
        }
        
        /// <summary>
        /// Resets the new treatment plan to default values
        /// </summary>
        private void ResetNewTreatment()
        {
            NewTreatment = new TreatmentPlan
            {
                StartDate = DateTime.Now,
                Status = TreatmentStatus.Active,
                TreatmentLogs = new List<TreatmentLog>(),
                Symptoms = new List<Symptom>(),
                ProgressPhotoIds = new List<int>()
            };
        }
        
        /// <summary>
        /// Enters edit mode for the selected treatment plan
        /// </summary>
        private void EditTreatment()
        {
            if (SelectedTreatment != null)
            {
                IsEditing = true;
            }
        }
        
        /// <summary>
        /// Saves changes to the selected treatment plan
        /// </summary>
        private void SaveTreatment()
        {
            if (EditableTreatment != null)
            {
                App.DatabaseManager.UpdateTreatmentPlan(EditableTreatment);
                
                // Update the item in the collection
                int index = Treatments.IndexOf(SelectedTreatment);
                if (index >= 0)
                {
                    Treatments[index] = EditableTreatment;
                    SelectedTreatment = EditableTreatment;
                }
                
                IsEditing = false;
                OnPropertyChanged(nameof(FilteredTreatments));
            }
        }
        
        /// <summary>
        /// Cancels editing the selected treatment plan
        /// </summary>
        private void CancelEdit()
        {
            if (SelectedTreatment != null)
            {
                EditableTreatment = CloneTreatment(SelectedTreatment);
            }
            
            IsEditing = false;
        }
        
        /// <summary>
        /// Deletes the selected treatment plan
        /// </summary>
        private void DeleteTreatment()
        {
            if (SelectedTreatment != null)
            {
                var confirmResult = NotificationHelper.ShowConfirmation(
                    "Delete Treatment Plan",
                    $"Are you sure you want to delete the treatment plan for '{SelectedTreatment.IllnessName}'?");
                    
                if (confirmResult)
                {
                    App.DatabaseManager.DeleteTreatmentPlan(SelectedTreatment.Id);
                    Treatments.Remove(SelectedTreatment);
                    SelectedTreatment = null;
                    EditableTreatment = null;
                    
                    OnPropertyChanged(nameof(FilteredTreatments));
                }
            }
        }
        
        /// <summary>
        /// Shows the dialog for adding a treatment log
        /// </summary>
        private void AddTreatmentLog()
        {
            if (SelectedTreatment != null)
            {
                ResetNewTreatmentLog();
                IsAddingTreatmentLog = true;
            }
        }
        
        /// <summary>
        /// Saves a new treatment log to the selected treatment plan
        /// </summary>
        private void SaveTreatmentLog()
        {
            if (SelectedTreatment != null && NewTreatmentLog != null)
            {
                SelectedTreatment.TreatmentLogs.Add(NewTreatmentLog);
                App.DatabaseManager.UpdateTreatmentPlan(SelectedTreatment);
                
                // Update EditableTreatment to reflect changes
                EditableTreatment = CloneTreatment(SelectedTreatment);
                
                OnPropertyChanged(nameof(TreatmentLogs));
                
                ResetNewTreatmentLog();
                IsAddingTreatmentLog = false;
            }
        }
        
        /// <summary>
        /// Cancels adding a treatment log
        /// </summary>
        private void CancelAddTreatmentLog()
        {
            IsAddingTreatmentLog = false;
        }
        
        /// <summary>
        /// Resets the new treatment log to default values
        /// </summary>
        private void ResetNewTreatmentLog()
        {
            NewTreatmentLog = new TreatmentLog
            {
                Date = DateTime.Now
            };
        }
        
        /// <summary>
        /// Shows the dialog for adding a symptom
        /// </summary>
        private void AddSymptom()
        {
            if (SelectedTreatment != null)
            {
                NewSymptomName = string.Empty;
                NewSymptomSeverity = SymptomSeverity.Mild;
                SymptomNotes = string.Empty;
                IsAddingSymptom = true;
            }
        }
        
        /// <summary>
        /// Saves a new symptom to the selected treatment plan
        /// </summary>
        private void SaveSymptom()
        {
            if (SelectedTreatment != null && !string.IsNullOrWhiteSpace(NewSymptomName))
            {
                SelectedTreatment.AddSymptom(NewSymptomName, NewSymptomSeverity);
                App.DatabaseManager.UpdateTreatmentPlan(SelectedTreatment);
                
                // Update EditableTreatment to reflect changes
                EditableTreatment = CloneTreatment(SelectedTreatment);
                
                OnPropertyChanged(nameof(Symptoms));
                
                IsAddingSymptom = false;
            }
        }
        
        /// <summary>
        /// Cancels adding a symptom
        /// </summary>
        private void CancelAddSymptom()
        {
            IsAddingSymptom = false;
        }
        
        /// <summary>
        /// Shows the dialog for updating a symptom
        /// </summary>
        /// <param name="parameter">Index of the symptom to update</param>
        private void UpdateSymptom(object parameter)
        {
            if (SelectedTreatment != null && parameter is int symptomIndex)
            {
                if (symptomIndex >= 0 && symptomIndex < SelectedTreatment.Symptoms.Count)
                {
                    SelectedSymptomIndex = symptomIndex;
                    UpdatedSymptomSeverity = SelectedTreatment.Symptoms[symptomIndex].Severity;
                    SymptomNotes = string.Empty;
                    IsUpdatingSymptom = true;
                }
            }
        }
        
        /// <summary>
        /// Saves a symptom update to the selected treatment plan
        /// </summary>
        private void SaveSymptomUpdate()
        {
            if (SelectedTreatment != null)
            {
                SelectedTreatment.UpdateSymptomSeverity(SelectedSymptomIndex, UpdatedSymptomSeverity, SymptomNotes);
                App.DatabaseManager.UpdateTreatmentPlan(SelectedTreatment);
                
                // Update EditableTreatment to reflect changes
                EditableTreatment = CloneTreatment(SelectedTreatment);
                
                OnPropertyChanged(nameof(Symptoms));
                
                IsUpdatingSymptom = false;
            }
        }
        
        /// <summary>
        /// Cancels updating a symptom
        /// </summary>
        private void CancelUpdateSymptom()
        {
            IsUpdatingSymptom = false;
        }
        
        /// <summary>
        /// Shows the dialog for adding a photo
        /// </summary>
        private void AddPhoto()
        {
            if (SelectedTreatment != null)
            {
                PhotoCaption = string.Empty;
                IsAddingPhoto = true;
            }
        }
        
        /// <summary>
        /// Saves a new photo to the selected treatment plan
        /// </summary>
        private void SavePhoto()
        {
            if (SelectedTreatment != null)
            {
                var photo = ImageHelper.SelectAndSaveImage(
                    PhotoCaption, 
                    "Treatment", 
                    true, 
                    SelectedTreatment.Id);
                
                if (photo != null)
                {
                    SelectedTreatment.ProgressPhotoIds.Add(photo.Id);
                    App.DatabaseManager.UpdateTreatmentPlan(SelectedTreatment);
                    
                    // Update EditableTreatment to reflect changes
                    EditableTreatment = CloneTreatment(SelectedTreatment);
                    
                    // Refresh photos
                    TreatmentPhotos.Add(photo);
                }
                
                IsAddingPhoto = false;
            }
        }
        
        /// <summary>
        /// Cancels adding a photo
        /// </summary>
        private void CancelAddPhoto()
        {
            IsAddingPhoto = false;
        }
        
        /// <summary>
        /// Deletes a photo from the selected treatment plan
        /// </summary>
        /// <param name="parameter">The photo to delete</param>
        private void DeletePhoto(object parameter)
        {
            if (SelectedTreatment != null && parameter is FishPhoto photo)
            {
                var confirmResult = NotificationHelper.ShowConfirmation(
                    "Delete Photo", 
                    "Are you sure you want to delete this photo?");
                    
                if (confirmResult)
                {
                    // Remove from treatment plan
                    SelectedTreatment.ProgressPhotoIds.Remove(photo.Id);
                    App.DatabaseManager.UpdateTreatmentPlan(SelectedTreatment);
                    
                    // Delete the photo
                    ImageHelper.DeleteImage(photo);
                    
                    // Update EditableTreatment to reflect changes
                    EditableTreatment = CloneTreatment(SelectedTreatment);
                    
                    // Remove from collection
                    TreatmentPhotos.Remove(photo);
                }
            }
        }
        
        /// <summary>
        /// Shows the dialog for completing a treatment
        /// </summary>
        private void CompleteTreatment()
        {
            if (SelectedTreatment != null && SelectedTreatment.Status == TreatmentStatus.Active)
            {
                TreatmentOutcome = string.Empty;
                IsCompletingTreatment = true;
            }
        }
        
        /// <summary>
        /// Saves treatment completion to the selected treatment plan
        /// </summary>
        private void SaveCompleteTreatment()
        {
            if (SelectedTreatment != null)
            {
                SelectedTreatment.CompleteTreatment(TreatmentOutcome);
                App.DatabaseManager.UpdateTreatmentPlan(SelectedTreatment);
                
                // Update EditableTreatment to reflect changes
                EditableTreatment = CloneTreatment(SelectedTreatment);
                
                OnPropertyChanged(nameof(FilteredTreatments));
                OnPropertyChanged(nameof(TreatmentLogs));
                OnPropertyChanged(nameof(CompleteTreatmentButtonVisibility));
                
                IsCompletingTreatment = false;
                
                // Show completion notification
                NotificationHelper.ShowInformation(
                    "Treatment Completed", 
                    $"Treatment for '{SelectedTreatment.IllnessName}' has been marked as completed.");
            }
        }
        
        /// <summary>
        /// Cancels completing a treatment
        /// </summary>
        private void CancelCompleteTreatment()
        {
            IsCompletingTreatment = false;
        }
    }
}

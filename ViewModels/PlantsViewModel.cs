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
    /// ViewModel for the Plants view
    /// </summary>
    public class PlantsViewModel : ViewModelBase
    {
        #region Fields
        
        private ObservableCollection<Plant> _plants;
        private Plant _selectedPlant;
        private Plant _editablePlant;
        private Plant _newPlant;
        private string _searchText;
        private PlantLocation? _selectedLocation;
        private bool _isEditing;
        private bool _isAddingPlant;
        private bool _isAddingIssue;
        private bool _isResolvingIssue;
        private PlantIssue _newIssue;
        private PlantIssue _selectedIssue;
        private int _selectedIssueIndex;
        private string _resolutionNotes;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Collection of all plants
        /// </summary>
        public ObservableCollection<Plant> Plants
        {
            get => _plants;
            set => SetProperty(ref _plants, value);
        }
        
        /// <summary>
        /// The currently selected plant
        /// </summary>
        public Plant SelectedPlant
        {
            get => _selectedPlant;
            set
            {
                if (SetProperty(ref _selectedPlant, value) && value != null)
                {
                    EditablePlant = ClonePlant(value);
                }
                OnPropertyChanged(nameof(EmptyStateVisibility));
                OnPropertyChanged(nameof(HasIssues));
                OnPropertyChanged(nameof(HasUnresolvedIssues));
                OnPropertyChanged(nameof(PlantIssues));
                OnPropertyChanged(nameof(UnresolvedIssues));
            }
        }
        
        /// <summary>
        /// Editable copy of the selected plant
        /// </summary>
        public Plant EditablePlant
        {
            get => _editablePlant;
            set => SetProperty(ref _editablePlant, value);
        }
        
        /// <summary>
        /// New plant being created
        /// </summary>
        public Plant NewPlant
        {
            get => _newPlant;
            set
            {
                SetProperty(ref _newPlant, value);
                OnPropertyChanged(nameof(CanAddPlant));
            }
        }
        
        /// <summary>
        /// Search text for filtering plants
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                OnPropertyChanged(nameof(FilteredPlants));
            }
        }
        
        /// <summary>
        /// Selected location for filtering plants
        /// </summary>
        public PlantLocation? SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                SetProperty(ref _selectedLocation, value);
                OnPropertyChanged(nameof(FilteredPlants));
            }
        }
        
        /// <summary>
        /// Indicates whether the selected plant is being edited
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
        /// Indicates whether a new plant is being added
        /// </summary>
        public bool IsAddingPlant
        {
            get => _isAddingPlant;
            set => SetProperty(ref _isAddingPlant, value);
        }
        
        /// <summary>
        /// Indicates whether a new issue is being added
        /// </summary>
        public bool IsAddingIssue
        {
            get => _isAddingIssue;
            set => SetProperty(ref _isAddingIssue, value);
        }
        
        /// <summary>
        /// Indicates whether an issue is being resolved
        /// </summary>
        public bool IsResolvingIssue
        {
            get => _isResolvingIssue;
            set => SetProperty(ref _isResolvingIssue, value);
        }
        
        /// <summary>
        /// New issue being created
        /// </summary>
        public PlantIssue NewIssue
        {
            get => _newIssue;
            set
            {
                SetProperty(ref _newIssue, value);
                OnPropertyChanged(nameof(CanAddIssue));
            }
        }
        
        /// <summary>
        /// The currently selected issue
        /// </summary>
        public PlantIssue SelectedIssue
        {
            get => _selectedIssue;
            set => SetProperty(ref _selectedIssue, value);
        }
        
        /// <summary>
        /// Index of the selected issue in the Issues list
        /// </summary>
        public int SelectedIssueIndex
        {
            get => _selectedIssueIndex;
            set => SetProperty(ref _selectedIssueIndex, value);
        }
        
        /// <summary>
        /// Resolution notes for resolving an issue
        /// </summary>
        public string ResolutionNotes
        {
            get => _resolutionNotes;
            set => SetProperty(ref _resolutionNotes, value);
        }
        
        /// <summary>
        /// Filtered plants based on search text and selected location
        /// </summary>
        public IEnumerable<Plant> FilteredPlants
        {
            get
            {
                var filteredPlants = Plants.AsEnumerable();
                
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var search = SearchText.ToLower();
                    filteredPlants = filteredPlants.Where(p => 
                        p.Name.ToLower().Contains(search) || 
                        p.ScientificName?.ToLower().Contains(search) == true || 
                        p.CareNotes?.ToLower().Contains(search) == true);
                }
                
                if (SelectedLocation.HasValue)
                {
                    filteredPlants = filteredPlants.Where(p => p.Location == SelectedLocation.Value);
                }
                
                return filteredPlants.OrderBy(p => p.Name);
            }
        }
        
        /// <summary>
        /// All issues for the selected plant
        /// </summary>
        public IEnumerable<PlantIssue> PlantIssues
        {
            get
            {
                if (SelectedPlant == null)
                    return new List<PlantIssue>();
                    
                return SelectedPlant.Issues.OrderByDescending(i => i.DateIdentified);
            }
        }
        
        /// <summary>
        /// Unresolved issues for the selected plant
        /// </summary>
        public IEnumerable<PlantIssue> UnresolvedIssues
        {
            get
            {
                if (SelectedPlant == null)
                    return new List<PlantIssue>();
                    
                return SelectedPlant.Issues.Where(i => !i.IsResolved)
                                         .OrderByDescending(i => i.DateIdentified);
            }
        }
        
        /// <summary>
        /// Indicates whether the selected plant has any issues
        /// </summary>
        public bool HasIssues => SelectedPlant != null && SelectedPlant.Issues.Count > 0;
        
        /// <summary>
        /// Indicates whether the selected plant has any unresolved issues
        /// </summary>
        public bool HasUnresolvedIssues => SelectedPlant != null && SelectedPlant.Issues.Any(i => !i.IsResolved);
        
        /// <summary>
        /// Options for plant locations
        /// </summary>
        public IEnumerable<PlantLocation> LocationOptions => Enum.GetValues(typeof(PlantLocation)).Cast<PlantLocation>();
        
        /// <summary>
        /// Options for light levels
        /// </summary>
        public IEnumerable<LightLevel> LightLevelOptions => Enum.GetValues(typeof(LightLevel)).Cast<LightLevel>();
        
        /// <summary>
        /// Options for issue severity
        /// </summary>
        public IEnumerable<IssueSeverity> SeverityOptions => Enum.GetValues(typeof(IssueSeverity)).Cast<IssueSeverity>();
        
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
        public Visibility EmptyStateVisibility => SelectedPlant == null ? Visibility.Visible : Visibility.Collapsed;
        
        /// <summary>
        /// Indicates whether a new plant can be added (validates required fields)
        /// </summary>
        public bool CanAddPlant => NewPlant != null && !string.IsNullOrWhiteSpace(NewPlant.Name);
        
        /// <summary>
        /// Indicates whether a new issue can be added (validates required fields)
        /// </summary>
        public bool CanAddIssue => NewIssue != null && !string.IsNullOrWhiteSpace(NewIssue.Description);
        
        #endregion
        
        #region Commands
        
        /// <summary>
        /// Command to add a new plant
        /// </summary>
        public ICommand AddPlantCommand { get; }
        
        /// <summary>
        /// Command to save a new plant
        /// </summary>
        public ICommand SaveNewPlantCommand { get; }
        
        /// <summary>
        /// Command to cancel adding a plant
        /// </summary>
        public ICommand CancelAddPlantCommand { get; }
        
        /// <summary>
        /// Command to edit the selected plant
        /// </summary>
        public ICommand EditPlantCommand { get; }
        
        /// <summary>
        /// Command to save changes to the selected plant
        /// </summary>
        public ICommand SavePlantCommand { get; }
        
        /// <summary>
        /// Command to cancel editing the selected plant
        /// </summary>
        public ICommand CancelEditCommand { get; }
        
        /// <summary>
        /// Command to delete the selected plant
        /// </summary>
        public ICommand DeletePlantCommand { get; }
        
        /// <summary>
        /// Command to add a new issue to the selected plant
        /// </summary>
        public ICommand AddIssueCommand { get; }
        
        /// <summary>
        /// Command to save a new issue
        /// </summary>
        public ICommand SaveNewIssueCommand { get; }
        
        /// <summary>
        /// Command to cancel adding an issue
        /// </summary>
        public ICommand CancelAddIssueCommand { get; }
        
        /// <summary>
        /// Command to resolve an issue
        /// </summary>
        public ICommand ResolveIssueCommand { get; }
        
        /// <summary>
        /// Command to save issue resolution
        /// </summary>
        public ICommand SaveIssueResolutionCommand { get; }
        
        /// <summary>
        /// Command to cancel resolving an issue
        /// </summary>
        public ICommand CancelResolveIssueCommand { get; }
        
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the PlantsViewModel class
        /// </summary>
        public PlantsViewModel()
        {
            // Initialize collections
            Plants = new ObservableCollection<Plant>();
            
            // Load initial data
            LoadPlants();
            
            // Initialize commands
            AddPlantCommand = new RelayCommand(_ => AddPlant());
            SaveNewPlantCommand = new RelayCommand(_ => SaveNewPlant(), _ => CanAddPlant);
            CancelAddPlantCommand = new RelayCommand(_ => CancelAddPlant());
            EditPlantCommand = new RelayCommand(_ => EditPlant());
            SavePlantCommand = new RelayCommand(_ => SavePlant());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            DeletePlantCommand = new RelayCommand(_ => DeletePlant());
            
            AddIssueCommand = new RelayCommand(_ => AddIssue());
            SaveNewIssueCommand = new RelayCommand(_ => SaveNewIssue(), _ => CanAddIssue);
            CancelAddIssueCommand = new RelayCommand(_ => CancelAddIssue());
            ResolveIssueCommand = new RelayCommand(param => ResolveIssue(param));
            SaveIssueResolutionCommand = new RelayCommand(_ => SaveIssueResolution());
            CancelResolveIssueCommand = new RelayCommand(_ => CancelResolveIssue());
            
            // Initialize new plant
            ResetNewPlant();
            
            // Initialize new issue
            ResetNewIssue();
        }
        
        /// <summary>
        /// Loads all plants from the database
        /// </summary>
        public void LoadPlants()
        {
            Plants.Clear();
            
            var plants = App.DatabaseManager.GetAllPlants();
            foreach (var plant in plants)
            {
                Plants.Add(plant);
            }
            
            OnPropertyChanged(nameof(FilteredPlants));
        }
        
        /// <summary>
        /// Creates a deep copy of a plant
        /// </summary>
        /// <param name="source">The plant to clone</param>
        /// <returns>A new plant with the same values</returns>
        private Plant ClonePlant(Plant source)
        {
            var clone = new Plant
            {
                Id = source.Id,
                Name = source.Name,
                ScientificName = source.ScientificName,
                AddedDate = source.AddedDate,
                CareNotes = source.CareNotes,
                Location = source.Location,
                LightRequirement = source.LightRequirement,
                Issues = new List<PlantIssue>()
            };
            
            // Clone the issues list
            foreach (var issue in source.Issues)
            {
                clone.Issues.Add(new PlantIssue
                {
                    Description = issue.Description,
                    Severity = issue.Severity,
                    DateIdentified = issue.DateIdentified,
                    IsResolved = issue.IsResolved,
                    ResolutionDate = issue.ResolutionDate,
                    ResolutionNotes = issue.ResolutionNotes
                });
            }
            
            return clone;
        }
        
        /// <summary>
        /// Shows the dialog for adding a new plant
        /// </summary>
        private void AddPlant()
        {
            ResetNewPlant();
            IsAddingPlant = true;
        }
        
        /// <summary>
        /// Saves a new plant to the database
        /// </summary>
        private void SaveNewPlant()
        {
            int id = App.DatabaseManager.AddPlant(NewPlant);
            NewPlant.Id = id;
            
            Plants.Add(NewPlant);
            SelectedPlant = NewPlant;
            
            OnPropertyChanged(nameof(FilteredPlants));
            
            ResetNewPlant();
            IsAddingPlant = false;
        }
        
        /// <summary>
        /// Cancels adding a new plant
        /// </summary>
        private void CancelAddPlant()
        {
            IsAddingPlant = false;
        }
        
        /// <summary>
        /// Resets the new plant to default values
        /// </summary>
        private void ResetNewPlant()
        {
            NewPlant = new Plant
            {
                AddedDate = DateTime.Now,
                Location = PlantLocation.InTank,
                LightRequirement = LightLevel.Medium,
                Issues = new List<PlantIssue>()
            };
        }
        
        /// <summary>
        /// Enters edit mode for the selected plant
        /// </summary>
        private void EditPlant()
        {
            if (SelectedPlant != null)
            {
                IsEditing = true;
            }
        }
        
        /// <summary>
        /// Saves changes to the selected plant
        /// </summary>
        private void SavePlant()
        {
            if (EditablePlant != null)
            {
                App.DatabaseManager.UpdatePlant(EditablePlant);
                
                // Update the item in the collection
                int index = Plants.IndexOf(SelectedPlant);
                if (index >= 0)
                {
                    Plants[index] = EditablePlant;
                    SelectedPlant = EditablePlant;
                }
                
                IsEditing = false;
                OnPropertyChanged(nameof(FilteredPlants));
            }
        }
        
        /// <summary>
        /// Cancels editing the selected plant
        /// </summary>
        private void CancelEdit()
        {
            if (SelectedPlant != null)
            {
                EditablePlant = ClonePlant(SelectedPlant);
            }
            
            IsEditing = false;
        }
        
        /// <summary>
        /// Deletes the selected plant
        /// </summary>
        private void DeletePlant()
        {
            if (SelectedPlant != null)
            {
                var confirmResult = NotificationHelper.ShowConfirmation(
                    "Delete Plant",
                    $"Are you sure you want to delete the plant '{SelectedPlant.Name}'?");
                    
                if (confirmResult)
                {
                    App.DatabaseManager.DeletePlant(SelectedPlant.Id);
                    Plants.Remove(SelectedPlant);
                    SelectedPlant = null;
                    EditablePlant = null;
                    
                    OnPropertyChanged(nameof(FilteredPlants));
                }
            }
        }
        
        /// <summary>
        /// Shows the dialog for adding a new issue to the selected plant
        /// </summary>
        private void AddIssue()
        {
            if (SelectedPlant != null)
            {
                ResetNewIssue();
                IsAddingIssue = true;
            }
        }
        
        /// <summary>
        /// Saves a new issue to the selected plant
        /// </summary>
        private void SaveNewIssue()
        {
            if (SelectedPlant != null && NewIssue != null)
            {
                SelectedPlant.Issues.Add(NewIssue);
                App.DatabaseManager.UpdatePlant(SelectedPlant);
                
                // Update EditablePlant to reflect changes
                EditablePlant = ClonePlant(SelectedPlant);
                
                OnPropertyChanged(nameof(PlantIssues));
                OnPropertyChanged(nameof(UnresolvedIssues));
                OnPropertyChanged(nameof(HasIssues));
                OnPropertyChanged(nameof(HasUnresolvedIssues));
                
                ResetNewIssue();
                IsAddingIssue = false;
                
                // Show warning for severe issues
                if (NewIssue.Severity == IssueSeverity.Severe)
                {
                    NotificationHelper.ShowWarning(
                        "Severe Plant Issue", 
                        $"A severe issue has been added to {SelectedPlant.Name}: {NewIssue.Description}");
                }
            }
        }
        
        /// <summary>
        /// Cancels adding a new issue
        /// </summary>
        private void CancelAddIssue()
        {
            IsAddingIssue = false;
        }
        
        /// <summary>
        /// Resets the new issue to default values
        /// </summary>
        private void ResetNewIssue()
        {
            NewIssue = new PlantIssue
            {
                DateIdentified = DateTime.Now,
                IsResolved = false,
                Severity = IssueSeverity.Minor
            };
        }
        
        /// <summary>
        /// Shows the dialog for resolving an issue
        /// </summary>
        /// <param name="parameter">Index of the issue to resolve</param>
        private void ResolveIssue(object parameter)
        {
            if (SelectedPlant != null && parameter is int issueIndex)
            {
                if (issueIndex >= 0 && issueIndex < SelectedPlant.Issues.Count)
                {
                    SelectedIssueIndex = issueIndex;
                    SelectedIssue = SelectedPlant.Issues[issueIndex];
                    ResolutionNotes = string.Empty;
                    IsResolvingIssue = true;
                }
            }
        }
        
        /// <summary>
        /// Saves the resolution for the selected issue
        /// </summary>
        private void SaveIssueResolution()
        {
            if (SelectedPlant != null && SelectedIssue != null)
            {
                SelectedPlant.ResolveIssue(SelectedIssueIndex, DateTime.Now, ResolutionNotes);
                App.DatabaseManager.UpdatePlant(SelectedPlant);
                
                // Update EditablePlant to reflect changes
                EditablePlant = ClonePlant(SelectedPlant);
                
                OnPropertyChanged(nameof(PlantIssues));
                OnPropertyChanged(nameof(UnresolvedIssues));
                OnPropertyChanged(nameof(HasUnresolvedIssues));
                
                IsResolvingIssue = false;
            }
        }
        
        /// <summary>
        /// Cancels resolving the selected issue
        /// </summary>
        private void CancelResolveIssue()
        {
            IsResolvingIssue = false;
        }
    }
}

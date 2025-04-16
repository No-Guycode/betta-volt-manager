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
    /// ViewModel for the Gallery view
    /// </summary>
    public class GalleryViewModel : ViewModelBase
    {
        #region Fields
        
        private ObservableCollection<FishPhoto> _photos;
        private FishPhoto _selectedPhoto;
        private FishPhoto _editablePhoto;
        private string _searchText;
        private string _selectedCategory;
        private bool _isEditing;
        private bool _isAddingPhoto;
        private bool _isViewingPhoto;
        private BitmapImage _currentPhotoImage;
        private BitmapImage _selectedThumbnail;
        private FishPhoto _newPhoto;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Collection of all photos
        /// </summary>
        public ObservableCollection<FishPhoto> Photos
        {
            get => _photos;
            set => SetProperty(ref _photos, value);
        }
        
        /// <summary>
        /// The currently selected photo
        /// </summary>
        public FishPhoto SelectedPhoto
        {
            get => _selectedPhoto;
            set
            {
                if (SetProperty(ref _selectedPhoto, value) && value != null)
                {
                    EditablePhoto = ClonePhoto(value);
                    SelectedThumbnail = ImageHelper.LoadResizedImage(value, 200, 200);
                }
                OnPropertyChanged(nameof(EmptyStateVisibility));
            }
        }
        
        /// <summary>
        /// Editable copy of the selected photo
        /// </summary>
        public FishPhoto EditablePhoto
        {
            get => _editablePhoto;
            set => SetProperty(ref _editablePhoto, value);
        }
        
        /// <summary>
        /// New photo being created
        /// </summary>
        public FishPhoto NewPhoto
        {
            get => _newPhoto;
            set => SetProperty(ref _newPhoto, value);
        }
        
        /// <summary>
        /// Search text for filtering photos
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                OnPropertyChanged(nameof(FilteredPhotos));
            }
        }
        
        /// <summary>
        /// Selected category for filtering photos
        /// </summary>
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                OnPropertyChanged(nameof(FilteredPhotos));
            }
        }
        
        /// <summary>
        /// Indicates whether the selected photo is being edited
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
        /// Indicates whether a new photo is being added
        /// </summary>
        public bool IsAddingPhoto
        {
            get => _isAddingPhoto;
            set => SetProperty(ref _isAddingPhoto, value);
        }
        
        /// <summary>
        /// Indicates whether a photo is being viewed in detail
        /// </summary>
        public bool IsViewingPhoto
        {
            get => _isViewingPhoto;
            set => SetProperty(ref _isViewingPhoto, value);
        }
        
        /// <summary>
        /// The current photo image being viewed
        /// </summary>
        public BitmapImage CurrentPhotoImage
        {
            get => _currentPhotoImage;
            set => SetProperty(ref _currentPhotoImage, value);
        }
        
        /// <summary>
        /// Thumbnail of the selected photo
        /// </summary>
        public BitmapImage SelectedThumbnail
        {
            get => _selectedThumbnail;
            set => SetProperty(ref _selectedThumbnail, value);
        }
        
        /// <summary>
        /// Filtered photos based on search text and selected category
        /// </summary>
        public IEnumerable<FishPhoto> FilteredPhotos
        {
            get
            {
                var filteredPhotos = Photos.AsEnumerable();
                
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var search = SearchText.ToLower();
                    filteredPhotos = filteredPhotos.Where(p => 
                        p.Caption?.ToLower().Contains(search) == true || 
                        p.Category?.ToLower().Contains(search) == true);
                }
                
                if (!string.IsNullOrWhiteSpace(SelectedCategory))
                {
                    filteredPhotos = filteredPhotos.Where(p => p.Category == SelectedCategory);
                }
                
                return filteredPhotos.OrderByDescending(p => p.DateTaken);
            }
        }
        
        /// <summary>
        /// List of distinct categories for filtering
        /// </summary>
        public IEnumerable<string> Categories
        {
            get
            {
                return Photos
                    .Select(p => p.Category)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
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
            "Volt",
            "Tank",
            "Decor",
            "Plants",
            "Treatment",
            "Before & After"
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
        public Visibility EmptyStateVisibility => SelectedPhoto == null ? Visibility.Visible : Visibility.Collapsed;
        
        #endregion
        
        #region Commands
        
        /// <summary>
        /// Command to add a new photo
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
        /// Command to edit the selected photo
        /// </summary>
        public ICommand EditPhotoCommand { get; }
        
        /// <summary>
        /// Command to save changes to the selected photo
        /// </summary>
        public ICommand SavePhotoCommand { get; }
        
        /// <summary>
        /// Command to cancel editing the selected photo
        /// </summary>
        public ICommand CancelEditCommand { get; }
        
        /// <summary>
        /// Command to delete the selected photo
        /// </summary>
        public ICommand DeletePhotoCommand { get; }
        
        /// <summary>
        /// Command to view a photo in detail
        /// </summary>
        public ICommand ViewPhotoCommand { get; }
        
        /// <summary>
        /// Command to close the photo detail view
        /// </summary>
        public ICommand ClosePhotoViewCommand { get; }
        
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the GalleryViewModel class
        /// </summary>
        public GalleryViewModel()
        {
            // Initialize collections
            Photos = new ObservableCollection<FishPhoto>();
            
            // Load initial data
            LoadPhotos();
            
            // Initialize commands
            AddPhotoCommand = new RelayCommand(_ => AddPhoto());
            SavePhotoCommand = new RelayCommand(_ => SavePhoto());
            CancelAddPhotoCommand = new RelayCommand(_ => CancelAddPhoto());
            EditPhotoCommand = new RelayCommand(_ => EditPhoto());
            SavePhotoCommand = new RelayCommand(_ => SavePhoto());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            DeletePhotoCommand = new RelayCommand(_ => DeletePhoto());
            ViewPhotoCommand = new RelayCommand(_ => ViewPhoto());
            ClosePhotoViewCommand = new RelayCommand(_ => ClosePhotoView());
            
            // Initialize new photo
            ResetNewPhoto();
        }
        
        /// <summary>
        /// Loads all photos from the database
        /// </summary>
        public void LoadPhotos()
        {
            Photos.Clear();
            
            // Only load non-treatment photos in the gallery
            var photos = App.DatabaseManager.GetAllPhotos().Where(p => !p.IsTreatmentPhoto);
            foreach (var photo in photos)
            {
                Photos.Add(photo);
            }
            
            OnPropertyChanged(nameof(FilteredPhotos));
            OnPropertyChanged(nameof(Categories));
        }
        
        /// <summary>
        /// Creates a deep copy of a photo
        /// </summary>
        /// <param name="source">The photo to clone</param>
        /// <returns>A new photo with the same values</returns>
        private FishPhoto ClonePhoto(FishPhoto source)
        {
            return new FishPhoto
            {
                Id = source.Id,
                Caption = source.Caption,
                DateTaken = source.DateTaken,
                FilePath = source.FilePath,
                FileName = source.FileName,
                Category = source.Category,
                IsTreatmentPhoto = source.IsTreatmentPhoto,
                TreatmentPlanId = source.TreatmentPlanId
            };
        }
        
        /// <summary>
        /// Shows the dialog for adding a new photo
        /// </summary>
        private void AddPhoto()
        {
            ResetNewPhoto();
            IsAddingPhoto = true;
        }
        
        /// <summary>
        /// Saves a new photo to the database
        /// </summary>
        private void SavePhoto()
        {
            string caption = NewPhoto?.Caption ?? string.Empty;
            string category = NewPhoto?.Category ?? "General";
            
            var photo = ImageHelper.SelectAndSaveImage(caption, category);
            
            if (photo != null)
            {
                Photos.Add(photo);
                SelectedPhoto = photo;
                
                OnPropertyChanged(nameof(FilteredPhotos));
                OnPropertyChanged(nameof(Categories));
            }
            
            IsAddingPhoto = false;
        }
        
        /// <summary>
        /// Cancels adding a new photo
        /// </summary>
        private void CancelAddPhoto()
        {
            IsAddingPhoto = false;
        }
        
        /// <summary>
        /// Resets the new photo to default values
        /// </summary>
        private void ResetNewPhoto()
        {
            NewPhoto = new FishPhoto
            {
                DateTaken = DateTime.Now,
                Category = "General"
            };
        }
        
        /// <summary>
        /// Enters edit mode for the selected photo
        /// </summary>
        private void EditPhoto()
        {
            if (SelectedPhoto != null)
            {
                IsEditing = true;
            }
        }
        
        /// <summary>
        /// Saves changes to the selected photo
        /// </summary>
        private void SavePhoto()
        {
            if (EditablePhoto != null)
            {
                App.DatabaseManager.UpdatePhoto(EditablePhoto);
                
                // Update the item in the collection
                int index = Photos.IndexOf(SelectedPhoto);
                if (index >= 0)
                {
                    Photos[index] = EditablePhoto;
                    SelectedPhoto = EditablePhoto;
                }
                
                IsEditing = false;
                OnPropertyChanged(nameof(FilteredPhotos));
                OnPropertyChanged(nameof(Categories));
            }
        }
        
        /// <summary>
        /// Cancels editing the selected photo
        /// </summary>
        private void CancelEdit()
        {
            if (SelectedPhoto != null)
            {
                EditablePhoto = ClonePhoto(SelectedPhoto);
            }
            
            IsEditing = false;
        }
        
        /// <summary>
        /// Deletes the selected photo
        /// </summary>
        private void DeletePhoto()
        {
            if (SelectedPhoto != null)
            {
                var confirmResult = NotificationHelper.ShowConfirmation(
                    "Delete Photo",
                    "Are you sure you want to delete this photo?");
                    
                if (confirmResult)
                {
                    ImageHelper.DeleteImage(SelectedPhoto);
                    Photos.Remove(SelectedPhoto);
                    SelectedPhoto = null;
                    EditablePhoto = null;
                    
                    OnPropertyChanged(nameof(FilteredPhotos));
                    OnPropertyChanged(nameof(Categories));
                }
            }
        }
        
        /// <summary>
        /// Shows the photo detail view
        /// </summary>
        private void ViewPhoto()
        {
            if (SelectedPhoto != null)
            {
                CurrentPhotoImage = ImageHelper.LoadImage(SelectedPhoto);
                IsViewingPhoto = true;
            }
        }
        
        /// <summary>
        /// Closes the photo detail view
        /// </summary>
        private void ClosePhotoView()
        {
            IsViewingPhoto = false;
            CurrentPhotoImage = null;
        }
    }
}

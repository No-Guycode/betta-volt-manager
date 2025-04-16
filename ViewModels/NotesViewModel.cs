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
    /// ViewModel for the Notes view
    /// </summary>
    public class NotesViewModel : ViewModelBase
    {
        #region Fields
        
        private ObservableCollection<Note> _notes;
        private Note _selectedNote;
        private Note _editableNote;
        private Note _newNote;
        private string _searchText;
        private string _selectedTag;
        private bool _isEditing;
        private bool _isAddingNote;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Collection of all notes
        /// </summary>
        public ObservableCollection<Note> Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }
        
        /// <summary>
        /// The currently selected note
        /// </summary>
        public Note SelectedNote
        {
            get => _selectedNote;
            set
            {
                if (SetProperty(ref _selectedNote, value) && value != null)
                {
                    EditableNote = CloneNote(value);
                }
                OnPropertyChanged(nameof(EmptyStateVisibility));
            }
        }
        
        /// <summary>
        /// Editable copy of the selected note
        /// </summary>
        public Note EditableNote
        {
            get => _editableNote;
            set => SetProperty(ref _editableNote, value);
        }
        
        /// <summary>
        /// New note being created
        /// </summary>
        public Note NewNote
        {
            get => _newNote;
            set
            {
                SetProperty(ref _newNote, value);
                OnPropertyChanged(nameof(CanAddNote));
            }
        }
        
        /// <summary>
        /// Search text for filtering notes
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                OnPropertyChanged(nameof(FilteredNotes));
            }
        }
        
        /// <summary>
        /// Selected tag for filtering notes
        /// </summary>
        public string SelectedTag
        {
            get => _selectedTag;
            set
            {
                SetProperty(ref _selectedTag, value);
                OnPropertyChanged(nameof(FilteredNotes));
            }
        }
        
        /// <summary>
        /// Indicates whether the selected note is being edited
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
        /// Indicates whether a new note is being added
        /// </summary>
        public bool IsAddingNote
        {
            get => _isAddingNote;
            set => SetProperty(ref _isAddingNote, value);
        }
        
        /// <summary>
        /// Filtered notes based on search text and selected tag
        /// </summary>
        public IEnumerable<Note> FilteredNotes
        {
            get
            {
                var filteredNotes = Notes.AsEnumerable();
                
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var search = SearchText.ToLower();
                    filteredNotes = filteredNotes.Where(n => 
                        n.Title.ToLower().Contains(search) || 
                        n.Content.ToLower().Contains(search) ||
                        n.Tags.ToLower().Contains(search));
                }
                
                if (!string.IsNullOrWhiteSpace(SelectedTag))
                {
                    filteredNotes = filteredNotes.Where(n => 
                        !string.IsNullOrWhiteSpace(n.Tags) && 
                        n.Tags.Split(',').Select(t => t.Trim()).Contains(SelectedTag));
                }
                
                return filteredNotes.OrderByDescending(n => n.ModifiedDateTime);
            }
        }
        
        /// <summary>
        /// List of all tags used in notes
        /// </summary>
        public IEnumerable<string> AllTags
        {
            get
            {
                var tags = new HashSet<string>();
                
                foreach (var note in Notes)
                {
                    if (!string.IsNullOrWhiteSpace(note.Tags))
                    {
                        foreach (var tag in note.Tags.Split(',').Select(t => t.Trim()))
                        {
                            if (!string.IsNullOrWhiteSpace(tag))
                            {
                                tags.Add(tag);
                            }
                        }
                    }
                }
                
                return tags.OrderBy(t => t);
            }
        }
        
        /// <summary>
        /// Suggested tag options
        /// </summary>
        public List<string> TagOptions => new List<string>(AllTags);
        
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
        public Visibility EmptyStateVisibility => SelectedNote == null ? Visibility.Visible : Visibility.Collapsed;
        
        /// <summary>
        /// Indicates whether a new note can be added (validates required fields)
        /// </summary>
        public bool CanAddNote => NewNote != null && 
                               !string.IsNullOrWhiteSpace(NewNote.Title) && 
                               !string.IsNullOrWhiteSpace(NewNote.Content);
        
        #endregion
        
        #region Commands
        
        /// <summary>
        /// Command to add a new note
        /// </summary>
        public ICommand AddNoteCommand { get; }
        
        /// <summary>
        /// Command to save a new note
        /// </summary>
        public ICommand SaveNewNoteCommand { get; }
        
        /// <summary>
        /// Command to cancel adding a note
        /// </summary>
        public ICommand CancelAddNoteCommand { get; }
        
        /// <summary>
        /// Command to edit the selected note
        /// </summary>
        public ICommand EditNoteCommand { get; }
        
        /// <summary>
        /// Command to save changes to the selected note
        /// </summary>
        public ICommand SaveNoteCommand { get; }
        
        /// <summary>
        /// Command to cancel editing the selected note
        /// </summary>
        public ICommand CancelEditCommand { get; }
        
        /// <summary>
        /// Command to delete the selected note
        /// </summary>
        public ICommand DeleteNoteCommand { get; }
        
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the NotesViewModel class
        /// </summary>
        public NotesViewModel()
        {
            // Initialize collections
            Notes = new ObservableCollection<Note>();
            
            // Load initial data
            LoadNotes();
            
            // Initialize commands
            AddNoteCommand = new RelayCommand(_ => AddNote());
            SaveNewNoteCommand = new RelayCommand(_ => SaveNewNote(), _ => CanAddNote);
            CancelAddNoteCommand = new RelayCommand(_ => CancelAddNote());
            EditNoteCommand = new RelayCommand(_ => EditNote());
            SaveNoteCommand = new RelayCommand(_ => SaveNote());
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            DeleteNoteCommand = new RelayCommand(_ => DeleteNote());
            
            // Initialize new note
            ResetNewNote();
        }
        
        /// <summary>
        /// Loads all notes from the database
        /// </summary>
        private void LoadNotes()
        {
            Notes.Clear();
            
            var notes = App.DatabaseManager.GetAllNotes();
            foreach (var note in notes)
            {
                Notes.Add(note);
            }
            
            OnPropertyChanged(nameof(AllTags));
            OnPropertyChanged(nameof(TagOptions));
        }
        
        /// <summary>
        /// Creates a deep copy of a note
        /// </summary>
        /// <param name="source">The note to clone</param>
        /// <returns>A new note with the same values</returns>
        private Note CloneNote(Note source)
        {
            return new Note
            {
                Id = source.Id,
                Title = source.Title,
                Content = source.Content,
                CreatedDateTime = source.CreatedDateTime,
                ModifiedDateTime = source.ModifiedDateTime,
                Tags = source.Tags
            };
        }
        
        /// <summary>
        /// Shows the dialog for adding a new note
        /// </summary>
        private void AddNote()
        {
            ResetNewNote();
            IsAddingNote = true;
        }
        
        /// <summary>
        /// Saves a new note to the database
        /// </summary>
        private void SaveNewNote()
        {
            int id = App.DatabaseManager.AddNote(NewNote);
            NewNote.Id = id;
            
            Notes.Add(NewNote);
            SelectedNote = NewNote;
            
            OnPropertyChanged(nameof(FilteredNotes));
            OnPropertyChanged(nameof(AllTags));
            OnPropertyChanged(nameof(TagOptions));
            
            ResetNewNote();
            IsAddingNote = false;
        }
        
        /// <summary>
        /// Cancels adding a new note
        /// </summary>
        private void CancelAddNote()
        {
            IsAddingNote = false;
        }
        
        /// <summary>
        /// Resets the new note to default values
        /// </summary>
        private void ResetNewNote()
        {
            NewNote = new Note
            {
                CreatedDateTime = DateTime.Now,
                ModifiedDateTime = DateTime.Now
            };
        }
        
        /// <summary>
        /// Enters edit mode for the selected note
        /// </summary>
        private void EditNote()
        {
            if (SelectedNote != null)
            {
                IsEditing = true;
            }
        }
        
        /// <summary>
        /// Saves changes to the selected note
        /// </summary>
        private void SaveNote()
        {
            if (EditableNote != null)
            {
                // Update the modification timestamp
                EditableNote.ModifiedDateTime = DateTime.Now;
                
                App.DatabaseManager.UpdateNote(EditableNote);
                
                // Update the item in the collection
                int index = Notes.IndexOf(SelectedNote);
                if (index >= 0)
                {
                    Notes[index] = EditableNote;
                    SelectedNote = EditableNote;
                }
                
                IsEditing = false;
                
                OnPropertyChanged(nameof(FilteredNotes));
                OnPropertyChanged(nameof(AllTags));
                OnPropertyChanged(nameof(TagOptions));
            }
        }
        
        /// <summary>
        /// Cancels editing the selected note
        /// </summary>
        private void CancelEdit()
        {
            if (SelectedNote != null)
            {
                EditableNote = CloneNote(SelectedNote);
            }
            
            IsEditing = false;
        }
        
        /// <summary>
        /// Deletes the selected note
        /// </summary>
        private void DeleteNote()
        {
            if (SelectedNote != null)
            {
                var confirmResult = NotificationHelper.ShowConfirmation(
                    "Delete Note",
                    $"Are you sure you want to delete the note '{SelectedNote.Title}'?");
                    
                if (confirmResult)
                {
                    App.DatabaseManager.DeleteNote(SelectedNote.Id);
                    Notes.Remove(SelectedNote);
                    SelectedNote = null;
                    EditableNote = null;
                    
                    OnPropertyChanged(nameof(FilteredNotes));
                    OnPropertyChanged(nameof(AllTags));
                    OnPropertyChanged(nameof(TagOptions));
                }
            }
        }
    }
    
    /// <summary>
    /// Extension methods for Note objects
    /// </summary>
    public static class NoteExtensions
    {
        /// <summary>
        /// Gets the list of tags from a note's Tags string
        /// </summary>
        /// <param name="note">The note</param>
        /// <returns>List of individual tags</returns>
        public static IEnumerable<string> TagList(this Note note)
        {
            if (string.IsNullOrWhiteSpace(note.Tags))
                return new List<string>();
                
            return note.Tags.Split(',')
                     .Select(t => t.Trim())
                     .Where(t => !string.IsNullOrWhiteSpace(t));
        }
    }
}

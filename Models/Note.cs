using System;

namespace VoltBettaManager.Models
{
    /// <summary>
    /// Represents a general note or observation
    /// </summary>
    public class Note
    {
        /// <summary>
        /// Unique identifier for the note
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Title of the note
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Content of the note
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Date and time when the note was created
        /// </summary>
        public DateTime CreatedDateTime { get; set; }
        
        /// <summary>
        /// Date and time when the note was last modified
        /// </summary>
        public DateTime ModifiedDateTime { get; set; }
        
        /// <summary>
        /// Optional tags for categorizing or filtering notes
        /// </summary>
        public string Tags { get; set; }
        
        /// <summary>
        /// Creates a new note with the current date and time
        /// </summary>
        public Note()
        {
            CreatedDateTime = DateTime.Now;
            ModifiedDateTime = DateTime.Now;
        }
        
        /// <summary>
        /// Creates a new note with specified details
        /// </summary>
        /// <param name="title">Title of the note</param>
        /// <param name="content">Content of the note</param>
        /// <param name="tags">Optional tags for the note</param>
        public Note(string title, string content, string tags = "")
        {
            Title = title;
            Content = content;
            Tags = tags;
            CreatedDateTime = DateTime.Now;
            ModifiedDateTime = DateTime.Now;
        }
        
        /// <summary>
        /// Updates the note content and sets the modified date to the current time
        /// </summary>
        /// <param name="title">New title for the note</param>
        /// <param name="content">New content for the note</param>
        /// <param name="tags">New tags for the note</param>
        public void Update(string title, string content, string tags)
        {
            Title = title;
            Content = content;
            Tags = tags;
            ModifiedDateTime = DateTime.Now;
        }
    }
}

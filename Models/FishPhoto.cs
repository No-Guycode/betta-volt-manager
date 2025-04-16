using System;

namespace VoltBettaManager.Models
{
    /// <summary>
    /// Represents a photo of the fish or tank
    /// </summary>
    public class FishPhoto
    {
        /// <summary>
        /// Unique identifier for the photo
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Caption or title for the photo
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// Date and time when the photo was taken
        /// </summary>
        public DateTime DateTaken { get; set; }
        
        /// <summary>
        /// File path where the photo is stored
        /// </summary>
        public string FilePath { get; set; }
        
        /// <summary>
        /// File name of the photo
        /// </summary>
        public string FileName { get; set; }
        
        /// <summary>
        /// Optional category or tag for the photo
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Indicates whether this is a treatment progress photo
        /// </summary>
        public bool IsTreatmentPhoto { get; set; }
        
        /// <summary>
        /// Associated treatment plan ID (if applicable)
        /// </summary>
        public int? TreatmentPlanId { get; set; }
        
        /// <summary>
        /// Creates a new photo entry with the current date and time
        /// </summary>
        public FishPhoto()
        {
            DateTaken = DateTime.Now;
            IsTreatmentPhoto = false;
        }
        
        /// <summary>
        /// Creates a new photo entry with specified details
        /// </summary>
        /// <param name="caption">Caption for the photo</param>
        /// <param name="filePath">File path where the photo is stored</param>
        /// <param name="fileName">File name of the photo</param>
        /// <param name="category">Optional category or tag</param>
        public FishPhoto(string caption, string filePath, string fileName, string category = "General")
        {
            Caption = caption;
            FilePath = filePath;
            FileName = fileName;
            Category = category;
            DateTaken = DateTime.Now;
            IsTreatmentPhoto = false;
        }
        
        /// <summary>
        /// Gets the full path to the photo file
        /// </summary>
        public string FullPath => System.IO.Path.Combine(FilePath, FileName);
    }
}

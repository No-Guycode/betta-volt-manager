using System;

namespace VoltBettaManager.Models
{
    /// <summary>
    /// Represents a log entry for tank-related activities or observations
    /// </summary>
    public class TankLog
    {
        /// <summary>
        /// Unique identifier for the log
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Title of the log entry
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Detailed description of the log entry
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Date and time when the log was created
        /// </summary>
        public DateTime LogDateTime { get; set; }
        
        /// <summary>
        /// Optional category or tag for the log entry
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Creates a new log entry with the current date and time
        /// </summary>
        public TankLog()
        {
            LogDateTime = DateTime.Now;
        }
        
        /// <summary>
        /// Creates a new log entry with specified details
        /// </summary>
        /// <param name="title">Title of the log entry</param>
        /// <param name="description">Detailed description</param>
        /// <param name="category">Optional category or tag</param>
        public TankLog(string title, string description, string category = "General")
        {
            Title = title;
            Description = description;
            LogDateTime = DateTime.Now;
            Category = category;
        }
        
        public override string ToString()
        {
            return $"{LogDateTime:g} - {Title}";
        }
    }
}

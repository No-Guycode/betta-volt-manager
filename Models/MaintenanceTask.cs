using System;

namespace VoltBettaManager.Models
{
    /// <summary>
    /// Represents a scheduled maintenance task for the fish tank
    /// </summary>
    public class MaintenanceTask
    {
        /// <summary>
        /// Unique identifier for the task
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Title of the maintenance task
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Detailed description of the task
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Category of the maintenance task
        /// </summary>
        public MaintenanceCategory Category { get; set; }
        
        /// <summary>
        /// Date and time when the task is scheduled
        /// </summary>
        public DateTime ScheduledDateTime { get; set; }
        
        /// <summary>
        /// Indicates whether the task repeats on a schedule
        /// </summary>
        public bool IsRecurring { get; set; }
        
        /// <summary>
        /// Frequency of recurrence in days (if recurring)
        /// </summary>
        public int RecurrenceFrequencyDays { get; set; }
        
        /// <summary>
        /// Date and time when the task was last completed
        /// </summary>
        public DateTime? LastCompletedDateTime { get; set; }
        
        /// <summary>
        /// Indicates whether notifications are enabled for this task
        /// </summary>
        public bool NotificationsEnabled { get; set; }
        
        /// <summary>
        /// Unique notification identifier used by the notification system
        /// </summary>
        public string NotificationId { get; set; }
        
        /// <summary>
        /// Creates a new maintenance task with default values
        /// </summary>
        public MaintenanceTask()
        {
            ScheduledDateTime = DateTime.Now.AddDays(1);
            NotificationsEnabled = true;
            IsRecurring = false;
            RecurrenceFrequencyDays = 7;
            NotificationId = Guid.NewGuid().ToString("N");
        }
        
        /// <summary>
        /// Gets the status of the task based on its schedule and completion
        /// </summary>
        public MaintenanceStatus Status
        {
            get
            {
                if (LastCompletedDateTime.HasValue)
                {
                    if (ScheduledDateTime > DateTime.Now)
                    {
                        return MaintenanceStatus.Upcoming;
                    }
                    else if (IsRecurring && 
                             LastCompletedDateTime.Value.AddDays(RecurrenceFrequencyDays) < DateTime.Now)
                    {
                        return MaintenanceStatus.Overdue;
                    }
                    else if (!IsRecurring && LastCompletedDateTime.Value < ScheduledDateTime)
                    {
                        return MaintenanceStatus.Completed;
                    }
                }
                
                if (ScheduledDateTime < DateTime.Now)
                {
                    return MaintenanceStatus.Overdue;
                }
                
                return MaintenanceStatus.Upcoming;
            }
        }
        
        /// <summary>
        /// Calculates the next scheduled date and time for a recurring task
        /// </summary>
        /// <returns>The next scheduled date and time</returns>
        public DateTime GetNextScheduledDateTime()
        {
            if (!IsRecurring)
            {
                return ScheduledDateTime;
            }
            
            if (LastCompletedDateTime.HasValue)
            {
                return LastCompletedDateTime.Value.AddDays(RecurrenceFrequencyDays);
            }
            
            return ScheduledDateTime;
        }
        
        /// <summary>
        /// Marks the task as completed at the current time
        /// </summary>
        public void MarkCompleted()
        {
            LastCompletedDateTime = DateTime.Now;
            
            if (IsRecurring)
            {
                ScheduledDateTime = DateTime.Now.AddDays(RecurrenceFrequencyDays);
            }
        }
    }
    
    /// <summary>
    /// Categories of maintenance tasks
    /// </summary>
    public enum MaintenanceCategory
    {
        WaterChange,
        Feeding,
        Enrichment,
        FilterCleaning,
        PlantMaintenance,
        Other
    }
    
    /// <summary>
    /// Status of a maintenance task
    /// </summary>
    public enum MaintenanceStatus
    {
        Upcoming,
        Overdue,
        Completed
    }
}

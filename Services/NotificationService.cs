using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Uwp.Notifications;
using VoltBettaManager.Models;
using Windows.UI.Notifications;
using System.Threading.Tasks;
using System.Timers;

namespace VoltBettaManager.Services
{
    /// <summary>
    /// Service for managing notifications for maintenance tasks
    /// </summary>
    public class NotificationService
    {
        private Timer _notificationCheckTimer;
        private const int CheckIntervalMinutes = 15;
        
        /// <summary>
        /// Initializes a new instance of the NotificationService class
        /// </summary>
        public NotificationService()
        {
            _notificationCheckTimer = new Timer(CheckIntervalMinutes * 60 * 1000); // Convert minutes to milliseconds
            _notificationCheckTimer.Elapsed += NotificationCheckTimer_Elapsed;
            _notificationCheckTimer.Start();
        }
        
        /// <summary>
        /// Loads all maintenance tasks from the database and sets up notifications for them
        /// </summary>
        public void SetupNotificationsFromDatabase()
        {
            var tasks = App.DatabaseManager.GetAllMaintenanceTasks();
            
            // Clear existing notifications
            ClearAllScheduledNotifications();
            
            // Set up notifications for all enabled tasks
            foreach (var task in tasks.Where(t => t.NotificationsEnabled))
            {
                if (task.Status == MaintenanceStatus.Upcoming)
                {
                    ScheduleNotification(task);
                }
            }
        }
        
        /// <summary>
        /// Schedules a notification for a maintenance task
        /// </summary>
        /// <param name="task">The maintenance task to schedule a notification for</param>
        public void ScheduleNotification(MaintenanceTask task)
        {
            if (!task.NotificationsEnabled)
                return;
            
            // Schedule notification for 1 hour before the task
            var notifyTime = task.ScheduledDateTime.AddHours(-1);
            
            // If the notify time is in the past, don't schedule
            if (notifyTime < DateTime.Now)
                return;
            
            // Create the notification
            var builder = new ToastContentBuilder()
                .AddText("Volt Maintenance Reminder")
                .AddText(task.Title)
                .AddText(task.Description)
                .AddText($"Scheduled for: {task.ScheduledDateTime.ToString("g")}");
            
            // Schedule the notification
            builder.Schedule(notifyTime, toast =>
            {
                toast.Tag = task.NotificationId;
                toast.Group = "MaintenanceTasks";
            });
        }
        
        /// <summary>
        /// Cancels a scheduled notification for a maintenance task
        /// </summary>
        /// <param name="task">The maintenance task to cancel the notification for</param>
        public void CancelNotification(MaintenanceTask task)
        {
            ToastNotificationManagerCompat.History.Remove(task.NotificationId, "MaintenanceTasks");
        }
        
        /// <summary>
        /// Shows an immediate notification
        /// </summary>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        public void ShowNotification(string title, string message)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }
        
        /// <summary>
        /// Clears all scheduled notifications
        /// </summary>
        public void ClearAllScheduledNotifications()
        {
            ToastNotificationManagerCompat.History.Clear();
        }
        
        private void NotificationCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Check for upcoming maintenance tasks that need notifications
            var tasks = App.DatabaseManager.GetAllMaintenanceTasks();
            
            foreach (var task in tasks.Where(t => t.NotificationsEnabled))
            {
                // If the task is due within the next 24 hours but more than 1 hour away
                var timeUntilDue = task.ScheduledDateTime - DateTime.Now;
                
                if (timeUntilDue.TotalHours <= 24 && timeUntilDue.TotalHours > 1)
                {
                    // Check if we need to schedule a notification
                    ScheduleNotification(task);
                }
            }
        }
        
        /// <summary>
        /// Check for overdue maintenance tasks and show notifications
        /// </summary>
        public void CheckForOverdueTasks()
        {
            var tasks = App.DatabaseManager.GetAllMaintenanceTasks();
            
            foreach (var task in tasks.Where(t => 
                t.NotificationsEnabled && 
                t.Status == MaintenanceStatus.Overdue))
            {
                // Show notification for overdue task
                ShowNotification(
                    "Overdue Maintenance Task", 
                    $"{task.Title} was scheduled for {task.ScheduledDateTime:g} and is now overdue.");
            }
        }
    }
}

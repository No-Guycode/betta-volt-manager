using System;
using System.Windows;
using VoltBettaManager.Models;

namespace VoltBettaManager.Helpers
{
    /// <summary>
    /// Helper class for working with notifications and UI dialogs
    /// </summary>
    public static class NotificationHelper
    {
        /// <summary>
        /// Shows a confirmation dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <returns>True if the user confirmed, false otherwise</returns>
        public static bool ShowConfirmation(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
        
        /// <summary>
        /// Shows an information message
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        public static void ShowInformation(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        /// <summary>
        /// Shows an error message
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        public static void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        /// <summary>
        /// Shows a warning message
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        public static void ShowWarning(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        
        /// <summary>
        /// Returns a formatted string representation of a maintenance task status
        /// </summary>
        /// <param name="status">The maintenance status</param>
        /// <returns>A formatted status string</returns>
        public static string FormatMaintenanceStatus(MaintenanceStatus status)
        {
            switch (status)
            {
                case MaintenanceStatus.Upcoming:
                    return "Upcoming";
                case MaintenanceStatus.Overdue:
                    return "Overdue";
                case MaintenanceStatus.Completed:
                    return "Completed";
                default:
                    return status.ToString();
            }
        }
        
        /// <summary>
        /// Returns a formatted string representation of a maintenance category
        /// </summary>
        /// <param name="category">The maintenance category</param>
        /// <returns>A formatted category string</returns>
        public static string FormatMaintenanceCategory(MaintenanceCategory category)
        {
            switch (category)
            {
                case MaintenanceCategory.WaterChange:
                    return "Water Change";
                case MaintenanceCategory.Feeding:
                    return "Feeding";
                case MaintenanceCategory.Enrichment:
                    return "Enrichment";
                case MaintenanceCategory.FilterCleaning:
                    return "Filter Cleaning";
                case MaintenanceCategory.PlantMaintenance:
                    return "Plant Maintenance";
                case MaintenanceCategory.Other:
                    return "Other";
                default:
                    return category.ToString();
            }
        }
        
        /// <summary>
        /// Returns a formatted string representation of a treatment status
        /// </summary>
        /// <param name="status">The treatment status</param>
        /// <returns>A formatted status string</returns>
        public static string FormatTreatmentStatus(TreatmentStatus status)
        {
            switch (status)
            {
                case TreatmentStatus.Active:
                    return "Active";
                case TreatmentStatus.Completed:
                    return "Completed";
                case TreatmentStatus.Discontinued:
                    return "Discontinued";
                default:
                    return status.ToString();
            }
        }
        
        /// <summary>
        /// Returns a formatted string representation of a symptom severity
        /// </summary>
        /// <param name="severity">The symptom severity</param>
        /// <returns>A formatted severity string</returns>
        public static string FormatSymptomSeverity(SymptomSeverity severity)
        {
            switch (severity)
            {
                case SymptomSeverity.None:
                    return "None";
                case SymptomSeverity.Mild:
                    return "Mild";
                case SymptomSeverity.Moderate:
                    return "Moderate";
                case SymptomSeverity.Severe:
                    return "Severe";
                default:
                    return severity.ToString();
            }
        }
        
        /// <summary>
        /// Returns a formatted string representation of a plant location
        /// </summary>
        /// <param name="location">The plant location</param>
        /// <returns>A formatted location string</returns>
        public static string FormatPlantLocation(PlantLocation location)
        {
            switch (location)
            {
                case PlantLocation.InTank:
                    return "In Tank";
                case PlantLocation.InQuarantine:
                    return "In Quarantine";
                case PlantLocation.InPreparation:
                    return "In Preparation";
                case PlantLocation.Removed:
                    return "Removed";
                default:
                    return location.ToString();
            }
        }
        
        /// <summary>
        /// Returns a formatted string representation of a light level
        /// </summary>
        /// <param name="level">The light level</param>
        /// <returns>A formatted light level string</returns>
        public static string FormatLightLevel(LightLevel level)
        {
            switch (level)
            {
                case LightLevel.Low:
                    return "Low";
                case LightLevel.Medium:
                    return "Medium";
                case LightLevel.High:
                    return "High";
                default:
                    return level.ToString();
            }
        }
        
        /// <summary>
        /// Returns a formatted string representation of an issue severity
        /// </summary>
        /// <param name="severity">The issue severity</param>
        /// <returns>A formatted severity string</returns>
        public static string FormatIssueSeverity(IssueSeverity severity)
        {
            switch (severity)
            {
                case IssueSeverity.Minor:
                    return "Minor";
                case IssueSeverity.Moderate:
                    return "Moderate";
                case IssueSeverity.Severe:
                    return "Severe";
                default:
                    return severity.ToString();
            }
        }
    }
}

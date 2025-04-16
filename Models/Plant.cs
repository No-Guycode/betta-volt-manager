using System;
using System.Collections.Generic;

namespace VoltBettaManager.Models
{
    /// <summary>
    /// Represents a plant in the fish tank or being prepared for the tank
    /// </summary>
    public class Plant
    {
        /// <summary>
        /// Unique identifier for the plant
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the plant species
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Scientific name of the plant (optional)
        /// </summary>
        public string ScientificName { get; set; }
        
        /// <summary>
        /// Date the plant was added to the tank or collection
        /// </summary>
        public DateTime AddedDate { get; set; }
        
        /// <summary>
        /// Detailed care instructions for the plant
        /// </summary>
        public string CareNotes { get; set; }
        
        /// <summary>
        /// Current placement or location of the plant
        /// </summary>
        public PlantLocation Location { get; set; }
        
        /// <summary>
        /// Light requirement level for the plant
        /// </summary>
        public LightLevel LightRequirement { get; set; }
        
        /// <summary>
        /// List of issues or warnings associated with the plant
        /// </summary>
        public List<PlantIssue> Issues { get; set; }
        
        /// <summary>
        /// Creates a new plant entry with default values
        /// </summary>
        public Plant()
        {
            AddedDate = DateTime.Now;
            Location = PlantLocation.InTank;
            LightRequirement = LightLevel.Medium;
            Issues = new List<PlantIssue>();
        }
        
        /// <summary>
        /// Adds a new issue or warning to the plant
        /// </summary>
        /// <param name="description">Description of the issue</param>
        /// <param name="severity">Severity level of the issue</param>
        /// <param name="dateIdentified">Date when the issue was identified</param>
        public void AddIssue(string description, IssueSeverity severity, DateTime dateIdentified)
        {
            Issues.Add(new PlantIssue
            {
                Description = description,
                Severity = severity,
                DateIdentified = dateIdentified,
                IsResolved = false
            });
        }
        
        /// <summary>
        /// Marks a specific plant issue as resolved
        /// </summary>
        /// <param name="issueIndex">Index of the issue in the Issues list</param>
        /// <param name="resolutionDate">Date when the issue was resolved</param>
        /// <param name="resolutionNotes">Notes about how the issue was resolved</param>
        public void ResolveIssue(int issueIndex, DateTime resolutionDate, string resolutionNotes)
        {
            if (issueIndex >= 0 && issueIndex < Issues.Count)
            {
                Issues[issueIndex].IsResolved = true;
                Issues[issueIndex].ResolutionDate = resolutionDate;
                Issues[issueIndex].ResolutionNotes = resolutionNotes;
            }
        }
    }
    
    /// <summary>
    /// Represents an issue or warning associated with a plant
    /// </summary>
    public class PlantIssue
    {
        /// <summary>
        /// Description of the plant issue
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Severity level of the issue
        /// </summary>
        public IssueSeverity Severity { get; set; }
        
        /// <summary>
        /// Date when the issue was identified
        /// </summary>
        public DateTime DateIdentified { get; set; }
        
        /// <summary>
        /// Indicates whether the issue has been resolved
        /// </summary>
        public bool IsResolved { get; set; }
        
        /// <summary>
        /// Date when the issue was resolved (if applicable)
        /// </summary>
        public DateTime? ResolutionDate { get; set; }
        
        /// <summary>
        /// Notes about how the issue was resolved
        /// </summary>
        public string ResolutionNotes { get; set; }
    }
    
    /// <summary>
    /// Possible locations for a plant
    /// </summary>
    public enum PlantLocation
    {
        InTank,
        InQuarantine,
        InPreparation,
        Removed
    }
    
    /// <summary>
    /// Light requirement levels for plants
    /// </summary>
    public enum LightLevel
    {
        Low,
        Medium,
        High
    }
    
    /// <summary>
    /// Severity levels for plant issues
    /// </summary>
    public enum IssueSeverity
    {
        Minor,
        Moderate,
        Severe
    }
}

using System;
using System.Collections.Generic;

namespace VoltBettaManager.Models
{
    /// <summary>
    /// Represents a treatment plan for fish illness
    /// </summary>
    public class TreatmentPlan
    {
        /// <summary>
        /// Unique identifier for the treatment plan
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the illness being treated
        /// </summary>
        public string IllnessName { get; set; }
        
        /// <summary>
        /// Detailed description of the illness and treatment approach
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Date when the treatment plan was started
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// Date when the treatment plan was completed (if applicable)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Current status of the treatment plan
        /// </summary>
        public TreatmentStatus Status { get; set; }
        
        /// <summary>
        /// Notes about medications and dosages used in the treatment
        /// </summary>
        public string MedicationNotes { get; set; }
        
        /// <summary>
        /// List of daily treatment logs
        /// </summary>
        public List<TreatmentLog> TreatmentLogs { get; set; }
        
        /// <summary>
        /// List of symptoms being tracked during the treatment
        /// </summary>
        public List<Symptom> Symptoms { get; set; }
        
        /// <summary>
        /// List of photos documenting the treatment progress
        /// </summary>
        public List<int> ProgressPhotoIds { get; set; }
        
        /// <summary>
        /// Creates a new treatment plan with default values
        /// </summary>
        public TreatmentPlan()
        {
            StartDate = DateTime.Now;
            Status = TreatmentStatus.Active;
            TreatmentLogs = new List<TreatmentLog>();
            Symptoms = new List<Symptom>();
            ProgressPhotoIds = new List<int>();
        }
        
        /// <summary>
        /// Adds a new daily treatment log
        /// </summary>
        /// <param name="date">Date of the treatment log</param>
        /// <param name="actions">Actions performed as part of the treatment</param>
        /// <param name="notes">Additional notes about the treatment</param>
        /// <returns>The newly created treatment log</returns>
        public TreatmentLog AddTreatmentLog(DateTime date, string actions, string notes)
        {
            var log = new TreatmentLog
            {
                Date = date,
                Actions = actions,
                Notes = notes
            };
            
            TreatmentLogs.Add(log);
            return log;
        }
        
        /// <summary>
        /// Adds a new symptom to track during the treatment
        /// </summary>
        /// <param name="name">Name of the symptom</param>
        /// <param name="severity">Initial severity of the symptom</param>
        /// <returns>The newly created symptom</returns>
        public Symptom AddSymptom(string name, SymptomSeverity severity)
        {
            var symptom = new Symptom
            {
                Name = name,
                Severity = severity,
                History = new List<SymptomHistory>
                {
                    new SymptomHistory
                    {
                        Date = DateTime.Now,
                        Severity = severity,
                        Notes = "Initial observation"
                    }
                }
            };
            
            Symptoms.Add(symptom);
            return symptom;
        }
        
        /// <summary>
        /// Updates the severity of a symptom
        /// </summary>
        /// <param name="symptomIndex">Index of the symptom in the Symptoms list</param>
        /// <param name="newSeverity">New severity level</param>
        /// <param name="notes">Notes about the symptom change</param>
        public void UpdateSymptomSeverity(int symptomIndex, SymptomSeverity newSeverity, string notes)
        {
            if (symptomIndex >= 0 && symptomIndex < Symptoms.Count)
            {
                var symptom = Symptoms[symptomIndex];
                symptom.Severity = newSeverity;
                
                symptom.History.Add(new SymptomHistory
                {
                    Date = DateTime.Now,
                    Severity = newSeverity,
                    Notes = notes
                });
            }
        }
        
        /// <summary>
        /// Marks the treatment plan as completed
        /// </summary>
        /// <param name="outcome">Outcome notes for the treatment</param>
        public void CompleteTreatment(string outcome)
        {
            Status = TreatmentStatus.Completed;
            EndDate = DateTime.Now;
            
            // Add a final treatment log with the outcome
            AddTreatmentLog(DateTime.Now, "Treatment completed", outcome);
        }
    }
    
    /// <summary>
    /// Represents a daily log entry for a treatment plan
    /// </summary>
    public class TreatmentLog
    {
        /// <summary>
        /// Date of the treatment log
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Actions performed as part of the treatment
        /// </summary>
        public string Actions { get; set; }
        
        /// <summary>
        /// Additional notes about the treatment
        /// </summary>
        public string Notes { get; set; }
    }
    
    /// <summary>
    /// Represents a symptom being tracked during treatment
    /// </summary>
    public class Symptom
    {
        /// <summary>
        /// Name of the symptom
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Current severity of the symptom
        /// </summary>
        public SymptomSeverity Severity { get; set; }
        
        /// <summary>
        /// History of changes in the symptom's severity
        /// </summary>
        public List<SymptomHistory> History { get; set; }
    }
    
    /// <summary>
    /// Represents a historical record of a symptom's severity
    /// </summary>
    public class SymptomHistory
    {
        /// <summary>
        /// Date when the symptom severity was recorded
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Severity level at the recorded date
        /// </summary>
        public SymptomSeverity Severity { get; set; }
        
        /// <summary>
        /// Notes about the symptom at this time
        /// </summary>
        public string Notes { get; set; }
    }
    
    /// <summary>
    /// Status options for a treatment plan
    /// </summary>
    public enum TreatmentStatus
    {
        Active,
        Completed,
        Discontinued
    }
    
    /// <summary>
    /// Severity levels for symptoms
    /// </summary>
    public enum SymptomSeverity
    {
        None,
        Mild,
        Moderate,
        Severe
    }
}

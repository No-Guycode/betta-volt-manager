using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.Json;
using VoltBettaManager.Models;

namespace VoltBettaManager.Data
{
    /// <summary>
    /// Manages database operations for the application
    /// </summary>
    public class DatabaseManager
    {
        private readonly string _connectionString;
        
        /// <summary>
        /// Initializes a new instance of the DatabaseManager class
        /// </summary>
        /// <param name="databasePath">Path to the SQLite database file</param>
        public DatabaseManager(string databasePath)
        {
            _connectionString = $"Data Source={databasePath};Version=3;";
        }
        
        /// <summary>
        /// Creates and initializes the database with tables if it doesn't exist
        /// </summary>
        public void InitializeDatabase()
        {
            if (!File.Exists(_connectionString.Replace("Data Source=", "").Replace(";Version=3;", "")))
            {
                SQLiteConnection.CreateFile(_connectionString.Replace("Data Source=", "").Replace(";Version=3;", ""));
            }
            
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                // Create tables if they don't exist
                CreateTankLogsTable(connection);
                CreateMaintenanceTasksTable(connection);
                CreatePlantsTable(connection);
                CreateTreatmentPlansTable(connection);
                CreatePhotosTable(connection);
                CreateNotesTable(connection);
            }
        }
        
        #region Table Creation
        
        private void CreateTankLogsTable(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS TankLogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT NOT NULL,
                        LogDateTime TEXT NOT NULL,
                        Category TEXT
                    )";
                command.ExecuteNonQuery();
            }
        }
        
        private void CreateMaintenanceTasksTable(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS MaintenanceTasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        Category INTEGER NOT NULL,
                        ScheduledDateTime TEXT NOT NULL,
                        IsRecurring INTEGER NOT NULL,
                        RecurrenceFrequencyDays INTEGER,
                        LastCompletedDateTime TEXT,
                        NotificationsEnabled INTEGER NOT NULL,
                        NotificationId TEXT NOT NULL
                    )";
                command.ExecuteNonQuery();
            }
        }
        
        private void CreatePlantsTable(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Plants (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        ScientificName TEXT,
                        AddedDate TEXT NOT NULL,
                        CareNotes TEXT,
                        Location INTEGER NOT NULL,
                        LightRequirement INTEGER NOT NULL,
                        Issues TEXT
                    )";
                command.ExecuteNonQuery();
            }
        }
        
        private void CreateTreatmentPlansTable(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS TreatmentPlans (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        IllnessName TEXT NOT NULL,
                        Description TEXT,
                        StartDate TEXT NOT NULL,
                        EndDate TEXT,
                        Status INTEGER NOT NULL,
                        MedicationNotes TEXT,
                        TreatmentLogs TEXT,
                        Symptoms TEXT,
                        ProgressPhotoIds TEXT
                    )";
                command.ExecuteNonQuery();
            }
        }
        
        private void CreatePhotosTable(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS FishPhotos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Caption TEXT,
                        DateTaken TEXT NOT NULL,
                        FilePath TEXT NOT NULL,
                        FileName TEXT NOT NULL,
                        Category TEXT,
                        IsTreatmentPhoto INTEGER NOT NULL,
                        TreatmentPlanId INTEGER
                    )";
                command.ExecuteNonQuery();
            }
        }
        
        private void CreateNotesTable(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Notes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Content TEXT NOT NULL,
                        CreatedDateTime TEXT NOT NULL,
                        ModifiedDateTime TEXT NOT NULL,
                        Tags TEXT
                    )";
                command.ExecuteNonQuery();
            }
        }
        
        #endregion
        
        #region Tank Logs
        
        /// <summary>
        /// Adds a new tank log to the database
        /// </summary>
        /// <param name="log">The tank log to add</param>
        /// <returns>The ID of the newly added log</returns>
        public int AddTankLog(TankLog log)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        INSERT INTO TankLogs (Title, Description, LogDateTime, Category)
                        VALUES (@Title, @Description, @LogDateTime, @Category);
                        SELECT last_insert_rowid();";
                    
                    command.Parameters.AddWithValue("@Title", log.Title);
                    command.Parameters.AddWithValue("@Description", log.Description);
                    command.Parameters.AddWithValue("@LogDateTime", log.LogDateTime.ToString("o"));
                    command.Parameters.AddWithValue("@Category", log.Category ?? "General");
                    
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        /// <summary>
        /// Updates an existing tank log in the database
        /// </summary>
        /// <param name="log">The tank log to update</param>
        public void UpdateTankLog(TankLog log)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        UPDATE TankLogs 
                        SET Title = @Title, Description = @Description, LogDateTime = @LogDateTime, Category = @Category
                        WHERE Id = @Id";
                    
                    command.Parameters.AddWithValue("@Id", log.Id);
                    command.Parameters.AddWithValue("@Title", log.Title);
                    command.Parameters.AddWithValue("@Description", log.Description);
                    command.Parameters.AddWithValue("@LogDateTime", log.LogDateTime.ToString("o"));
                    command.Parameters.AddWithValue("@Category", log.Category ?? "General");
                    
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Deletes a tank log from the database
        /// </summary>
        /// <param name="logId">The ID of the log to delete</param>
        public void DeleteTankLog(int logId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "DELETE FROM TankLogs WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", logId);
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Gets all tank logs from the database
        /// </summary>
        /// <returns>A list of tank logs</returns>
        public List<TankLog> GetAllTankLogs()
        {
            var logs = new List<TankLog>();
            
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM TankLogs ORDER BY LogDateTime DESC", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new TankLog
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                LogDateTime = DateTime.Parse(reader["LogDateTime"].ToString()),
                                Category = reader["Category"].ToString()
                            });
                        }
                    }
                }
            }
            
            return logs;
        }
        
        /// <summary>
        /// Gets a specific tank log by its ID
        /// </summary>
        /// <param name="logId">The ID of the log to retrieve</param>
        /// <returns>The tank log, or null if not found</returns>
        public TankLog GetTankLogById(int logId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM TankLogs WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", logId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TankLog
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                LogDateTime = DateTime.Parse(reader["LogDateTime"].ToString()),
                                Category = reader["Category"].ToString()
                            };
                        }
                    }
                }
            }
            
            return null;
        }
        
        #endregion
        
        #region Maintenance Tasks
        
        /// <summary>
        /// Adds a new maintenance task to the database
        /// </summary>
        /// <param name="task">The maintenance task to add</param>
        /// <returns>The ID of the newly added task</returns>
        public int AddMaintenanceTask(MaintenanceTask task)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        INSERT INTO MaintenanceTasks (
                            Title, Description, Category, ScheduledDateTime, 
                            IsRecurring, RecurrenceFrequencyDays, LastCompletedDateTime,
                            NotificationsEnabled, NotificationId
                        )
                        VALUES (
                            @Title, @Description, @Category, @ScheduledDateTime,
                            @IsRecurring, @RecurrenceFrequencyDays, @LastCompletedDateTime,
                            @NotificationsEnabled, @NotificationId
                        );
                        SELECT last_insert_rowid();";
                    
                    command.Parameters.AddWithValue("@Title", task.Title);
                    command.Parameters.AddWithValue("@Description", task.Description ?? "");
                    command.Parameters.AddWithValue("@Category", (int)task.Category);
                    command.Parameters.AddWithValue("@ScheduledDateTime", task.ScheduledDateTime.ToString("o"));
                    command.Parameters.AddWithValue("@IsRecurring", task.IsRecurring ? 1 : 0);
                    command.Parameters.AddWithValue("@RecurrenceFrequencyDays", task.RecurrenceFrequencyDays);
                    command.Parameters.AddWithValue("@LastCompletedDateTime", 
                        task.LastCompletedDateTime.HasValue ? task.LastCompletedDateTime.Value.ToString("o") : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@NotificationsEnabled", task.NotificationsEnabled ? 1 : 0);
                    command.Parameters.AddWithValue("@NotificationId", task.NotificationId);
                    
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        /// <summary>
        /// Updates an existing maintenance task in the database
        /// </summary>
        /// <param name="task">The maintenance task to update</param>
        public void UpdateMaintenanceTask(MaintenanceTask task)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        UPDATE MaintenanceTasks 
                        SET Title = @Title, 
                            Description = @Description, 
                            Category = @Category, 
                            ScheduledDateTime = @ScheduledDateTime,
                            IsRecurring = @IsRecurring, 
                            RecurrenceFrequencyDays = @RecurrenceFrequencyDays, 
                            LastCompletedDateTime = @LastCompletedDateTime,
                            NotificationsEnabled = @NotificationsEnabled, 
                            NotificationId = @NotificationId
                        WHERE Id = @Id";
                    
                    command.Parameters.AddWithValue("@Id", task.Id);
                    command.Parameters.AddWithValue("@Title", task.Title);
                    command.Parameters.AddWithValue("@Description", task.Description ?? "");
                    command.Parameters.AddWithValue("@Category", (int)task.Category);
                    command.Parameters.AddWithValue("@ScheduledDateTime", task.ScheduledDateTime.ToString("o"));
                    command.Parameters.AddWithValue("@IsRecurring", task.IsRecurring ? 1 : 0);
                    command.Parameters.AddWithValue("@RecurrenceFrequencyDays", task.RecurrenceFrequencyDays);
                    command.Parameters.AddWithValue("@LastCompletedDateTime", 
                        task.LastCompletedDateTime.HasValue ? task.LastCompletedDateTime.Value.ToString("o") : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@NotificationsEnabled", task.NotificationsEnabled ? 1 : 0);
                    command.Parameters.AddWithValue("@NotificationId", task.NotificationId);
                    
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Deletes a maintenance task from the database
        /// </summary>
        /// <param name="taskId">The ID of the task to delete</param>
        public void DeleteMaintenanceTask(int taskId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "DELETE FROM MaintenanceTasks WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", taskId);
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Gets all maintenance tasks from the database
        /// </summary>
        /// <returns>A list of maintenance tasks</returns>
        public List<MaintenanceTask> GetAllMaintenanceTasks()
        {
            var tasks = new List<MaintenanceTask>();
            
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM MaintenanceTasks ORDER BY ScheduledDateTime", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var task = new MaintenanceTask
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                Category = (MaintenanceCategory)Convert.ToInt32(reader["Category"]),
                                ScheduledDateTime = DateTime.Parse(reader["ScheduledDateTime"].ToString()),
                                IsRecurring = Convert.ToBoolean(Convert.ToInt32(reader["IsRecurring"])),
                                RecurrenceFrequencyDays = Convert.ToInt32(reader["RecurrenceFrequencyDays"]),
                                NotificationsEnabled = Convert.ToBoolean(Convert.ToInt32(reader["NotificationsEnabled"])),
                                NotificationId = reader["NotificationId"].ToString()
                            };
                            
                            if (reader["LastCompletedDateTime"] != DBNull.Value)
                            {
                                task.LastCompletedDateTime = DateTime.Parse(reader["LastCompletedDateTime"].ToString());
                            }
                            
                            tasks.Add(task);
                        }
                    }
                }
            }
            
            return tasks;
        }
        
        /// <summary>
        /// Gets a specific maintenance task by its ID
        /// </summary>
        /// <param name="taskId">The ID of the task to retrieve</param>
        /// <returns>The maintenance task, or null if not found</returns>
        public MaintenanceTask GetMaintenanceTaskById(int taskId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM MaintenanceTasks WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", taskId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var task = new MaintenanceTask
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                Category = (MaintenanceCategory)Convert.ToInt32(reader["Category"]),
                                ScheduledDateTime = DateTime.Parse(reader["ScheduledDateTime"].ToString()),
                                IsRecurring = Convert.ToBoolean(Convert.ToInt32(reader["IsRecurring"])),
                                RecurrenceFrequencyDays = Convert.ToInt32(reader["RecurrenceFrequencyDays"]),
                                NotificationsEnabled = Convert.ToBoolean(Convert.ToInt32(reader["NotificationsEnabled"])),
                                NotificationId = reader["NotificationId"].ToString()
                            };
                            
                            if (reader["LastCompletedDateTime"] != DBNull.Value)
                            {
                                task.LastCompletedDateTime = DateTime.Parse(reader["LastCompletedDateTime"].ToString());
                            }
                            
                            return task;
                        }
                    }
                }
            }
            
            return null;
        }
        
        #endregion
        
        #region Plants
        
        /// <summary>
        /// Adds a new plant to the database
        /// </summary>
        /// <param name="plant">The plant to add</param>
        /// <returns>The ID of the newly added plant</returns>
        public int AddPlant(Plant plant)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        INSERT INTO Plants (
                            Name, ScientificName, AddedDate, CareNotes, 
                            Location, LightRequirement, Issues
                        )
                        VALUES (
                            @Name, @ScientificName, @AddedDate, @CareNotes,
                            @Location, @LightRequirement, @Issues
                        );
                        SELECT last_insert_rowid();";
                    
                    command.Parameters.AddWithValue("@Name", plant.Name);
                    command.Parameters.AddWithValue("@ScientificName", plant.ScientificName ?? "");
                    command.Parameters.AddWithValue("@AddedDate", plant.AddedDate.ToString("o"));
                    command.Parameters.AddWithValue("@CareNotes", plant.CareNotes ?? "");
                    command.Parameters.AddWithValue("@Location", (int)plant.Location);
                    command.Parameters.AddWithValue("@LightRequirement", (int)plant.LightRequirement);
                    command.Parameters.AddWithValue("@Issues", JsonSerializer.Serialize(plant.Issues));
                    
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        /// <summary>
        /// Updates an existing plant in the database
        /// </summary>
        /// <param name="plant">The plant to update</param>
        public void UpdatePlant(Plant plant)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        UPDATE Plants 
                        SET Name = @Name, 
                            ScientificName = @ScientificName, 
                            AddedDate = @AddedDate, 
                            CareNotes = @CareNotes,
                            Location = @Location, 
                            LightRequirement = @LightRequirement, 
                            Issues = @Issues
                        WHERE Id = @Id";
                    
                    command.Parameters.AddWithValue("@Id", plant.Id);
                    command.Parameters.AddWithValue("@Name", plant.Name);
                    command.Parameters.AddWithValue("@ScientificName", plant.ScientificName ?? "");
                    command.Parameters.AddWithValue("@AddedDate", plant.AddedDate.ToString("o"));
                    command.Parameters.AddWithValue("@CareNotes", plant.CareNotes ?? "");
                    command.Parameters.AddWithValue("@Location", (int)plant.Location);
                    command.Parameters.AddWithValue("@LightRequirement", (int)plant.LightRequirement);
                    command.Parameters.AddWithValue("@Issues", JsonSerializer.Serialize(plant.Issues));
                    
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Deletes a plant from the database
        /// </summary>
        /// <param name="plantId">The ID of the plant to delete</param>
        public void DeletePlant(int plantId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "DELETE FROM Plants WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", plantId);
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Gets all plants from the database
        /// </summary>
        /// <returns>A list of plants</returns>
        public List<Plant> GetAllPlants()
        {
            var plants = new List<Plant>();
            
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM Plants ORDER BY Name", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var plant = new Plant
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                ScientificName = reader["ScientificName"].ToString(),
                                AddedDate = DateTime.Parse(reader["AddedDate"].ToString()),
                                CareNotes = reader["CareNotes"].ToString(),
                                Location = (PlantLocation)Convert.ToInt32(reader["Location"]),
                                LightRequirement = (LightLevel)Convert.ToInt32(reader["LightRequirement"]),
                                Issues = JsonSerializer.Deserialize<List<PlantIssue>>(reader["Issues"].ToString())
                            };
                            
                            plants.Add(plant);
                        }
                    }
                }
            }
            
            return plants;
        }
        
        /// <summary>
        /// Gets a specific plant by its ID
        /// </summary>
        /// <param name="plantId">The ID of the plant to retrieve</param>
        /// <returns>The plant, or null if not found</returns>
        public Plant GetPlantById(int plantId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM Plants WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", plantId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Plant
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                ScientificName = reader["ScientificName"].ToString(),
                                AddedDate = DateTime.Parse(reader["AddedDate"].ToString()),
                                CareNotes = reader["CareNotes"].ToString(),
                                Location = (PlantLocation)Convert.ToInt32(reader["Location"]),
                                LightRequirement = (LightLevel)Convert.ToInt32(reader["LightRequirement"]),
                                Issues = JsonSerializer.Deserialize<List<PlantIssue>>(reader["Issues"].ToString())
                            };
                        }
                    }
                }
            }
            
            return null;
        }
        
        #endregion
        
        #region Treatment Plans
        
        /// <summary>
        /// Adds a new treatment plan to the database
        /// </summary>
        /// <param name="plan">The treatment plan to add</param>
        /// <returns>The ID of the newly added treatment plan</returns>
        public int AddTreatmentPlan(TreatmentPlan plan)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        INSERT INTO TreatmentPlans (
                            IllnessName, Description, StartDate, EndDate, 
                            Status, MedicationNotes, TreatmentLogs, Symptoms,
                            ProgressPhotoIds
                        )
                        VALUES (
                            @IllnessName, @Description, @StartDate, @EndDate,
                            @Status, @MedicationNotes, @TreatmentLogs, @Symptoms,
                            @ProgressPhotoIds
                        );
                        SELECT last_insert_rowid();";
                    
                    command.Parameters.AddWithValue("@IllnessName", plan.IllnessName);
                    command.Parameters.AddWithValue("@Description", plan.Description ?? "");
                    command.Parameters.AddWithValue("@StartDate", plan.StartDate.ToString("o"));
                    command.Parameters.AddWithValue("@EndDate", plan.EndDate.HasValue ? plan.EndDate.Value.ToString("o") : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", (int)plan.Status);
                    command.Parameters.AddWithValue("@MedicationNotes", plan.MedicationNotes ?? "");
                    command.Parameters.AddWithValue("@TreatmentLogs", JsonSerializer.Serialize(plan.TreatmentLogs));
                    command.Parameters.AddWithValue("@Symptoms", JsonSerializer.Serialize(plan.Symptoms));
                    command.Parameters.AddWithValue("@ProgressPhotoIds", JsonSerializer.Serialize(plan.ProgressPhotoIds));
                    
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        /// <summary>
        /// Updates an existing treatment plan in the database
        /// </summary>
        /// <param name="plan">The treatment plan to update</param>
        public void UpdateTreatmentPlan(TreatmentPlan plan)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        UPDATE TreatmentPlans 
                        SET IllnessName = @IllnessName, 
                            Description = @Description, 
                            StartDate = @StartDate, 
                            EndDate = @EndDate,
                            Status = @Status, 
                            MedicationNotes = @MedicationNotes, 
                            TreatmentLogs = @TreatmentLogs,
                            Symptoms = @Symptoms,
                            ProgressPhotoIds = @ProgressPhotoIds
                        WHERE Id = @Id";
                    
                    command.Parameters.AddWithValue("@Id", plan.Id);
                    command.Parameters.AddWithValue("@IllnessName", plan.IllnessName);
                    command.Parameters.AddWithValue("@Description", plan.Description ?? "");
                    command.Parameters.AddWithValue("@StartDate", plan.StartDate.ToString("o"));
                    command.Parameters.AddWithValue("@EndDate", plan.EndDate.HasValue ? plan.EndDate.Value.ToString("o") : (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", (int)plan.Status);
                    command.Parameters.AddWithValue("@MedicationNotes", plan.MedicationNotes ?? "");
                    command.Parameters.AddWithValue("@TreatmentLogs", JsonSerializer.Serialize(plan.TreatmentLogs));
                    command.Parameters.AddWithValue("@Symptoms", JsonSerializer.Serialize(plan.Symptoms));
                    command.Parameters.AddWithValue("@ProgressPhotoIds", JsonSerializer.Serialize(plan.ProgressPhotoIds));
                    
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Deletes a treatment plan from the database
        /// </summary>
        /// <param name="planId">The ID of the treatment plan to delete</param>
        public void DeleteTreatmentPlan(int planId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "DELETE FROM TreatmentPlans WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", planId);
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Gets all treatment plans from the database
        /// </summary>
        /// <returns>A list of treatment plans</returns>
        public List<TreatmentPlan> GetAllTreatmentPlans()
        {
            var plans = new List<TreatmentPlan>();
            
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM TreatmentPlans ORDER BY StartDate DESC", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var plan = new TreatmentPlan
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IllnessName = reader["IllnessName"].ToString(),
                                Description = reader["Description"].ToString(),
                                StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                                Status = (TreatmentStatus)Convert.ToInt32(reader["Status"]),
                                MedicationNotes = reader["MedicationNotes"].ToString(),
                                TreatmentLogs = JsonSerializer.Deserialize<List<TreatmentLog>>(reader["TreatmentLogs"].ToString()),
                                Symptoms = JsonSerializer.Deserialize<List<Symptom>>(reader["Symptoms"].ToString()),
                                ProgressPhotoIds = JsonSerializer.Deserialize<List<int>>(reader["ProgressPhotoIds"].ToString())
                            };
                            
                            if (reader["EndDate"] != DBNull.Value)
                            {
                                plan.EndDate = DateTime.Parse(reader["EndDate"].ToString());
                            }
                            
                            plans.Add(plan);
                        }
                    }
                }
            }
            
            return plans;
        }
        
        /// <summary>
        /// Gets a specific treatment plan by its ID
        /// </summary>
        /// <param name="planId">The ID of the treatment plan to retrieve</param>
        /// <returns>The treatment plan, or null if not found</returns>
        public TreatmentPlan GetTreatmentPlanById(int planId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM TreatmentPlans WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", planId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var plan = new TreatmentPlan
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                IllnessName = reader["IllnessName"].ToString(),
                                Description = reader["Description"].ToString(),
                                StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                                Status = (TreatmentStatus)Convert.ToInt32(reader["Status"]),
                                MedicationNotes = reader["MedicationNotes"].ToString(),
                                TreatmentLogs = JsonSerializer.Deserialize<List<TreatmentLog>>(reader["TreatmentLogs"].ToString()),
                                Symptoms = JsonSerializer.Deserialize<List<Symptom>>(reader["Symptoms"].ToString()),
                                ProgressPhotoIds = JsonSerializer.Deserialize<List<int>>(reader["ProgressPhotoIds"].ToString())
                            };
                            
                            if (reader["EndDate"] != DBNull.Value)
                            {
                                plan.EndDate = DateTime.Parse(reader["EndDate"].ToString());
                            }
                            
                            return plan;
                        }
                    }
                }
            }
            
            return null;
        }
        
        #endregion
        
        #region Fish Photos
        
        /// <summary>
        /// Adds a new photo to the database
        /// </summary>
        /// <param name="photo">The photo to add</param>
        /// <returns>The ID of the newly added photo</returns>
        public int AddPhoto(FishPhoto photo)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        INSERT INTO FishPhotos (
                            Caption, DateTaken, FilePath, FileName,
                            Category, IsTreatmentPhoto, TreatmentPlanId
                        )
                        VALUES (
                            @Caption, @DateTaken, @FilePath, @FileName,
                            @Category, @IsTreatmentPhoto, @TreatmentPlanId
                        );
                        SELECT last_insert_rowid();";
                    
                    command.Parameters.AddWithValue("@Caption", photo.Caption ?? "");
                    command.Parameters.AddWithValue("@DateTaken", photo.DateTaken.ToString("o"));
                    command.Parameters.AddWithValue("@FilePath", photo.FilePath);
                    command.Parameters.AddWithValue("@FileName", photo.FileName);
                    command.Parameters.AddWithValue("@Category", photo.Category ?? "General");
                    command.Parameters.AddWithValue("@IsTreatmentPhoto", photo.IsTreatmentPhoto ? 1 : 0);
                    command.Parameters.AddWithValue("@TreatmentPlanId", photo.TreatmentPlanId.HasValue ? photo.TreatmentPlanId.Value : (object)DBNull.Value);
                    
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        /// <summary>
        /// Updates an existing photo in the database
        /// </summary>
        /// <param name="photo">The photo to update</param>
        public void UpdatePhoto(FishPhoto photo)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        UPDATE FishPhotos 
                        SET Caption = @Caption, 
                            DateTaken = @DateTaken, 
                            FilePath = @FilePath, 
                            FileName = @FileName,
                            Category = @Category, 
                            IsTreatmentPhoto = @IsTreatmentPhoto, 
                            TreatmentPlanId = @TreatmentPlanId
                        WHERE Id = @Id";
                    
                    command.Parameters.AddWithValue("@Id", photo.Id);
                    command.Parameters.AddWithValue("@Caption", photo.Caption ?? "");
                    command.Parameters.AddWithValue("@DateTaken", photo.DateTaken.ToString("o"));
                    command.Parameters.AddWithValue("@FilePath", photo.FilePath);
                    command.Parameters.AddWithValue("@FileName", photo.FileName);
                    command.Parameters.AddWithValue("@Category", photo.Category ?? "General");
                    command.Parameters.AddWithValue("@IsTreatmentPhoto", photo.IsTreatmentPhoto ? 1 : 0);
                    command.Parameters.AddWithValue("@TreatmentPlanId", photo.TreatmentPlanId.HasValue ? photo.TreatmentPlanId.Value : (object)DBNull.Value);
                    
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Deletes a photo from the database
        /// </summary>
        /// <param name="photoId">The ID of the photo to delete</param>
        public void DeletePhoto(int photoId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "DELETE FROM FishPhotos WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", photoId);
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Gets all photos from the database
        /// </summary>
        /// <returns>A list of photos</returns>
        public List<FishPhoto> GetAllPhotos()
        {
            var photos = new List<FishPhoto>();
            
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM FishPhotos ORDER BY DateTaken DESC", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var photo = new FishPhoto
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Caption = reader["Caption"].ToString(),
                                DateTaken = DateTime.Parse(reader["DateTaken"].ToString()),
                                FilePath = reader["FilePath"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Category = reader["Category"].ToString(),
                                IsTreatmentPhoto = Convert.ToBoolean(Convert.ToInt32(reader["IsTreatmentPhoto"]))
                            };
                            
                            if (reader["TreatmentPlanId"] != DBNull.Value)
                            {
                                photo.TreatmentPlanId = Convert.ToInt32(reader["TreatmentPlanId"]);
                            }
                            
                            photos.Add(photo);
                        }
                    }
                }
            }
            
            return photos;
        }
        
        /// <summary>
        /// Gets all photos for a specific treatment plan
        /// </summary>
        /// <param name="treatmentPlanId">The ID of the treatment plan</param>
        /// <returns>A list of photos associated with the treatment plan</returns>
        public List<FishPhoto> GetPhotosByTreatmentPlanId(int treatmentPlanId)
        {
            var photos = new List<FishPhoto>();
            
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM FishPhotos WHERE TreatmentPlanId = @TreatmentPlanId ORDER BY DateTaken DESC", connection))
                {
                    command.Parameters.AddWithValue("@TreatmentPlanId", treatmentPlanId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var photo = new FishPhoto
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Caption = reader["Caption"].ToString(),
                                DateTaken = DateTime.Parse(reader["DateTaken"].ToString()),
                                FilePath = reader["FilePath"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Category = reader["Category"].ToString(),
                                IsTreatmentPhoto = Convert.ToBoolean(Convert.ToInt32(reader["IsTreatmentPhoto"])),
                                TreatmentPlanId = Convert.ToInt32(reader["TreatmentPlanId"])
                            };
                            
                            photos.Add(photo);
                        }
                    }
                }
            }
            
            return photos;
        }
        
        /// <summary>
        /// Gets a specific photo by its ID
        /// </summary>
        /// <param name="photoId">The ID of the photo to retrieve</param>
        /// <returns>The photo, or null if not found</returns>
        public FishPhoto GetPhotoById(int photoId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM FishPhotos WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", photoId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var photo = new FishPhoto
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Caption = reader["Caption"].ToString(),
                                DateTaken = DateTime.Parse(reader["DateTaken"].ToString()),
                                FilePath = reader["FilePath"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Category = reader["Category"].ToString(),
                                IsTreatmentPhoto = Convert.ToBoolean(Convert.ToInt32(reader["IsTreatmentPhoto"]))
                            };
                            
                            if (reader["TreatmentPlanId"] != DBNull.Value)
                            {
                                photo.TreatmentPlanId = Convert.ToInt32(reader["TreatmentPlanId"]);
                            }
                            
                            return photo;
                        }
                    }
                }
            }
            
            return null;
        }
        
        #endregion
        
        #region Notes
        
        /// <summary>
        /// Adds a new note to the database
        /// </summary>
        /// <param name="note">The note to add</param>
        /// <returns>The ID of the newly added note</returns>
        public int AddNote(Note note)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        INSERT INTO Notes (Title, Content, CreatedDateTime, ModifiedDateTime, Tags)
                        VALUES (@Title, @Content, @CreatedDateTime, @ModifiedDateTime, @Tags);
                        SELECT last_insert_rowid();";
                    
                    command.Parameters.AddWithValue("@Title", note.Title);
                    command.Parameters.AddWithValue("@Content", note.Content);
                    command.Parameters.AddWithValue("@CreatedDateTime", note.CreatedDateTime.ToString("o"));
                    command.Parameters.AddWithValue("@ModifiedDateTime", note.ModifiedDateTime.ToString("o"));
                    command.Parameters.AddWithValue("@Tags", note.Tags ?? "");
                    
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        /// <summary>
        /// Updates an existing note in the database
        /// </summary>
        /// <param name="note">The note to update</param>
        public void UpdateNote(Note note)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        UPDATE Notes 
                        SET Title = @Title, 
                            Content = @Content, 
                            ModifiedDateTime = @ModifiedDateTime, 
                            Tags = @Tags
                        WHERE Id = @Id";
                    
                    command.Parameters.AddWithValue("@Id", note.Id);
                    command.Parameters.AddWithValue("@Title", note.Title);
                    command.Parameters.AddWithValue("@Content", note.Content);
                    command.Parameters.AddWithValue("@ModifiedDateTime", note.ModifiedDateTime.ToString("o"));
                    command.Parameters.AddWithValue("@Tags", note.Tags ?? "");
                    
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Deletes a note from the database
        /// </summary>
        /// <param name="noteId">The ID of the note to delete</param>
        public void DeleteNote(int noteId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "DELETE FROM Notes WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", noteId);
                    command.ExecuteNonQuery();
                }
            }
        }
        
        /// <summary>
        /// Gets all notes from the database
        /// </summary>
        /// <returns>A list of notes</returns>
        public List<Note> GetAllNotes()
        {
            var notes = new List<Note>();
            
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM Notes ORDER BY ModifiedDateTime DESC", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notes.Add(new Note
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Content = reader["Content"].ToString(),
                                CreatedDateTime = DateTime.Parse(reader["CreatedDateTime"].ToString()),
                                ModifiedDateTime = DateTime.Parse(reader["ModifiedDateTime"].ToString()),
                                Tags = reader["Tags"].ToString()
                            });
                        }
                    }
                }
            }
            
            return notes;
        }
        
        /// <summary>
        /// Gets a specific note by its ID
        /// </summary>
        /// <param name="noteId">The ID of the note to retrieve</param>
        /// <returns>The note, or null if not found</returns>
        public Note GetNoteById(int noteId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SQLiteCommand("SELECT * FROM Notes WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", noteId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Note
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Content = reader["Content"].ToString(),
                                CreatedDateTime = DateTime.Parse(reader["CreatedDateTime"].ToString()),
                                ModifiedDateTime = DateTime.Parse(reader["ModifiedDateTime"].ToString()),
                                Tags = reader["Tags"].ToString()
                            };
                        }
                    }
                }
            }
            
            return null;
        }
        
        #endregion
    }
}

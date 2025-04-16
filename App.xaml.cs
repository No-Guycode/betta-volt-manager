using System;
using System.Windows;
using System.IO;
using VoltBettaManager.Data;
using VoltBettaManager.Services;

namespace VoltBettaManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static DatabaseManager _databaseManager;
        private static NotificationService _notificationService;
        private static string _dataDirectory;

        public static DatabaseManager DatabaseManager => _databaseManager;
        public static NotificationService NotificationService => _notificationService;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize app data directory
            _dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VoltBettaManager");
            
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
            
            // Initialize the database manager
            _databaseManager = new DatabaseManager(Path.Combine(_dataDirectory, "volt.db"));
            
            // Initialize the notification service
            _notificationService = new NotificationService();
            
            // Create and load database
            _databaseManager.InitializeDatabase();
            
            // Set up notifications for any scheduled tasks
            _notificationService.SetupNotificationsFromDatabase();
            
            // Set up unhandled exception handling
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Log the exception
            string errorMessage = $"An unhandled exception occurred: {e.Exception.Message}\n{e.Exception.StackTrace}";
            File.AppendAllText(Path.Combine(_dataDirectory, "error.log"), $"[{DateTime.Now}] {errorMessage}\n");
            
            MessageBox.Show($"An error occurred in the application. The error has been logged.\n\nError details: {e.Exception.Message}", 
                "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            string errorMessage = $"A fatal error occurred: {(ex != null ? ex.Message + "\n" + ex.StackTrace : "Unknown error")}\nTerminating: {e.IsTerminating}";
            File.AppendAllText(Path.Combine(_dataDirectory, "error.log"), $"[{DateTime.Now}] {errorMessage}\n");
            
            MessageBox.Show($"A fatal error occurred in the application. The error has been logged.\n\nThe application will now close.", 
                "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        public static string GetAppDataPath()
        {
            return _dataDirectory;
        }

        public static string GetImageStoragePath()
        {
            string path = Path.Combine(_dataDirectory, "Images");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}

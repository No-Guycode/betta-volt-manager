using System;
using System.Windows;
using System.Windows.Controls;
using VoltBettaManager.Views;

namespace VoltBettaManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LogsView _logsView;
        private MaintenanceView _maintenanceView;
        private PlantsView _plantsView;
        private TreatmentsView _treatmentsView;
        private GalleryView _galleryView;
        private NotesView _notesView;
        
        private Button _currentActiveButton;

        public MainWindow()
        {
            InitializeComponent();
            InitializeViews();
            
            // Set default view
            contentArea.Content = _logsView;
            SetActiveButton(btnLogs);
        }

        private void InitializeViews()
        {
            _logsView = new LogsView();
            _maintenanceView = new MaintenanceView();
            _plantsView = new PlantsView();
            _treatmentsView = new TreatmentsView();
            _galleryView = new GalleryView();
            _notesView = new NotesView();
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string viewName = button.Tag.ToString();
                
                // Set active button style
                SetActiveButton(button);
                
                // Change the view based on the button clicked
                switch (viewName)
                {
                    case "Logs":
                        contentArea.Content = _logsView;
                        break;
                    case "Maintenance":
                        contentArea.Content = _maintenanceView;
                        break;
                    case "Plants":
                        contentArea.Content = _plantsView;
                        break;
                    case "Treatments":
                        contentArea.Content = _treatmentsView;
                        break;
                    case "Gallery":
                        contentArea.Content = _galleryView;
                        break;
                    case "Notes":
                        contentArea.Content = _notesView;
                        break;
                }
            }
        }

        private void SetActiveButton(Button button)
        {
            // Reset previous active button
            if (_currentActiveButton != null)
            {
                _currentActiveButton.Background = System.Windows.Media.Brushes.Transparent;
                _currentActiveButton.Foreground = (System.Windows.Media.Brush)FindResource("TextBrush");
                _currentActiveButton.FontWeight = FontWeights.Normal;
            }
            
            // Set current button as active
            button.Background = (System.Windows.Media.Brush)FindResource("PrimaryBrush");
            button.Foreground = System.Windows.Media.Brushes.White;
            button.FontWeight = FontWeights.Bold;
            
            // Store current active button
            _currentActiveButton = button;
        }
    }
}

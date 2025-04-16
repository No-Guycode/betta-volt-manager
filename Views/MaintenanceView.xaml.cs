using System.Windows.Controls;
using VoltBettaManager.ViewModels;

namespace VoltBettaManager.Views
{
    /// <summary>
    /// Interaction logic for MaintenanceView.xaml
    /// </summary>
    public partial class MaintenanceView : UserControl
    {
        private MaintenanceViewModel _viewModel;
        
        public MaintenanceView()
        {
            InitializeComponent();
            
            _viewModel = new MaintenanceViewModel();
            DataContext = _viewModel;
        }
    }
}

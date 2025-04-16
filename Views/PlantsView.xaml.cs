using System.Windows.Controls;
using VoltBettaManager.ViewModels;

namespace VoltBettaManager.Views
{
    /// <summary>
    /// Interaction logic for PlantsView.xaml
    /// </summary>
    public partial class PlantsView : UserControl
    {
        private PlantsViewModel _viewModel;
        
        public PlantsView()
        {
            InitializeComponent();
            
            _viewModel = new PlantsViewModel();
            DataContext = _viewModel;
        }
    }
}

using System.Windows.Controls;
using VoltBettaManager.ViewModels;

namespace VoltBettaManager.Views
{
    /// <summary>
    /// Interaction logic for TreatmentsView.xaml
    /// </summary>
    public partial class TreatmentsView : UserControl
    {
        private TreatmentsViewModel _viewModel;
        
        public TreatmentsView()
        {
            InitializeComponent();
            
            _viewModel = new TreatmentsViewModel();
            DataContext = _viewModel;
        }
    }
}

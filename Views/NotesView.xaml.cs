using System.Windows.Controls;
using VoltBettaManager.ViewModels;

namespace VoltBettaManager.Views
{
    /// <summary>
    /// Interaction logic for NotesView.xaml
    /// </summary>
    public partial class NotesView : UserControl
    {
        private NotesViewModel _viewModel;
        
        public NotesView()
        {
            InitializeComponent();
            
            _viewModel = new NotesViewModel();
            DataContext = _viewModel;
        }
    }
}

using System.Windows.Controls;
using VoltBettaManager.ViewModels;

namespace VoltBettaManager.Views
{
    /// <summary>
    /// Interaction logic for GalleryView.xaml
    /// </summary>
    public partial class GalleryView : UserControl
    {
        private GalleryViewModel _viewModel;
        
        public GalleryView()
        {
            InitializeComponent();
            
            _viewModel = new GalleryViewModel();
            DataContext = _viewModel;
        }
    }
}

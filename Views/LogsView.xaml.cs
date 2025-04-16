using System;
using System.Windows.Controls;
using VoltBettaManager.ViewModels;

namespace VoltBettaManager.Views
{
    /// <summary>
    /// Interaction logic for LogsView.xaml
    /// </summary>
    public partial class LogsView : UserControl
    {
        private LogsViewModel _viewModel;
        
        public LogsView()
        {
            InitializeComponent();
            
            _viewModel = new LogsViewModel();
            DataContext = _viewModel;
        }
    }
}

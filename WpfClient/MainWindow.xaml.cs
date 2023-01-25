using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfClient.ViewModels;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        public MainWindow MainView { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel = new MainWindowViewModel();
        }
        public MainWindow(MainWindow window, SVC.Client currentClient)
        {
            DataContext = _viewModel = new MainWindowViewModel();
            this.Window = window;
            this.CurrentClient = currentClient;
        }

        public MainWindow Window { get; }
        public SVC.Client CurrentClient { get; }
    }
}

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

namespace WpfClient.Views
{
    /// <summary>
    /// Interaction logic for ChatLayout.xaml
    /// </summary>
    public partial class ChatLayout : UserControl
    {
        ChatLayoutViewModel _viewModel;

        public ChatLayout()
        {
            InitializeComponent();
        }

        public ChatLayout(SVC.Client currentPerson)
        {
            InitializeComponent();
            DataContext = _viewModel = new ChatLayoutViewModel(currentPerson);
        }
    }
}

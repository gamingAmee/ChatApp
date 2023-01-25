using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfClient.SVC;
using WpfClient.Views;

namespace WpfClient.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindow _window { get; }
        public MainWindowViewModel()
        {
            _window = (MainWindow)Application.Current.MainWindow;
            SignIn signIn = new SignIn();
            _window.Main.Children.Add(signIn);
        }
    }
}

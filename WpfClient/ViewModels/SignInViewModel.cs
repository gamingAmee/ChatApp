using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfClient.SVC;
using WpfClient.Views;

namespace WpfClient.ViewModels
{
    public class SignInViewModel : BaseViewModel
    {
        private MainWindow _window;
        public SignInViewModel()
        {
            _window = (MainWindow)Application.Current.MainWindow;
            SignIn_AddClickCommand = new RelayCommand(SignIn_AddBtnClick, param => this.canExecute);
            SignIn_CloseClickCommand = new RelayCommand(SignIn_CloseBtnClick, param => this.canExecute);
        }
        private bool canExecute = true;
        public bool CanExecute
        {
            get
            {
                return this.canExecute;
            }

            set
            {
                if (this.canExecute == value)
                {
                    return;
                }

                this.canExecute = value;
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                SetProperty(ref _username, value);
            }
        }

        private ICommand _signIn_AddClickCommand;
        public ICommand SignIn_AddClickCommand
        {
            get { return _signIn_AddClickCommand; }
            set
            {
                _signIn_AddClickCommand = value;
            }
        }

        private async void SignIn_AddBtnClick(object obj)
        {
            if (!string.IsNullOrEmpty(_username))
            {
                this.localClient = new SVC.Client();
                this.localClient.Name = _username;
                _window.MainView = new MainWindow(_window, this.localClient);

                ConnectProxy();
                await proxy.ConnectAsync(this.localClient);

                _window.Main.Children.Clear();
                ChatLayout chatLayout = new ChatLayout(this.localClient);
                _window.Main.Children.Add(chatLayout);
            }
        }

        private ICommand _signIn_CloseClickCommand;
        public ICommand SignIn_CloseClickCommand
        {
            get { return _signIn_CloseClickCommand; }
            set
            {
                _signIn_CloseClickCommand = value;
            }
        }

        private void SignIn_CloseBtnClick(object obj)
        {
            Application.Current.Shutdown(0);
        }
    }
}


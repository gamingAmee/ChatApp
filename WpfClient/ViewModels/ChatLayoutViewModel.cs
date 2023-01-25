using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using WpfClient.SVC;
using System.Net;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.ServiceModel;

namespace WpfClient.ViewModels
{
    public class ChatLayoutViewModel : BaseViewModel
    {
        SVC.Client _currentClient;
        public ChatLayoutViewModel()
        {

        }

        public ChatLayoutViewModel(SVC.Client currentPerson)
        {
            SendMessageClickCommand = new RelayCommand(SendMessageBtnClick, param => this.canExecute);
            _currentClient = currentPerson;
        }

        public ObservableCollection<SVC.Client> Users { get; set; }

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

        private string _txtMessage;
        public string TxtMessage
        {
            get { return _txtMessage; }
            set
            {
                _txtMessage = value;
                SetProperty(ref _txtMessage, value);
            }
        }

        private ICommand _sendMessageClickCommand;
        public ICommand SendMessageClickCommand
        {
            get { return _sendMessageClickCommand; }
            set
            {
                _sendMessageClickCommand = value;
            }
        }

        private async void SendMessageBtnClick(object obj)
        {
            Message message = new Message();
            message.Content = _txtMessage;
            message.Time = DateTime.Now;
            message.Sender = _currentClient.Name;
            ConnectProxy();

            await proxy.SayAsync(message);
            await proxy.IsWritingAsync(null);
        }
    }
}

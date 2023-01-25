using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.ServiceModel.Channels;
using WpfClient.SVC;
using Message = WpfClient.SVC.Message;
using WpfClient.Views;

namespace WpfClient.ViewModels
{
    public class ChatViewModel : BaseViewModel, SVC.IChatCallback
    {
        private MainWindow _window;
        //public SVC.ChatClient proxy = null;
        //public SVC.Client receiver = null;
        //public SVC.Client localClient = null;
        public ObservableCollection<SVC.Client> Users { get; set; }
        public ChatViewModel()
        {
            _window = (MainWindow)Application.Current.MainWindow;
            SignIn_AddClickCommand = new RelayCommand(SignIn_AddBtnClick, param => this.canExecute);
            SignIn_CloseClickCommand = new RelayCommand(SignIn_CloseBtnClick, param => this.canExecute);
            SendMessageClickCommand = new RelayCommand(SendMessageBtnClick, param => this.canExecute);
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

        #region Proxy
        public void ConnectProxy()
        {
            InstanceContext context = new InstanceContext(this);
            proxy = new SVC.ChatClient(context);
            string servicePath = proxy.Endpoint.ListenUri.AbsolutePath;
            string serviceListenPort = proxy.Endpoint.Address.Uri.Port.ToString();

            proxy.Endpoint.Address = new EndpointAddress("net.tcp://" + _IpTxt + ":" + serviceListenPort + servicePath);

            proxy.Open();
        }
        #endregion

        #region ChatLayout
        private ObservableCollection<ListBoxItem> _users;
        public ObservableCollection<ListBoxItem> UsersList
        {
            get { return _users; }
            set
            {
                _users = value;
                SetProperty(ref _users, value);
            }
        }

        private ObservableCollection<ListBoxItem> _messages;
        public ObservableCollection<ListBoxItem> MessageList
        {
            get { return _messages; }
            set
            {
                _messages = value;
                SetProperty(ref _messages, value);
            }
        }

        private string _connectedWith;
        public string ConnectedWith
        {
            get { return _connectedWith; }
            set
            {
                _connectedWith = value;
                SetProperty(ref _connectedWith, value);
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
            message.Sender = localClient.Name;
            ConnectProxy();

            await proxy.SayAsync(message);
        }

        #endregion


        #region SignIn

        private string _IpTxt = "localhost";
        public string IpTxt
        {
            get { return _IpTxt; }
            set
            {
                _IpTxt = value;
                SetProperty(ref _IpTxt, value);
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
                ChatLayout chatLayout = new ChatLayout(this);
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

        #endregion

        #region ICallBackMembers

        public void IsWritingCallback(Client client)
        {
            throw new NotImplementedException();
        }

        public void Receive(Message msg)
        {
            if (_messages == null)
            {
                _messages = new ObservableCollection<ListBoxItem>(new List<ListBoxItem>());
            }
            foreach (SVC.Client c in this.OnlineClients.Values)
            {
                if (c.Name == msg.Sender)
                {
                    ListBoxItem item = MakeItem($"{c.Name} : {msg.Content}");
                    _messages.Add(item);
                }
            }
        }

        public void ReceiveWhisper(Message msg, Client receiver)
        {
            throw new NotImplementedException();
        }

        public void RefreshClients(List<Client> clients)
        {
            if (_users == null)
            {
                _users = new ObservableCollection<ListBoxItem>(new List<ListBoxItem>());
            }
            UsersList.Clear();
            OnlineClients.Clear();
            foreach (SVC.Client c in clients)
            {
                ListBoxItem item = MakeItem(c.Name);
                _users.Add(item);
                OnlineClients.Add(item, c);
            }
        }

        public void UserJoin(Client client)
        {
            ConnectedWith = $"Connected with {client.Name}";
        }

        public void UserLeave(Client client)
        {

        }
        #endregion

        private ListBoxItem MakeItem(string text)
        {
            ListBoxItem Item = new ListBoxItem();

            TextBlock txtblock = new TextBlock();
            txtblock.Text = text;
            txtblock.VerticalAlignment = VerticalAlignment.Center;

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(txtblock);

            Item.Content = panel;

            return Item;
        }

    }
}

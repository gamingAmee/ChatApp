using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfClient.SVC;
using static System.Net.Mime.MediaTypeNames;

namespace WpfClient.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, SVC.IChatCallback
    {
        public SVC.ChatClient proxy = null;
        public SVC.Client receiver = null;
        public SVC.Client localClient = null;

        public void ConnectProxy()
        {
            InstanceContext context = new InstanceContext(this);
            proxy = new SVC.ChatClient(context);
            string servicePath = proxy.Endpoint.ListenUri.AbsolutePath;
            string serviceListenPort = proxy.Endpoint.Address.Uri.Port.ToString();

            proxy.Endpoint.Address = new EndpointAddress("net.tcp://" + _IpTxt + ":" + serviceListenPort + servicePath);

            proxy.Open();
        }

        public Dictionary<ListBoxItem, SVC.Client> OnlineClients = new Dictionary<ListBoxItem, Client>();

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

        private ObservableCollection<ListBoxItem> _messageList;
        public ObservableCollection<ListBoxItem> MessageList
        {
            get { return _messageList; }
            set
            {
                _messageList = value;
                SetProperty(ref _messageList, value);
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

        #region INotifyPropertyChanged
        protected bool SetProperty<T>(ref T backingStore, T value,
           [CallerMemberName] string propertyName = "",
           Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region RelayCommand
        public class RelayCommand : ICommand
        {
            private Action<object> execute;

            private Predicate<object> canExecute;

            private event EventHandler CanExecuteChangedInternal;

            public RelayCommand(Action<object> execute)
                : this(execute, DefaultCanExecute)
            {
            }

            public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            {
                if (execute == null)
                {
                    throw new ArgumentNullException("execute");
                }

                if (canExecute == null)
                {
                    throw new ArgumentNullException("canExecute");
                }

                this.execute = execute;
                this.canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                    this.CanExecuteChangedInternal += value;
                }

                remove
                {
                    CommandManager.RequerySuggested -= value;
                    this.CanExecuteChangedInternal -= value;
                }
            }

            public bool CanExecute(object parameter)
            {
                return this.canExecute != null && this.canExecute(parameter);
            }

            public void Execute(object parameter)
            {
                this.execute(parameter);
            }

            public void OnCanExecuteChanged()
            {
                EventHandler handler = this.CanExecuteChangedInternal;
                if (handler != null)
                {
                    //DispatcherHelper.BeginInvokeOnUIThread(() => handler.Invoke(this, EventArgs.Empty));
                    handler.Invoke(this, EventArgs.Empty);
                }
            }

            public void Destroy()
            {
                this.canExecute = _ => false;
                this.execute = _ => { return; };
            }

            private static bool DefaultCanExecute(object parameter)
            {
                return true;
            }
        }
        #endregion

        #region ICallBackMembers

        public void IsWritingCallback(Client client)
        {
            throw new NotImplementedException();
        }

        public void Receive(Message msg)
        {
            foreach (SVC.Client c in this.OnlineClients.Values)
            {
                if (c.Name == msg.Sender)
                {
                    if (_messageList == null)
                    {
                        _messageList = new ObservableCollection<ListBoxItem>();
                    }
                    ListBoxItem item = MakeItem(msg.Sender + " : " + msg.Content);
                    _messageList.Add(item);
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
                _users = new ObservableCollection<ListBoxItem>(new List<ListBoxItem> { MakeItem("Test") });
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
            ConnectedWith = $"Connected with {localClient.Name}";
        }

        public void UserLeave(Client client)
        {
            
        }
        #endregion

        private ListBoxItem MakeItem(string text)
        {
            ListBoxItem item = new ListBoxItem();

            TextBlock txtblock = new TextBlock();
            txtblock.Text = text;
            txtblock.VerticalAlignment = VerticalAlignment.Center;

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(item);
            panel.Children.Add(txtblock);

            ListBoxItem bigItem = new ListBoxItem();
            bigItem.Content = panel;

            return bigItem;
        }

        private ScrollViewer FindVisualChild(DependencyObject obj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is ScrollViewer)
                {
                    return (ScrollViewer)child;
                }
                else
                {
                    ScrollViewer childOfChild = FindVisualChild(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
    }
}

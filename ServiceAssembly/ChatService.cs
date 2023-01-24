using ServiceAssembly.Entities;
using System.Collections.Generic;
using System.ServiceModel;

namespace ServiceAssembly
{

    [ServiceContract(CallbackContract = typeof(IChatCallback), SessionMode = SessionMode.Required)]
    public interface IChat
    {
        [OperationContract(IsInitiating = true)]
        bool Connect(Client client);

        [OperationContract(IsOneWay = true)]
        void Say(Message msg);

        [OperationContract(IsOneWay = true)]
        void Whisper(Message msg, Client receiver);

        [OperationContract(IsOneWay = true)]
        void IsWriting(Client client);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(Client client);
    }

    public interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void RefreshClients(List<Client> clients);

        [OperationContract(IsOneWay = true)]
        void Receive(Message msg);

        [OperationContract(IsOneWay = true)]
        void ReceiveWhisper(Message msg, Client receiver);

        [OperationContract(IsOneWay = true)]
        void IsWritingCallback(Client client);

        [OperationContract(IsOneWay = true)]
        void UserJoin(Client client);

        [OperationContract(IsOneWay = true)]
        void UserLeave(Client client);
    }


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ChatService : IChat
    {
        Dictionary<Client, IChatCallback> clients = new Dictionary<Client, IChatCallback>();

        List<Client> clientList = new List<Client>();

        public IChatCallback CurrentCallback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IChatCallback>();

            }
        }

        object syncObj = new object();

        private bool SearchClientsByName(string name)
        {
            foreach (Client c in clients.Keys)
            {
                if (c.Name == name)
                {
                    return true;
                }
            }
            return false;
        }



        #region IChat Members

        public bool Connect(Client client)
        {
            if (!clients.ContainsValue(CurrentCallback) && !SearchClientsByName(client.Name))
            {
                lock (syncObj)
                {
                    clients.Add(client, CurrentCallback);
                    clientList.Add(client);

                    foreach (Client key in clients.Keys)
                    {
                        IChatCallback callback = clients[key];
                        try
                        {
                            callback.RefreshClients(clientList);
                            callback.UserJoin(client);
                        }
                        catch
                        {
                            clients.Remove(key);
                            return false;
                        }

                    }

                }
                return true;
            }
            return false;
        }

        public void Say(Message msg)
        {
            lock (syncObj)
            {
                foreach (IChatCallback callback in clients.Values)
                {
                    callback.Receive(msg);
                }
            }
        }

        public void Whisper(Message msg, Client receiver)
        {
            foreach (Client rec in clients.Keys)
            {
                if (rec.Name == receiver.Name)
                {
                    IChatCallback callback = clients[rec];
                    callback.ReceiveWhisper(msg, rec);

                    foreach (Client sender in clients.Keys)
                    {
                        if (sender.Name == msg.Sender)
                        {
                            IChatCallback senderCallback = clients[sender];
                            senderCallback.ReceiveWhisper(msg, rec);
                            return;
                        }
                    }
                }
            }
        }

        public void IsWriting(Client client)
        {
            lock (syncObj)
            {
                foreach (IChatCallback callback in clients.Values)
                {
                    callback.IsWritingCallback(client);
                }
            }
        }

        public void Disconnect(Client client)
        {
            foreach (Client c in clients.Keys)
            {
                if (client.Name == c.Name)
                {
                    lock (syncObj)
                    {
                        this.clients.Remove(c);
                        this.clientList.Remove(c);
                        foreach (IChatCallback callback in clients.Values)
                        {
                            callback.RefreshClients(this.clientList);
                            callback.UserLeave(client);
                        }
                    }
                    return;
                }
            }
        }

        #endregion
    }
}

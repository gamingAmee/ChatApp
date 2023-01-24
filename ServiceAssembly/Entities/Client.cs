using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAssembly.Entities
{

    [DataContract]
    public class Client
    {
        private string _name;
        private int _userID;
        private DateTime _time;

        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        [DataMember]
        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }
        [DataMember]
        public DateTime Time
        {
            get { return _time; }
            set { _time = value; }
        }
    }
}

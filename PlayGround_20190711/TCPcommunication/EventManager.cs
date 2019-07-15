using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandler
{
    public class EventManager
    {
        //private static readonly EventManager instance = new EventManager();

        //public static EventManager Instance
        //{
        //    get { return instance; }
        //}

        public class RequestMatchingDataReceivedArgs : EventArgs
        {
            public MatchingData Data
            { get; set; }
        }
        public delegate void RequestMatchingEventHandler(RequestMatchingDataReceivedArgs e);
        public event RequestMatchingEventHandler RequestMatchingEvent;

        public void ReceiveRequestMatching(MatchingData message)
        {
            if(RequestMatchingEvent != null)
            {
                RequestMatchingDataReceivedArgs Parameter = new RequestMatchingDataReceivedArgs();
                Parameter.Data = message;

                RequestMatchingEvent(Parameter);
            }
        }
    }
}

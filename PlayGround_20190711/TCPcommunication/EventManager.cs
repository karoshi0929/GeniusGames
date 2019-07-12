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

        //public class RequstMatchingReceiveArgs : EventArgs
        //{
        //    public JoinGame Data
        //    { get; set; }
        //}

        //public delegate void RequestMatchingEventHandler(RequstMatchingReceiveArgs e);
        //public event RequestMatchingEventHandler RequestMatchingEvent;
        //public void ReceiveRequestMatching(JoinGame data)
        //{
        //    if(RequestMatchingEvent != null)
        //    {
        //        RequstMatchingReceiveArgs Parameter = new RequstMatchingReceiveArgs();
        //        Parameter.Data = data;
        //        this.RequestMatchingEvent(Parameter);
        //    }
        //}

        public delegate void JoinClientEventHandler(StartMatching message);
        public event JoinClientEventHandler RequestMatchingEvent;

        public void ReceiveRequestMatching(StartMatching message)
        {
            if(RequestMatchingEvent != null)
            {
                StartMatching Parameter = new StartMatching();
                Parameter = message;

                RequestMatchingEvent(Parameter);
            }
        }
    }
}

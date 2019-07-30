using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandler
{
    public class EventManager
    {
        private static readonly EventManager instance = new EventManager();

        public static EventManager Instance
        {
            get { return instance; }
        }

        /* ************************************************************************************ */
        public class LoginPacketReceivedArgs : EventArgs
        {
            public LoginPacket Data
            { get; set; }
        }

        public delegate void LoginPacketEventHandler(LoginPacketReceivedArgs e);
        public event LoginPacketEventHandler LoginPacketEvent;

        public void ReceiveLoginPacket(LoginPacket loginPacket)
        {
            LoginPacketReceivedArgs parameter = new LoginPacketReceivedArgs();
            parameter.Data = loginPacket;

            LoginPacketEvent(parameter);
        }

        /* ************************************************************************************ */
        public class MatchingPacketReceivedArgs : EventArgs
        {
            public MatchingPacket Data
            { get; set; }
        }

        public delegate void MatchingPacketEventHandler(MatchingPacketReceivedArgs e);
        public event MatchingPacketEventHandler MatchingPacketEvent;

        public void ReceiveMatchingPacket(MatchingPacket matchingPacket)
        {
            MatchingPacketReceivedArgs parameter = new MatchingPacketReceivedArgs();
            parameter.Data = matchingPacket;

            MatchingPacketEvent(parameter);
        }
        /* ************************************************************************************ */

        public class IndianPokerGamePacketReceivedArgs : EventArgs
        {
            public IndianPokerGamePacket Data
            { get; set; }
        }

        public delegate void IndianPokerGamePacketEventHandler(IndianPokerGamePacketReceivedArgs e);
        public event IndianPokerGamePacketEventHandler IndianPokerGamePacketEvent;

        public void ReceiveIndianPokerGamePacket(IndianPokerGamePacket indianPokerGamePacket)
        {
            IndianPokerGamePacketReceivedArgs parameter = new IndianPokerGamePacketReceivedArgs();
            parameter.Data = indianPokerGamePacket;

            IndianPokerGamePacketEvent(parameter);
        }

        /* ************************************************************************************ */
    }
}

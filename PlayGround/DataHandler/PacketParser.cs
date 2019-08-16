using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataHandler
{
    public class PacketParser
    {
        public static byte[] RawSerialize(object anything)
        {
            string strError;
            try
            {
                int rawsize = Marshal.SizeOf(anything);

                IntPtr buffer = Marshal.AllocHGlobal(rawsize);

                Marshal.StructureToPtr(anything, buffer, false);

                byte[] rawdatas = new byte[rawsize];

                Marshal.Copy(buffer, rawdatas, 0, rawsize);

                Marshal.FreeHGlobal(buffer);
                return rawdatas;

            }
            catch (Exception _ex)
            {
                strError = _ex.Message;
                return null;
            }
        }

        public static void PacketParsing(byte[] packet, Socket clientSocket = null)
        {
            switch (packet[0])
            {
                case (byte)Header.Login:
                    LoginPacket loginPacket = LoginPacket_ToStruct(packet);
                    EventManager.Instance.ReceiveLoginPacket(loginPacket, clientSocket);
                    break;
                case (byte)Header.Matching:
                    MatchingPacket matchingPacket = MatchingPacket_ToStruct(packet);
                    EventManager.Instance.ReceiveMatchingPacket(matchingPacket);
                    break;
                case (byte)Header.Game:
                    IndianPokerGamePacket indianPokerPacket = IndianPokerGame_ToStruct(packet);
                    EventManager.Instance.ReceiveIndianPokerGamePacket(indianPokerPacket);
                    break;
            }
        }
        public static LoginPacket LoginPacket_ToStruct(byte[] packet)
        {
            LoginPacket temp = new LoginPacket();
            object obj = (object)temp;
            PacketToStruct(packet, ref obj);
            temp = (LoginPacket)obj;
            return temp;
        }

        public static MatchingPacket MatchingPacket_ToStruct(byte[] packet)
        {
            MatchingPacket temp = new MatchingPacket();
            object obj = (object)temp;
            PacketToStruct(packet, ref obj);
            temp = (MatchingPacket)obj;
            return temp;
        }

        public static IndianPokerGamePacket IndianPokerGame_ToStruct(byte[] packet)
        {
            IndianPokerGamePacket temp = new IndianPokerGamePacket();
            object obj = (object)temp;
            PacketToStruct(packet, ref obj);
            temp = (IndianPokerGamePacket)obj;
            return temp;
        }

        public static void PacketToStruct(byte[] packet, ref object topicstruct)
        {
            int len = Marshal.SizeOf(topicstruct);
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.Copy(packet, 1, ptr, len);
            topicstruct = Marshal.PtrToStructure(ptr, topicstruct.GetType());
            Marshal.FreeHGlobal(ptr);
        }

        
    }
}

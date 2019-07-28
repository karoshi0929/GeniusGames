﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void PacketParsing(byte[] packet)
        {
            switch (packet[0])
            {


                //case (byte)KindOfGame.IndianPokser:
                //    MatchingData startmatching = JoinGame_ToStruct(packet);
                //    EventManager.Instance.ReceiveRequestMatching(startmatching);
                //    break;
                //case (byte)KindOfGame.MazeOfMemory:
                //    break;
                //case (byte)KindOfGame.RememberNumber:
                //    break;
                //case (byte)KindOfGame.FinishedAndSum:
                //    break;
            }
        }

        public static void PacketToStruct(byte[] packet, ref object topicstruct)
        {
            int len = Marshal.SizeOf(topicstruct);
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.Copy(packet, 2, ptr, len);
            topicstruct = Marshal.PtrToStructure(ptr, topicstruct.GetType());
            Marshal.FreeHGlobal(ptr);
        }

        public static MatchingData JoinGame_ToStruct(byte[] packet)
        {
            MatchingData temp = new MatchingData();
            object obj = (object)temp;
            PacketToStruct(packet, ref obj);
            temp = (MatchingData)obj;

            return temp;
        }
    }
}

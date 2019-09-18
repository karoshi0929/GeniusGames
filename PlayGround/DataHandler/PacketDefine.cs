using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataHandler
{
    public enum Header
    {
        Login = 0x01,
        Matching = 0x02,
        Game = 0x03,
        GameMotion = 0x04
    }

    public enum KindOfGame
    {
        IndianPokser = 0xA1,
        MazeOfMemory = 0xA2,
        RememberNumber = 0xA3,
        FinishedAndSum = 0xA4
    }

    public enum Matching
    {
        StartMatching = 0xB1,
        StopMatching = 0xB2,
        MatchComplete = 0xB3
    }

    public enum Betting
    {
        //다이 삥 따당 체크 쿼터 하프
        BettingCall = 0,
        BettingDie,
        BettingBbing,
        BettingDouble,
        BettingCheck,
        BettingQueter,
        BettingHalf
    }

    /***************************** 클라에게서 받은 패킷 ********************************/
    public struct LoginPacket
    {
        //유저 아이디
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string clientID;

        //로그인 여부 true = 1 ,false = 0
        public bool isLogin;

        //메세지 수신 체크
        public byte Ack;
    }

    public struct MatchingPacket
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string clientID;

        //시작할 게임 종류 아이디
        public byte GameID;

        //매칭 시작 및 매칭 취소 메세지
        //StartMatching, StopMatching
        public byte matchingMsg;

        //메세지 수신을 받았는지 못받았는지 체크 true = 1, false 0
        public byte Ack;

        //매칭이 성사 되었다면 서버에서 true 송신
        public bool matchingComplete;
    }

    public struct HandleGamePacket
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string clientID;

        //로딩 완료
        public bool loadingComplete;

        //게임시작
        public bool startGame;
        public int betting;
        public short card;
        public short playerTurn;
    }

    public struct IndianPokerGamePacket
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string clientID;

        public int betting;
        public short card;
        public short playerTurn;
    }
    /***********************************************************************************/


   

    /***********************************************************************************/

    public class PacketDefine
    {
        public static byte[] MakePacket(Header header, object Data)
        {
            byte[] temp = new byte[] { };
            temp = PacketParser.RawSerialize(Data);
            byte[] outpacket = new byte[temp.Length + 1];
            outpacket[0] = (byte)header;
            Array.Copy(temp, 0, outpacket, 1, temp.Length);

            //packet 검사 코드
            //outpakcet[outpacket.Length - 1] = Crc8.ComputeChecksum(outpacket);

            return outpacket;
        }
    }
}

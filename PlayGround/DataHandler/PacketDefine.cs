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
        //다이 따당 체크 쿼터 하프
        BettingCall = 0,
        BettingDie,
        BettingDouble,
        BettingCheck,
        BettingQueter,
        BettingHalf
    }

    /***************************** 클라에게서 받은 패킷 ********************************/
    public struct LoginPacket
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string clientID;

        /// <summary>
        /// 로그인 여부
        /// </summary>
        public bool isLogin;

        /// <summary>
        /// 유저 아이디 중복 검사
        /// </summary>
        public bool isDuplication;
    }

    public struct MatchingPacket
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string clientID;

        
        ///시작할 게임 종류 아이디
        public byte GameID;

        //매칭 시작 및 매칭 취소 메세지
        //StartMatching, StopMatching
        public byte matchingMsg;

        //메세지 수신을 받았는지 못받았는지 체크 true = 1, false 0
        public byte Ack;

        /// <summary>
        /// 매칭이 성사 되면 서버에서 true 송신
        /// </summary>
        public bool matchingComplete;
    }

    public struct HandleGamePacket
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string clientID;

        /// <summary>
        /// 로딩완료 변수
        /// </summary>
        public bool loadingComplete;

        /// <summary>
        /// 현재 게임중 상태
        /// </summary>
        public bool startGame;

        public short MyIndex;
        public short MyCard;
        public short OtherPlayerCard;
        public short playerTurn;
        public int MyMoney;
        public int OtherPlayerMoney;
        public int TotalBettingMoney;
    }

    public struct IndianPokerGamePacket
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string clientID;

        public int Betting;
        public int BettingMoney;
        public int OtherPlayerMoney;

        public int MyMoney;
        public short VictoryUser;
        public short playerTurn;
        public short playerIndex;
        //public short Mycard;
        //public short OtherPlayerCard;
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

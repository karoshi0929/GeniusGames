using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACTD.NET
{
    /// <summary>
    /// 클라이언트 연결 상태
    /// </summary>
    public enum ConnectStatus
    {
        /// <summary>
        /// Success
        /// </summary>
        SUCCESS = 0,
        /// <summary>
        /// Failed by time-out
        /// </summary>
        FAIL_TIME_OUT,
        /// <summary>
        /// Failed due to connection already exists
        /// </summary>
        FAIL_ALREADY_CONNECTED,
        /// <summary>
        /// Failed due to unknown error
        /// </summary>
        FAIL_SOCKET_ERROR,
        /// <summary>
        /// None
        /// </summary>
        None
    }

    /// <summary>
    /// 서버 동작 상태
    /// </summary>
    public enum StartStatus
    {
        /// <summary>
        /// Success
        /// </summary>
        SUCCESS = 0,
        /// <summary>
        /// Failed due to server already started
        /// </summary>
        FAIL_ALREADY_STARTED,
        /// <summary>
        /// Failed due to socket error
        /// </summary>
        FAIL_SOCKET_ERROR
    }

    /// <summary>
    /// 메시지 전송 상태
    /// </summary>
    public enum SendStatus : uint
    {
        /// <summary>
        /// Success
        /// </summary>
        SUCCESS = 0,
        /// <summary>
        /// Failed due to socket error
        /// </summary>
        FAIL_SOCKET_ERROR,
        /// <summary>
        /// Failed due to no connection exists
        /// </summary>
        FAIL_NOT_CONNECTED,
        /// <summary>
        /// Failed due to invalid packet
        /// </summary>
        FAIL_INVALID_PACKET,
        /// <summary>
        /// Failed due to connection closing
        /// </summary>
        FAIL_CONNECTION_CLOSING,
    }

    /// <summary>
    /// Enumerator for packet type
    /// </summary>
    public enum PacketType
    {
        /// <summary>
        /// Header type
        /// </summary>
        HEADER = 0,
        /// <summary>
        /// Receive type
        /// </summary>
        DATA
    }

    /// <summary>
    /// IP End-point type
    /// </summary>
    public enum IPEndPointType
    {
        /// <summary>
        /// local
        /// </summary>
        LOCAL = 0,
        /// <summary>
        /// remote
        /// </summary>
        REMOTE
    }

    public enum ServerType
    {
        /// <summary>
        /// Brighter Radar
        /// </summary>
        RADAR = 0,
        /// <summary>
        /// EOIR
        /// </summary>
        EOIR,
        EOIRByPassServer,

    }
    public enum eRadarVer
    {
        Version14 = 0,
        Version18
    }
    /// <summary>
    /// 서버 환경설정(IP, Port) 정보
    /// </summary>
    public class ServerConf
    {
        /// <summary>
        /// Default hostname (localhost)
        /// </summary>
        public const String DEFAULT_HOSTNAME = "localhost";
        /// <summary>
        /// Default port (80808)
        /// </summary>
        public const String DEFAULT_PORT = "34310";
    }
}

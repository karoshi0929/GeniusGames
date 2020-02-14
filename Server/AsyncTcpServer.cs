using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace ACTD.NET
{
    public sealed class AsyncTcpServer : ThreadManaged, INetworkServer
    {
        private String m_ip = ServerConf.DEFAULT_HOSTNAME;        
        private String m_port = ServerConf.DEFAULT_PORT;
        
        private TcpListener m_listener = null;        
        private ServerOfs m_serverOfs = null;        
        private INetworkServerCallback m_callBackObj = null;        
        private Object m_generalLock = new Object();
        
        private Object m_listLock = new Object();        
        private List<AsyncTcpSocket> m_socketList = new List<AsyncTcpSocket>();

        private ServerType m_serverType;
        public AsyncTcpServer()
            : base()
        {
        }
        
        public AsyncTcpServer(AsyncTcpServer b)
            : base(b)
        {
            m_port = b.m_port;
            m_serverOfs = b.m_serverOfs;
        }

        ~AsyncTcpServer()
        {
            if (IsServerStarted())
                StopServer();
        }
        
        public String GetIPAddress()
        {
            return m_ip.ToString();
        }

        public String GetPort()
        {
            return m_port;
        }
        
        [Serializable]
        private class CallbackException : Exception
        {
            public CallbackException()
                : base()
            {
            }

            public CallbackException(String message)
                : base(message)
            {
            }
        }
        
        protected override void execute()
        {
            StartStatus status = StartStatus.FAIL_SOCKET_ERROR;
            try
            {
                lock (m_generalLock)
                {
                    if (IsServerStarted())
                    {
                        status = StartStatus.FAIL_ALREADY_STARTED;
                        throw new CallbackException();
                    }

                    m_callBackObj = m_serverOfs.callBackObj;
                    m_ip = m_serverOfs.ip;
                    m_port = m_serverOfs.port;

                    if (m_port == null || m_port.Length == 0)
                    {
                        m_port = ServerConf.DEFAULT_PORT;
                    }

                    m_listener = new TcpListener(IPAddress.Parse(m_ip), Convert.ToInt32(m_port));
                    m_listener.Start();
                    m_listener.BeginAcceptTcpClient(new AsyncCallback(onAccept), this);
                }
            }
            catch (CallbackException)
            {
                m_callBackObj.OnServerStarted(this, status);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                if (m_listener != null)
                    m_listener.Stop();
                m_listener = null;
                m_callBackObj.OnServerStarted(this, StartStatus.FAIL_SOCKET_ERROR);
                return;
            }

            m_callBackObj.OnServerStarted(this, StartStatus.SUCCESS);
        }

        private void onAccept(IAsyncResult result)
        {
            AsyncTcpServer server = result.AsyncState as AsyncTcpServer;
            TcpClient client = null;
            try
            {
                client = server.m_listener.EndAcceptTcpClient(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                if (client != null)
                    client.Close();
                server.StopServer();
                return;
            }

            try
            {
                server.m_listener.BeginAcceptTcpClient(new AsyncCallback(onAccept), server);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                if (client != null)
                    client.Close();
                server.StopServer();
                return;
            }

            AsyncTcpSocket socket = new AsyncTcpSocket(client, server);
            INetworkSocketCallback socketCallbackObj = server.m_callBackObj.OnAccept(server, socket.GetIPInfo());
            if (socketCallbackObj == null)
            {
                socket.Disconnect();
            }
            else
            {
                socket.SetSocketCallback(socketCallbackObj);
                socket.Start();
                lock (server.m_listLock)
                {
                    server.m_socketList.Add(socket);
                }
            }
        }
        
        public void StartServer(ServerOfs ops)
        {
            if (ops == null)
            {
                ops = ServerOfs.defaultServerOfs;
            }

            if (ops.callBackObj == null)
            {
                throw new NullReferenceException("callBackObj is null!");
            }

            lock (m_generalLock)
            {
                m_serverOfs = ops;
            }

            Start();
        }
        
        public void StopServer()
        {
            lock (m_generalLock)
            {
                if (!IsServerStarted())
                    return;
                m_listener.Stop();
                m_listener = null;
            }

            ShutdownAllClient();

            if (m_callBackObj != null)
            {
                m_callBackObj.OnServerStopped(this);
            }
        }
        
        public bool IsServerStarted()
        {
            if (m_listener != null)
            {
                return true;
            }

            return false;
        }

        public void ShutdownAllClient()
        {
            lock (m_listLock)
            {
                for (int trav = m_socketList.Count - 1; trav >= 0; trav--)
                {
                    m_socketList[trav].Disconnect();
                }

                m_socketList.Clear();
            }
        }


        public List<AsyncTcpSocket> GetClientSocketList()
        {
            lock (m_listLock)
            {
                return new List<AsyncTcpSocket>(m_socketList);
            }
        }

        public bool DetachClient(AsyncTcpSocket clientSocket)
        {
            lock (m_listLock)
            {
                return m_socketList.Remove(clientSocket);
            }
        }
        public void SetServerType(ServerType type)
        {
            m_serverType = type;
        }

        public ServerType GetServerType()
        {
            return m_serverType;
        }
    }
}

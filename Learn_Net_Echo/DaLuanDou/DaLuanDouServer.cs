using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Learn_Net_Echo.DLD
{
    public class DaLuanDouServer
    {
        public class ClientState
        {
            public Socket socket;
            public byte[] readBuff = new byte[1024];
            
            public float x = 0;
            public float y = 0;
            public float z = 0;
        }
        
        public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
        private Socket socket;
        
        //使用Select接受和分发消息
        public void StartServer()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 8888);
            socket.Bind(ipEndPoint);
            socket.Listen(10);
            Console.WriteLine("大乱斗服务器启动成功！");

            List<Socket> checkReads = new List<Socket>();

            while (true)
            {
                checkReads.Clear();
                checkReads.Add(socket);
                checkReads.AddRange(clients.Values.Select(client => client.socket));
                
                Socket.Select(checkReads, null, null, 1000);
                foreach (Socket s in checkReads)
                {
                    if (s == socket)
                    {
                        ReadListenfd(s);
                    }
                    else
                    {
                        ReadClientfd(s);
                    }
                }
            }
        }
        
        private bool ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];
            //接收
            int count = 0;
            try
            {
                count = clientfd.Receive(state.readBuff);
            }
            catch (SocketException ex)
            {
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Receive SocketException " + ex.ToString());
                return false;
            }
            
            //客户端关闭
            if (count == 0)
            {
                MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
                object[] ob = { state };
                mei.Invoke(null, ob);
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Socket Close");
                return false;
            }
            //广播
            string recvStr =
                System.Text.Encoding.Default.GetString(state.readBuff, 0, count);
            Console.WriteLine("Receive--->" + recvStr);
            string[] split = recvStr.Split('|');
            var msgName = split[0];
            var msgArg = split[1];
            string funcName = $"Msg{msgName}";
            MethodInfo mi = typeof(MsgHandler).GetMethod(funcName);
            Console.WriteLine(mi);
            
            try
            {
                object[] obs = { state ,msgArg};
                mi.Invoke(null, obs);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            //改为消息处理其控制
            // string sendStr = recvStr;
            // byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
            // foreach (ClientState cs in clients.Values)
            // {
            //     cs.socket.Send(sendBytes);
            // }
            return true;
        }

        private void ReadListenfd(Socket listenfd)
        {
            Console.WriteLine("Accept");
            Socket clientfd = listenfd.Accept();
            ClientState state = new ClientState();
            state.socket = clientfd;
            clients.Add(clientfd, state);
        }
        
        public static void SendMsg(ClientState stage, string msg)
        {
            var msgBytes = System.Text.Encoding.Default.GetBytes(msg);
            stage.socket.Send(msgBytes);
        }


    }
}
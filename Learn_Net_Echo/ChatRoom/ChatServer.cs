using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Learn_Net_Echo.ChatRoom
{
    public class ChatServer
    {
        public class ClientStage
        {
            public Socket ClientSocket;
            public byte[] Buffer = new byte[1024];
        }
        static string GetLocalIPv4()
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4
                {
                    return ip.ToString();
                }
            }
            return "未找到本地IPv4地址";
        }
        private Socket socket;
        private Dictionary<Socket, ClientStage> clients = new Dictionary<Socket, ClientStage>();

        public void StartServer()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ip = GetLocalIPv4();
            BindIpAndPort(ip, 8888);
        }

        public void BindIpAndPort(string ip, int port)
        {
            Console.WriteLine($"IP:{ip}:{port}");
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            socket.Bind(ipEndPoint);
            Listen();
        }

        public void Listen()
        {
            socket.Listen(10);
            Console.WriteLine("服务器启动成功");
            Accept();
        }

        public void Accept()
        {
            
            // socket.BeginAccept(AcceptCallBack, socket);
            
            //TODO: 这里使用Select 进行监听
            List<Socket> checkRead = new List<Socket>();
            while (true)
            {
                checkRead.Clear();
                checkRead.Add(socket);

                checkRead.AddRange(clients.Keys);
                
                Socket.Select(checkRead,null,null,1000);

                foreach (var s in checkRead)
                {
                    if (s == socket)
                    {
                        ReadServer(s);
                    }
                    else
                    {
                        ReadClient(s);
                    }
                }
            }
        }


        private void ReadServer(Socket serverSocket)
        {
            Console.WriteLine("Accept");
            var clientSocket =  serverSocket.Accept();
            var clientState = new ClientStage
            {
                ClientSocket = clientSocket
            };
            clients.Add(clientSocket, clientState);
            OnLineCallBack();
        }

        private void ReadClient(Socket clientSocket)
        {
            var clientStage = clients[clientSocket];

            var count = clientSocket.Receive(clientStage.Buffer);

            if (count ==0)
            {
                clientSocket.Close();
                clients.Remove(clientSocket);
                OffLineCallBack();
                return;
            }

            string receMsg = Encoding.UTF8.GetString(clientStage.Buffer, 0,count);

            Console.WriteLine($"recvMsg:{receMsg}");

            string sendMsg = $"{clientSocket.RemoteEndPoint}:{receMsg}";
            
            BroadCast(EncodeMessage(true, sendMsg));

        }



        private void AcceptCallBack(IAsyncResult ar)
        {
            var server = (Socket)ar.AsyncState;
            var client = server.EndAccept(ar);
            var clientStage = new ClientStage();
            clientStage.ClientSocket = client;
            clients.Add(client, clientStage);
            OnLineCallBack();
            client.BeginReceive(clientStage.Buffer, 0, clientStage.Buffer.Length, SocketFlags.None, ReceiveCallBack,
                clientStage);
            //再次启动Accept
            server.BeginAccept(AcceptCallBack, server);
        }

        /// <summary>
        /// 有玩家上线时广播当前在线人数
        /// </summary>
        /// <param name="client"></param>
        private void OnLineCallBack()
        {
            BroadCast(EncodeMessage(false, clients.Count.ToString()));
        }

        /// <summary>
        /// 有玩家下线时广播当前在线人数
        /// </summary>
        /// <param name="client"></param>
        private void OffLineCallBack()
        {
            BroadCast(EncodeMessage(false, clients.Count.ToString()));
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            var clientStage = (ClientStage)ar.AsyncState;
            var client = clientStage.ClientSocket;
            var count = client.EndReceive(ar);
            if (count > 0)
            {
                var str = Encoding.UTF8.GetString(clientStage.Buffer, 0, count);
                str = str.Substring(0, str.Length - 1);
                Console.WriteLine($"{client.RemoteEndPoint} : {str}");
                var returnStr = $"{client.RemoteEndPoint} : {str}";
                BroadCast(EncodeMessage(true, returnStr));
                
                // BroadCast(EncodeMessage(false, clients.Count.ToString()));
                // SendMessage(client,str);
                client.BeginReceive(clientStage.Buffer, 0, clientStage.Buffer.Length, SocketFlags.None, ReceiveCallBack,
                    clientStage);
            }
            else if (count == 0)
            {
                clients.Remove(client);
                OffLineCallBack();
                client.Close();
                Console.WriteLine("Client socket closed");
                return;
            }
        }

        private byte[] EncodeMessage(bool isUser, string message)
        {
            string sendMsg;
            if (isUser)
            {
                sendMsg = "[USER]" + message;
            }
            else
            {
                sendMsg = "[SYSTEM]" + message;
            }

            Console.WriteLine($"服务器向所有客户端转发：{sendMsg}");
            
            byte[] msgByte = Encoding.UTF8.GetBytes(sendMsg);
            
            Int16 len = (Int16)msgByte.Length; 
            
            var lens = BitConverter.GetBytes(len);
            
            return lens.Concat(msgByte).ToArray();
            
            // short len = (short)msgByte.Length;
            // using (var ms = new MemoryStream())
            // using (var bw = new BinaryWriter(ms))
            // {
            //     // 写入消息长度（2字节，大端序）
            //     bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(len)));
            //     // 写入消息内容
            //     bw.Write(msgByte);
            //     return ms.ToArray();
            // }
        }


        /// <summary>
        /// 广播给所有在线的用户
        /// </summary>
        /// <param name="data"></param>
        private void BroadCast(byte[] data)
        {
            Console.WriteLine($"Server --> Client:{Encoding.UTF8.GetString(data)}");
            foreach (var client in clients)
            {
                try
                {
                    client.Value.ClientSocket.Send(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}
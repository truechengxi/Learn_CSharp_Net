using System.Net;
using System.Net.Sockets;

namespace Learn_Net_Echo
{
    public class AsyncServer
    {
        private Socket socket;
        
        public void StartServer()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
        }

        public void BindIpAndPort(string ip, int port )
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            socket.Bind(ipEndPoint);
        }

        public void Listen()
        {
            socket.Listen(10);
        }

        public void Accept()
        {
            
        }
    }
}
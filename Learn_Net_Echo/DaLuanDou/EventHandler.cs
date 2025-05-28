using System;
using static Learn_Net_Echo.DLD.DaLuanDouServer;

namespace Learn_Net_Echo.DLD
{
    public class EventHandler
    {
        public static void OnDisconnect(ClientState clientState)
        {
            string desc = clientState.socket.RemoteEndPoint.ToString();
            string sendStr = "Leave|" + desc + ",";
            foreach (ClientState cs in clients.Values){
                SendMsg(cs, sendStr);
            }
            Console.WriteLine($"{clientState.socket.RemoteEndPoint}:OnDisconnect");
        }
    }
}
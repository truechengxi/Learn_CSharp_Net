using System;
using System.Text;
using static Learn_Net_Echo.DLD.DaLuanDouServer;

namespace Learn_Net_Echo.DLD
{
    public class MsgHandler
    {
        public static void MsgEnter(ClientState c,string msg)
        {
            Console.WriteLine("MsgEnter"+ msg);
            var splitMsg = msg.Split(',');
            var Adr = splitMsg[0];
            var x = float.Parse(splitMsg[1]);
            var y = float.Parse(splitMsg[2]);
            var z = float.Parse(splitMsg[3]);
            c.x = x;
            c.y = y;
            c.z = z;
            //广播
            var sendStr = "Enter|" + msg;
            foreach (var cs in clients.Values)
            {
                SendMsg(cs,sendStr);
            }
        }
        
        public static void MsgList(ClientState c,string msg)
        {
            Console.WriteLine("MsgList"+ msg);
            StringBuilder sb = new StringBuilder();
            sb.Append("List|");
            foreach (var cs in clients.Values)
            {
                sb.Append(cs.socket.RemoteEndPoint.ToString());
                sb.Append(",");
                sb.Append(cs.x.ToString());
                sb.Append(",");
                sb.Append(cs.y.ToString());
                sb.Append(",");
                sb.Append(cs.z.ToString());
                sb.Append("/"); //注意按照 ‘/’ 切割时会在最后增加一个空元素
            }

            Console.WriteLine(sb.ToString());
            SendMsg(c,sb.ToString());
        }

        public static void MsgMove(ClientState c,string msg)
        {
            Console.WriteLine("MsgMove"+ msg);
            
            var sendStr = "Move|" + msg;
            foreach (var cs in clients.Values)
            {
                SendMsg(cs,sendStr);
            }
        }
    }
}
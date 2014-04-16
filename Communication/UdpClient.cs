using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    public class UdpClient
    {
        Socket  _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint _endPoint;

        public event EventHandler MessageSent;

        public UdpClient(string address, string port)
        { 
            int p = int.Parse(port);
            IPAddress ip = IPAddress.Parse(address);
            _endPoint = new IPEndPoint(ip, p);
            _socket.Connect(_endPoint);
        }

        public void SendUdp(string msg)
        {
            byte[] sendBuffer = Encoding.ASCII.GetBytes(msg);

            var resultByteCount = _socket.Send(sendBuffer);

            if (resultByteCount == msg.Length)
            {
                if (MessageSent != null)
                {
                    MessageSent.Invoke(msg, EventArgs.Empty);
                }
            }
        }
    }
}

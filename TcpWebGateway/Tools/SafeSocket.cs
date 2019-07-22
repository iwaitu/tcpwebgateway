using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpWebGateway.Tools
{
    public static class SafeSocket
    {
        public static Socket ConnectSocket(IPEndPoint ipe)
        {
           
            Socket tempSocket =
                    new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            tempSocket.Connect(ipe);

            if (tempSocket.Connected)
            {
                return tempSocket;
            }
            else
            {
                return null;
            }
        }

    }
}

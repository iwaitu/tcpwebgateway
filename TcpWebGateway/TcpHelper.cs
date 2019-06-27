using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpWebGateway
{
    public static class TcpHelper
    {
        public static int GetStatus(int id)
        {
            int retPercent = 0;
            using (TcpClient client = new TcpClient("192.168.50.17", 26))
            using (NetworkStream stream = client.GetStream())
            {
                byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x55, 0x01, (byte)id, 0x01, 0x02, 0x01 };
                stream.Write(data, 0, data.Length);

                data = new Byte[12];
                Int32 ret = stream.Read(data, 0, data.Length);
                retPercent = data[11];

                stream.Close();
                client.Close();
            }
            return retPercent;
        }

        public static void SetStatus(int id,int value)
        {
            if(value <1 || value > 99)
            {
                return;
            }
            using (TcpClient client = new TcpClient("192.168.50.17", 26))
            using (NetworkStream stream = client.GetStream())
            {
                byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x55, 0x01, (byte)id, 0x03, 0x04, (byte)value };
                stream.Write(data, 0, data.Length);

                data = new Byte[12];
                Int32 ret = stream.Read(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
        }

        public static void Open(int id)
        {
            using (TcpClient client = new TcpClient("192.168.50.17", 26))
            using (NetworkStream stream = client.GetStream())
            {
                byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x55, 0x01, (byte)id, 0x03, 0x01 };
                stream.Write(data, 0, data.Length);

                data = new Byte[12];
                Int32 ret = stream.Read(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
        }

        public static void Close(int id)
        {
            using (TcpClient client = new TcpClient("192.168.50.17", 26))
            using (NetworkStream stream = client.GetStream())
            {
                byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x55, 0x01, (byte)id, 0x03, 0x02 };
                stream.Write(data, 0, data.Length);

                data = new Byte[12];
                Int32 ret = stream.Read(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
        }

        public static void Stop(int id)
        {
            using (TcpClient client = new TcpClient("192.168.50.17", 26))
            using (NetworkStream stream = client.GetStream())
            {
                byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x55, 0x01, (byte)id, 0x03, 0x03 };
                stream.Write(data, 0, data.Length);

                data = new Byte[12];
                Int32 ret = stream.Read(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
        }
    }
}

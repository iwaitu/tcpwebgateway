using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpWebGateway.Services
{
    public class TcpHelper : IDisposable
    {
        private TcpClient _clientCurtain;
        private TcpClient _clientHailin;
        private NetworkStream _streamCurtain;
        private NetworkStream _streamHailin;

        public TcpHelper()
        {
            _clientCurtain = new TcpClient("192.168.50.17", 26);
            _clientHailin = new TcpClient("192.168.50.17", 23);
            _streamCurtain = _clientCurtain.GetStream();
            _streamHailin = _clientHailin.GetStream();

        }
        
        public async Task<int> GetCurtainStatus(int id)
        {
            
            int retPercent = 0;
            byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x55, 0x01, (byte)id, 0x01, 0x02, 0x01 };
            await _streamCurtain.WriteAsync(data, 0, data.Length);
            data = new Byte[12];
            Int32 ret = await _streamCurtain.ReadAsync(data, 0, data.Length);
            retPercent = data[11];
            await _streamCurtain.FlushAsync();
            return retPercent;
        }

        public async Task SetCurtainStatus(int id,int value)
        {
            if(value <1 )
            {
                value = 1;
            }
            if(value >99 )
            {
                value = 99;
            }
            byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x55, 0x01, (byte)id, 0x03, 0x04, (byte)value };
            await _streamCurtain.WriteAsync(data, 0, data.Length);

            data = new Byte[12];
            Int32 ret = await _streamCurtain.ReadAsync(data, 0, data.Length);
            await _streamCurtain.FlushAsync();
        }

        public async Task OpenCurtain(int id)
        {
            byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x55, 0x01, (byte)id, 0x03, 0x01 };
            await _streamCurtain.WriteAsync(data, 0, data.Length);

            data = new Byte[12];
            Int32 ret = await _streamCurtain.ReadAsync(data, 0, data.Length);
            await _streamCurtain.FlushAsync();
        }

        public async Task CloseCurtain(int id)
        {
            byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x55, 0x01, (byte)id, 0x03, 0x02 };
            await _streamCurtain.WriteAsync(data, 0, data.Length);

            data = new Byte[12];
            Int32 ret = await _streamCurtain.ReadAsync(data, 0, data.Length);
            await _streamCurtain.FlushAsync();
        }

        public async Task StopCurtain(int id)
        {
            byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x55, 0x01, (byte)id, 0x03, 0x03 };
            await _streamCurtain.WriteAsync(data, 0, data.Length);

            data = new Byte[12];
            Int32 ret = await _streamCurtain.ReadAsync(data, 0, data.Length);
            await _streamCurtain.FlushAsync();
        }

        /// <summary>
        /// 获取地暖恒温器的室内温度
        /// </summary>
        /// <returns></returns>
        public async Task<float> GetTemperature(int id)
        {
            float ret = 0;
            byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, (byte)id, 0x03, 0x00, 0x06, 0x00, 0x01 };
            //byte[] CRC = CRCHelper.get_CRC16_C(data);
            //byte[] cmd = new byte[] { data[0], data[1], data[2], data[3], data[4], data[5], CRC[0], CRC[1] };
            await _streamHailin.WriteAsync(data, 0, data.Length);

            data = new Byte[11];
            ret = await _streamHailin.ReadAsync(data, 0, data.Length);
            ret = BitConverter.ToInt16(new byte[] { data[10], data[9] });

            await _streamHailin.FlushAsync();
            return ret;
        }

        /// <summary>
        /// 获取地暖恒温器的设置温度
        /// </summary>
        /// <returns></returns>
        public async Task<float> GetTemperatureSetResult(int id)
        {
            float ret = 0;
            byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, (byte)id, 0x03, 0x00, 0x01, 0x00, 0x01 };
            //byte[] CRC = CRCHelper.get_CRC16_C(data);
            //byte[] cmd = new byte[] { data[0], data[1], data[2], data[3], data[4], data[5], CRC[0], CRC[1] };
            await _streamHailin.WriteAsync(data, 0, data.Length);

            data = new Byte[11];
            ret = await _streamHailin.ReadAsync(data, 0, data.Length);
            ret = BitConverter.ToInt16(new byte[] { data[10], data[9] });

            await _streamHailin.FlushAsync();
            return ret;
        }


        /// <summary>
        /// 设定温度
        /// </summary>
        /// <param name="id"></param>
        /// <param name="temperature"></param>
        /// <returns></returns>
        public async Task<bool> SetTemperature(int id, Int16 temperature)
        {
            int ret = 0;
            var byteTemp = BitConverter.GetBytes(temperature);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteTemp);
            }
            byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, (byte)id, 0x06, 0x00, 0x01, byteTemp[0], byteTemp[1] };
            await _streamHailin.WriteAsync(data, 0, data.Length);

            data = new Byte[12];
            ret = await _streamHailin.ReadAsync(data, 0, data.Length);
            ret = BitConverter.ToInt16(new byte[] { data[11], data[10] });

            await _streamHailin.FlushAsync();
            if (ret == temperature)
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            _streamCurtain.Close();
            _streamHailin.Close();
            _clientCurtain.Dispose();
            _clientHailin.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TcpWebGateway
{
    public static class CRCHelper
    {
        //获取并校验两数据是否一致
        public static bool checkCRC(byte[] srcData, byte[] desData)
        {
            byte[] data = get_CRC16_C(srcData);
            string crc = byteToHexStr(data,data.Length);
            if (crc == byteToHexStr(desData, desData.Length))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 计算CRC校验码，并转换为十六进制字符串
        /// Cyclic Redundancy Check 循环冗余校验码
        /// 是数据通信领域中最常用的一种差错校验码
        /// 特征是信息字段和校验字段的长度可以任意选定
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] get_CRC16_C(byte[] data)
        {
            byte num = 0xff;
            byte num2 = 0xff;

            byte num3 = 1;
            byte num4 = 160;
            byte[] buffer = data;

            for (int i = 0; i < buffer.Length; i++)
            {
                //位异或运算
                num = (byte)(num ^ buffer[i]);

                for (int j = 0; j <= 7; j++)
                {
                    byte num5 = num2;
                    byte num6 = num;

                    //位右移运算
                    num2 = (byte)(num2 >> 1);
                    num = (byte)(num >> 1);

                    //位与运算
                    if ((num5 & 1) == 1)
                    {
                        //位或运算
                        num = (byte)(num | 0x80);
                    }
                    if ((num6 & 1) == 1)
                    {
                        num2 = (byte)(num2 ^ num4);
                        num = (byte)(num ^ num3);
                    }
                }
            }
            return new byte[] { num, num2 };
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes, int size)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < size; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        public static int byteToInt32(byte[] bytes)
        {
            return BitConverter.ToInt16(bytes,0);
            
        }

        /// <summary>
        /// 累加校验和
        /// </summary>
        /// <param name="memorySpage">需要校验的数据</param>
        /// <returns>返回校验和结果</returns>
        public static Int16 Checksum(byte[] memorySpage)
        {
            int num = 0;
            for (int i = 0; i < memorySpage.Length; i++)
            {
                num = (num + memorySpage[i]) % 0xffff;
            }
            //实际上num 这里已经是结果了，如果只是取int 可以直接返回了
            memorySpage = BitConverter.GetBytes(num);
            //返回累加校验和
            return BitConverter.ToInt16(new byte[] { memorySpage[0], memorySpage[1] }, 0);
        }

        /// <summary>
        /// 功能码转byte[]
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

    }
}

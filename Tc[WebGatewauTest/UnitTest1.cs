using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using TcpWebGateway;

namespace Tc_WebGatewauTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //var val = TcpHelper.GetTemperature(1);
            //Assert.IsTrue(val == 300);

            //Int16 temp = 85;
            //var data = BitConverter.GetBytes(temp);
            //if (BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(data);
            //}

            //var str = CRCHelper.byteToHexStr(data, 2);
            //Assert.AreEqual(str, "0055");
            ////byte[] 转 int16 ，需要反转数组
            //Array.Reverse(data);
            //var intval = BitConverter.ToInt16(data);
            //Assert.IsTrue(intval == temp);
            var cmd = StringToByteArray("01 50 01 01 01 02");
            var crc = CRCHelper.Checksum(cmd);
            Assert.IsTrue(crc == 86);
        }

        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}

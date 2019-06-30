using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TcpWebGateway;

namespace Tc_WebGatewauTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var val = TcpHelper.GetTemperature(1);
            Assert.IsTrue(val == 300);

            Int16 temp = 85;
            var data = BitConverter.GetBytes(temp);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            
            var str = CRCHelper.byteToHexStr(data, 2);
            Assert.AreEqual(str, "0055");
            //byte[] ת int16 ����Ҫ��ת����
            Array.Reverse(data);
            var intval = BitConverter.ToInt16(data);
            Assert.IsTrue(intval == temp);
        }
    }
}

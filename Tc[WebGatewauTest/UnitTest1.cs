using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TcpWebGateway;
using TcpWebGateway.Tools;

namespace Tc_WebGatewauTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test3()
        {
            var data = "00 00 00 00 00 06 55 01 02 01 01 2A";
            Regex reg = new Regex("00 00 00 00 00 06 55 01 (.+) 01 01 (.+)");
            var match = reg.Match(data);
            Assert.IsTrue(match.Success == true);
            Assert.IsTrue(match.Groups[1].Value == "02");
            Assert.IsTrue(match.Groups[2].Value == "2A");
            var str = "2A";
            var ret = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            Assert.IsTrue(ret == 42);
        }

        [TestMethod]
        public void TestMethod0()
        {
            int i = 0;
            var task = Task.Run(async () => { i =await Count().ConfigureAwait(true); });
            task.Wait();
            Assert.IsTrue(i == 10);
        }

        public async Task<int> Count()
        {
            int i = 100;
            i = await Count1().ConfigureAwait(true);
            return i;
        }

        public async Task<int> Count1()
        {
            await Task.Delay(100).ConfigureAwait(true);
            return 10;
        }

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
            var str = JsonConvert.SerializeObject(new HvacStateObject());
            Assert.IsTrue(str.Length > 0);
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

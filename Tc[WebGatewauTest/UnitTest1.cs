using Microsoft.VisualStudio.TestTools.UnitTesting;
using TcpWebGateway;

namespace Tc_WebGatewauTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            int val = TcpHelper.GetTemperature(1);
            Assert.IsTrue(val == 300);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;

namespace TcpWebGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurtainController : ControllerBase
    {
        private IMqttClientOptions options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer("192.168.50.245", 1883)
            .WithCleanSession()
            .Build();

        private IMqttClient _mqttClient;

        public CurtainController()
        {
            MqttFactory factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

        }


        /// <summary>
        /// 读取窗帘状态
        /// 布帘id=3 , 纱帘id=2
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "Get")]
        public async Task<int> Get(int id)
        {
            if (id < 2 || id > 3)
            {
                return 0;
            }
            int retPercent = TcpHelper.GetStatus(id);

            var result = await _mqttClient.ConnectAsync(options);
            if (result.ResultCode == MqttClientConnectResultCode.Success)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("/Home/Curtain" + id + "/State")
                    .WithPayload(retPercent.ToString())
                    .WithAtLeastOnceQoS()
                    .Build();
                await _mqttClient.PublishAsync(message);
                await _mqttClient.DisconnectAsync();
            }
            return retPercent;

        }

        /// <summary>
        /// 设置窗帘闭合度
        /// </summary>
        /// <param name="id">布帘id=3 , 纱帘id=2</param>
        /// <param name="value">百分比</param>
        [HttpPost]
        public void SetPosition(int id, int value)
        {
            TcpHelper.SetStatus(id, value);
        }

        [HttpPost]
        public void Stop(int id)
        {
            TcpHelper.Stop(id);
        }
    }
}

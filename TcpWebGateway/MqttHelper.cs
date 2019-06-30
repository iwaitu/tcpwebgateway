using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpWebGateway
{
    public class MqttHelper
    {
        private IMqttClientOptions options = new MqttClientOptionsBuilder()
            .WithClientId("MqttNetCoreClient1")
            .WithTcpServer("192.168.50.245", 1883)
            .Build();

        private MqttClient _mqttClient;




        public async Task<bool> Connect()
        {
            int ret = 0;
            bool bSuccess = false;
            MqttFactory factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient() as MqttClient;

            _mqttClient.UseApplicationMessageReceivedHandler(e => {

                var sVal = string.Empty;
                Console.WriteLine("### 数据接收 ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {sVal}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
                if(e.ApplicationMessage.Topic == "Home/Curtain2/Set")
                {
                    sVal = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    TcpHelper.SetStatus(2, Convert.ToInt32(sVal));
                }else if (e.ApplicationMessage.Topic == "Home/Curtain3/Set")
                {
                    sVal = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    TcpHelper.SetStatus(3, Convert.ToInt32(sVal));
                }else if(e.ApplicationMessage.Topic == "Home/Curtain3/Get")
                {
                    ret = TcpHelper.GetStatus(3);
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Curtain3/Status")
                   .WithPayload(ret.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    this.Publish(message);
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain2/Get")
                {
                    ret = TcpHelper.GetStatus(2);
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Curtain2/Status")
                   .WithPayload(ret.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    this.Publish(message);
                }
                else if(e.ApplicationMessage.Topic == "Home/Curtain2/Open")
                {
                    TcpHelper.Open(2);
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain2/Stop")
                {
                    TcpHelper.Stop(2);
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain2/Close")
                {
                    TcpHelper.Close(2);
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain3/Open")
                {
                    TcpHelper.Open(3);
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain3/Stop")
                {
                    TcpHelper.Stop(3);
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain3/Close")
                {
                    TcpHelper.Close(3);
                }else if(e.ApplicationMessage.Topic == "Home/Hailin1/Set")
                {
                    sVal = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    bSuccess = false;
                    bSuccess = TcpHelper.SetTemperature(1, Convert.ToInt16 (sVal));
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin2/Set")
                {
                    sVal = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    TcpHelper.SetTemperature(2, Convert.ToInt16(sVal));
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin3/Set")
                {
                    sVal = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    TcpHelper.SetTemperature(3, Convert.ToInt16(sVal));
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin1/GetCurrent")
                {
                    var temp = TcpHelper.GetTemperature(1) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin1/CurrentTemp")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    this.Publish(message);
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin2/GetCurrent")
                {
                    var temp = TcpHelper.GetTemperature(2) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin2/CurrentTemp")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    this.Publish(message);
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin3/GetCurrent")
                {
                    var temp = TcpHelper.GetTemperature(3) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin3/CurrentTemp")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    this.Publish(message);
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin1/GetSetResult")
                {
                    var temp = TcpHelper.GetTemperatureSetResult(1) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin1/SetResult")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    this.Publish(message);
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin2/GetSetResult")
                {
                    var temp = TcpHelper.GetTemperatureSetResult(2) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin2/SetResult")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    this.Publish(message);
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin3/GetSetResult")
                {
                    var temp = TcpHelper.GetTemperatureSetResult(3) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin3/SetResult")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    this.Publish(message);
                }

            });

            //_mqttClient.UseDisconnectedHandler(async e => {
            //    //Console.WriteLine("### MQTT掉线,1秒后重连 ###");
            //    //await Task.Delay(1000);
            //    //await _mqttClient.ReconnectAsync();
            //});

            var result =  await _mqttClient.ConnectAsync(options);
            if(result.ResultCode == MQTTnet.Client.Connecting.MqttClientConnectResultCode.Success)
            {
                return true;
            }
            return false;
        }

        public async void Subscribe(string topic)
        {
            await _mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
        }

        public async void Publish(MqttApplicationMessage message)
        {
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ReconnectAsync();
            }
            await _mqttClient.PublishAsync(message);
        }


    }
}

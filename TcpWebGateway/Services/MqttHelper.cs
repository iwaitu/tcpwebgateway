
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using NLog;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpWebGateway.Services
{
    public class MqttHelper : IMqttHelper
    {
        private IMqttClientOptions options = new MqttClientOptionsBuilder()
            .WithClientId("MqttNetCoreClient1")
            .WithTcpServer("192.168.50.245", 1883)
            .Build();

        private MqttClient _mqttClient;
        private readonly ILogger _logger;
        private bool Started = false;

        public MqttHelper()
        {
            _logger = LogManager.GetCurrentClassLogger();

            int ret = 0;
            bool bSuccess = false;
            MqttFactory factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient() as MqttClient;

            _mqttClient.UseApplicationMessageReceivedHandler(e =>
            {

                var sVal = string.Empty;
                sVal = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                _logger.Info("### 数据接收 ###");
                _logger.Info($"+ Topic = {e.ApplicationMessage.Topic}");
                _logger.Info($"+ Payload = {sVal}");
                _logger.Info($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                _logger.Info($"+ Retain = {e.ApplicationMessage.Retain}");
                _logger.Info("");

                if (e.ApplicationMessage.Topic == "Home/Curtain2/Set")
                {
                    
                    if(Convert.ToInt32(sVal) == -1)
                    {
                        TcpHelper.Stop(2);
                        ret = TcpHelper.GetStatus(2);
                        var message = new MqttApplicationMessageBuilder().WithTopic("Home/Curtain2/Status")
                        .WithPayload(ret.ToString())
                        .WithAtLeastOnceQoS()
                        .Build();
                        Task.Run(async () => { await Publish(message); });
                    }
                    else
                    {
                        TcpHelper.SetStatus(2, Convert.ToInt32(sVal));
                    }
                    
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain3/Set")
                {

                    if (Convert.ToInt32(sVal) == -1)
                    {
                        TcpHelper.Stop(3);
                        ret = TcpHelper.GetStatus(3);
                        var message = new MqttApplicationMessageBuilder().WithTopic("Home/Curtain2/Status")
                        .WithPayload(ret.ToString())
                        .WithAtLeastOnceQoS()
                        .Build();
                        Task.Run(async () => { await Publish(message); });
                    }
                    else
                    {
                        TcpHelper.SetStatus(3, Convert.ToInt32(sVal));
                    }
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain3/Get")
                {
                    ret = TcpHelper.GetStatus(3);
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Curtain3/Status")
                   .WithPayload(ret.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain2/Get")
                {
                    ret = TcpHelper.GetStatus(2);
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Curtain2/Status")
                   .WithPayload(ret.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain2")
                {

                    if (sVal == "open")
                    {
                        TcpHelper.Open(2);
                    }
                    else if (sVal == "close")
                    {
                        TcpHelper.Close(2);
                    }
                    else if (sVal == "stop")
                    {
                        TcpHelper.Stop(2);
                        ret = TcpHelper.GetStatus(2);
                        var message = new MqttApplicationMessageBuilder().WithTopic("Home/Curtain2/Status")
                        .WithPayload(ret.ToString())
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag(false)
                        .Build();
                        Task.Run(async () => { await Publish(message); });
                    }
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain3")
                {

                    if (sVal == "open")
                    {
                        TcpHelper.Open(3);
                    }
                    else if(sVal == "close")
                    {
                        TcpHelper.Close(3);
                    }
                    else if(sVal == "stop")
                    {
                        TcpHelper.Stop(3);
                        ret = TcpHelper.GetStatus(3);
                        var message = new MqttApplicationMessageBuilder().WithTopic("Home/Curtain3/Status")
                        .WithPayload(ret.ToString())
                        .WithAtLeastOnceQoS()
                        .Build();
                        Task.Run(async () => { await Publish(message); });

                    }
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin1/Set")
                {

                    float fVal = float.Parse(sVal);
                    fVal = fVal * 10;
                    bSuccess = false;
                    bSuccess = TcpHelper.SetTemperature(1, (short)fVal);
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin2/Set")
                {

                    float fVal = float.Parse(sVal);
                    fVal = fVal * 10;
                    bSuccess = false;
                    TcpHelper.SetTemperature(2, (short)fVal);
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin3/Set")
                {

                    float fVal = float.Parse(sVal);
                    fVal = fVal * 10;
                    bSuccess = false;
                    TcpHelper.SetTemperature(3, (short)fVal);
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin1/GetCurrent")
                {
                    var temp = TcpHelper.GetTemperature(1) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin1/CurrentTemp")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin2/GetCurrent")
                {
                    var temp = TcpHelper.GetTemperature(2) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin2/CurrentTemp")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin3/GetCurrent")
                {
                    var temp = TcpHelper.GetTemperature(3) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin3/CurrentTemp")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin1/GetSetResult")
                {
                    var temp = TcpHelper.GetTemperatureSetResult(1) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin1/SetResult")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin2/GetSetResult")
                {
                    var temp = TcpHelper.GetTemperatureSetResult(2) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin2/SetResult")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin3/GetSetResult")
                {
                    var temp = TcpHelper.GetTemperatureSetResult(3) / 10;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin3/SetResult")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }

            });

            _mqttClient.UseDisconnectedHandler(async e => {
                await _mqttClient.ReconnectAsync();
                SetupSubscribe();
            });

            var result = _mqttClient.ConnectAsync(options).Result;
            if (result.ResultCode == MQTTnet.Client.Connecting.MqttClientConnectResultCode.Success)
            {
                SetupSubscribe();
            }
            Started = true;
        }


        public async void Subscribe(string topic)
        {
            await _mqttClient.SubscribeAsync(topic, MqttQualityOfServiceLevel.AtLeastOnce);
        }

        public async Task Publish(MqttApplicationMessage message)
        {
            try
            {
                if(!_mqttClient.IsConnected && Started == true)
                {
                    await _mqttClient.ReconnectAsync();
                    //SetupSubscribe();
                }
                await _mqttClient.PublishAsync(message);
            }
            catch (Exception ex)
            {

                _logger.Error(ex.Message);
            }

        }

        private void SetupSubscribe()
        {
            Subscribe("Home/Curtain2/Set");
            Subscribe("Home/Curtain3/Set");
            Subscribe("Home/Curtain2/Get");
            Subscribe("Home/Curtain3/Get");
            Subscribe("Home/Curtain3");
            Subscribe("Home/Curtain2");
            Subscribe("Home/Hailin1/GetCurrent");
            Subscribe("Home/Hailin2/GetCurrent");
            Subscribe("Home/Hailin3/GetCurrent");
            Subscribe("Home/Hailin1/GetSetResult");
            Subscribe("Home/Hailin2/GetSetResult");
            Subscribe("Home/Hailin3/GetSetResult");
            Subscribe("Home/Hailin1/Set");
            Subscribe("Home/Hailin2/Set");
            Subscribe("Home/Hailin3/Set");
        }

        public async Task StartAsync()
        {
            try
            {
                var result = await _mqttClient.ConnectAsync(options);
                if (result.ResultCode == MQTTnet.Client.Connecting.MqttClientConnectResultCode.Success)
                {
                    SetupSubscribe();
                }
                Started = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }


        public void Dispose()
        {
            if (_mqttClient != null)
            {
                if (_mqttClient.IsConnected)
                {
                    var task = new Task(() => { _mqttClient.DisconnectAsync(); });
                    task.RunSynchronously();
                }
                _mqttClient.Dispose();
            }
        }
    }
}

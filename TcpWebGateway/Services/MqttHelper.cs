
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
    public class MqttHelper : BackgroundService
    {
        private IMqttClientOptions options = new MqttClientOptionsBuilder()
            .WithClientId("MqttNetCoreClient1")
            .WithTcpServer("192.168.50.245", 1883)
            .Build();

        private MqttClient _mqttClient;
        private readonly ILogger _logger;
        private readonly TcpHelper _tcpHelper;
        private bool Started = false;

        public MqttHelper(TcpHelper tcpHelper)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _tcpHelper = tcpHelper;
            int ret = 0;
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

                    if (Convert.ToInt32(sVal) == -1)
                    {
                        Task.Run(async () => { await _tcpHelper.StopCurtain(2); });
                        Task.Run(async () => { ret = await _tcpHelper.GetCurtainStatus(2); });
                        
                        var message = new MqttApplicationMessageBuilder().WithTopic("Home/Curtain2/Status")
                        .WithPayload(ret.ToString())
                        .WithAtLeastOnceQoS()
                        .Build();
                        Task.Run(async () => { await Publish(message); });
                    }
                    else
                    {
                        Task.Run(async () => { await _tcpHelper.SetCurtainStatus(2, Convert.ToInt32(sVal)); });
                    }
                    
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain3/Set")
                {

                    if (Convert.ToInt32(sVal) == -1)
                    {
                        Task.Run(async () => { await _tcpHelper.StopCurtain(3); });
                        Task.Run(async () => { ret = await _tcpHelper.GetCurtainStatus(3); });
                        var message = new MqttApplicationMessageBuilder().WithTopic("Home/Curtain2/Status")
                        .WithPayload(ret.ToString())
                        .WithAtLeastOnceQoS()
                        .Build();
                        Task.Run(async () => { await Publish(message); });
                    }
                    else
                    {
                        Task.Run(async () => { await _tcpHelper.SetCurtainStatus(3, Convert.ToInt32(sVal)); });
                    }
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain3/Get")
                {
                    Task.Run(async () => { ret = await _tcpHelper.GetCurtainStatus(3); });
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Curtain3/Status")
                   .WithPayload(ret.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain2/Get")
                {
                    Task.Run(async () => { ret = await _tcpHelper.GetCurtainStatus(2); });
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Curtain2/Status")
                   .WithPayload(ret.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain2/Command")
                {

                    if (sVal == "open")
                    {
                        Task.Run(async () => { await _tcpHelper.OpenCurtain(2); });
                    }
                    else if (sVal == "close")
                    {
                        Task.Run(async () => { await _tcpHelper.CloseCurtain(2); });
                    }
                    else if (sVal == "stop")
                    {
                        
                        Task.Run(async () => { await _tcpHelper.StopCurtain(2);  });
                        Task.Delay(100);
                        Task.Run(async () => { ret = await _tcpHelper.GetCurtainStatus(2); });
                        var message = new MqttApplicationMessageBuilder().WithTopic("Home/Curtain2/Status")
                        .WithPayload(ret.ToString())
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag(false)
                        .Build();
                        Task.Run(async () => { await Publish(message); });
                    }
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain3/Command")
                {

                    if (sVal == "open")
                    {
                        Task.Run(async () => { await _tcpHelper.OpenCurtain(3); });
                    }
                    else if (sVal == "close")
                    {
                        Task.Run(async () => { await _tcpHelper.CloseCurtain(3); });
                    }
                    else if (sVal == "stop")
                    {

                        Task.Run(async () => { await _tcpHelper.StopCurtain(3); });
                        Task.Delay(100);
                        Task.Run(async () => { ret = await _tcpHelper.GetCurtainStatus(3); });
                        var message = new MqttApplicationMessageBuilder().WithTopic("Home/Curtain3/Status")
                        .WithPayload(ret.ToString())
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag(false)
                        .Build();
                        Task.Run(async () => { await Publish(message); });
                    }
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin1/Set")
                {

                    float fVal = float.Parse(sVal);
                    fVal = fVal * 10;

                    Task.Run(async () => { await _tcpHelper.SetTemperature(1, (short)fVal); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin2/Set")
                {

                    float fVal = float.Parse(sVal);
                    fVal = fVal * 10;

                    Task.Run(async () => { await _tcpHelper.SetTemperature(2, (short)fVal); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin3/Set")
                {

                    float fVal = float.Parse(sVal);
                    fVal = fVal * 10;

                    Task.Run(async () => { await _tcpHelper.SetTemperature(3, (short)fVal); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin1/GetCurrent")
                {
                    float temp = 0;
                    Task.Run(async () => { temp = await _tcpHelper.GetTemperature(1) / 10; }); ;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin1/CurrentTemp")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin2/GetCurrent")
                {
                    float temp = 0;
                    Task.Run(async () => { temp  = await _tcpHelper.GetTemperature(2) / 10; }); ;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin2/CurrentTemp")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin3/GetCurrent")
                {
                    float temp = 0;
                    Task.Run(async () => { temp = await _tcpHelper.GetTemperature(3) / 10; }); ;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin3/CurrentTemp")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin1/GetSetResult")
                {
                    float temp = 0;
                    Task.Run(async () => { temp = await _tcpHelper.GetTemperature(1) / 10; }); ;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin1/SetResult")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin2/GetSetResult")
                {
                    float temp = 0;
                    Task.Run(async () => { temp = await _tcpHelper.GetTemperature(2) / 10; }); ;
                    var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin2/SetResult")
                   .WithPayload(temp.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    Task.Run(async () => { await Publish(message); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Hailin3/GetSetResult")
                {
                    float temp = 0;
                    Task.Run(async () => { temp = await _tcpHelper.GetTemperature(3) / 10; }); ;
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
            Subscribe("Home/Curtain2/Set"); //设置窗帘开合百分比
            Subscribe("Home/Curtain3/Set"); //设置窗帘开合百分比
            Subscribe("Home/Curtain2/Get");
            Subscribe("Home/Curtain3/Get");
            Subscribe("Home/Curtain3/Command"); //接收命令:open,close,stop
            Subscribe("Home/Curtain2/Command"); //接收命令:open,close,stop
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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return StartAsync();


        }
    }
}

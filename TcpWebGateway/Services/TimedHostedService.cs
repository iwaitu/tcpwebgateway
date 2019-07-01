using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TcpWebGateway.Services
{

    internal class TimedHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;
        private IMqttClientOptions options = new MqttClientOptionsBuilder()
            .WithClientId("MqttNetCoreTimer1")
            .WithTcpServer("192.168.50.245", 1883)
            .Build();


        public TimedHostedService(ILogger<TimedHostedService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Timed Background Service is starting.");
           
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }


        private async void DoWork(object state)
        {
           
            MqttFactory factory = new MqttFactory();
            using (MqttClient _mqttClient = factory.CreateMqttClient() as MqttClient)
            {
                try
                {
                    var result = _mqttClient.ConnectAsync(options).Result;
                    if (result.ResultCode == MQTTnet.Client.Connecting.MqttClientConnectResultCode.Success)
                    {
                        ///客厅
                        var temp1 = TcpHelper.GetTemperature(1) / 10;
                        var message = new MqttApplicationMessageBuilder()
                               .WithTopic("Home/Hailin1/CurrentTemp")
                               .WithPayload(temp1.ToString())
                               .WithAtLeastOnceQoS()
                               .Build();
                        _ = await _mqttClient.PublishAsync(message);
                        temp1 = TcpHelper.GetTemperatureSetResult(1) / 10;
                        message = new MqttApplicationMessageBuilder()
                       .WithTopic("Home/Hailin1/SetResult")
                       .WithPayload(temp1.ToString())
                       .WithAtLeastOnceQoS()
                       .Build();
                        _ = await _mqttClient.PublishAsync(message);

                        ///客卧
                        temp1 = TcpHelper.GetTemperature(2) / 10;
                        message = new MqttApplicationMessageBuilder()
                               .WithTopic("Home/Hailin2/CurrentTemp")
                               .WithPayload(temp1.ToString())
                               .WithAtLeastOnceQoS()
                               .Build();
                        _ = _mqttClient.PublishAsync(message).Result;
                        temp1 = TcpHelper.GetTemperatureSetResult(3) / 10;
                        message = new MqttApplicationMessageBuilder()
                       .WithTopic("Home/Hailin2/SetResult")
                       .WithPayload(temp1.ToString())
                       .WithAtLeastOnceQoS()
                       .Build();
                        _ = await _mqttClient.PublishAsync(message);

                        ///主卧
                        temp1 = TcpHelper.GetTemperature(3) / 10;
                        message = new MqttApplicationMessageBuilder()
                               .WithTopic("Home/Hailin3/CurrentTemp")
                               .WithPayload(temp1.ToString())
                               .WithAtLeastOnceQoS()
                               .Build();
                        _ = await _mqttClient.PublishAsync(message);
                        temp1 = TcpHelper.GetTemperatureSetResult(3) / 10;
                        message = new MqttApplicationMessageBuilder()
                       .WithTopic("Home/Hailin3/SetResult")
                       .WithPayload(temp1.ToString())
                       .WithAtLeastOnceQoS()
                       .Build();
                        _ = await _mqttClient.PublishAsync(message);

                        await _mqttClient.DisconnectAsync();
                    }
                }
                catch (Exception ex)
                {
                    if(_mqttClient != null && _mqttClient.IsConnected)
                    {
                        await _mqttClient.DisconnectAsync();
                    }
                    _logger.LogError(ex.Message);
                }
                
            }
                
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Timed Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

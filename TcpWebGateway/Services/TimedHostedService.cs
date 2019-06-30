using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private MqttClient _mqttClient;

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

        private void DoWork(object state)
        {
           
            MqttFactory factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient() as MqttClient;
            _ = _mqttClient.ConnectAsync(options).Result;

            ///客厅
            var temp1 = TcpHelper.GetTemperature(1) / 10;
            var message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin1/CurrentTemp")
                   .WithPayload(temp1.ToString() )
                   .WithAtLeastOnceQoS()
                   .Build();
            _ = _mqttClient.PublishAsync(message).Result;
            temp1 = TcpHelper.GetTemperatureSetResult(1) / 10;
            message = new MqttApplicationMessageBuilder()
           .WithTopic("Home/Hailin1/SetResult")
           .WithPayload(temp1.ToString())
           .WithAtLeastOnceQoS()
           .Build();
            _ = _mqttClient.PublishAsync(message).Result;

            ///客卧
            temp1 = TcpHelper.GetTemperature(2) / 10;
            message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin2/CurrentTemp")
                   .WithPayload(temp1.ToString() )
                   .WithAtLeastOnceQoS()
                   .Build();
            _ = _mqttClient.PublishAsync(message).Result;
            temp1 = TcpHelper.GetTemperatureSetResult(3) / 10;
            message = new MqttApplicationMessageBuilder()
           .WithTopic("Home/Hailin2/SetResult")
           .WithPayload(temp1.ToString())
           .WithAtLeastOnceQoS()
           .Build();
            _ = _mqttClient.PublishAsync(message).Result;

            ///主卧
            temp1 = TcpHelper.GetTemperature(3) / 10;
            message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin3/CurrentTemp")
                   .WithPayload(temp1.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
            _ = _mqttClient.PublishAsync(message).Result;
            temp1 = TcpHelper.GetTemperatureSetResult(3) / 10;
            message = new MqttApplicationMessageBuilder()
           .WithTopic("Home/Hailin3/SetResult")
           .WithPayload(temp1.ToString())
           .WithAtLeastOnceQoS()
           .Build();
            _ = _mqttClient.PublishAsync(message).Result;

            _mqttClient.DisconnectAsync();
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

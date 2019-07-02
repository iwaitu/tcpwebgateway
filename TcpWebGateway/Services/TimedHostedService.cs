using Microsoft.Extensions.DependencyInjection;
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
        private readonly MqttHelper _mqttHelper;
        public IServiceProvider Services { get; }

        private Timer _timer;


        public TimedHostedService(ILogger<TimedHostedService> logger, MqttHelper mqtthelper)
        {
            _logger = logger;
            _mqttHelper = mqtthelper;
            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //_logger.LogInformation("Timed Background Service is starting.");
           
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(30));
            return _mqttHelper.StartAsync();
        }


        private async void DoWork(object state)
        {
            _logger.LogInformation("后台任务|" + DateTime.Now.ToString());
            if (_mqttHelper != null)
            {
                try
                {
                    //var scopedProcessingService = scope.ServiceProvider.GetRequiredService<MqttHelper>();

                    ///客厅
                    var temp1 = TcpHelper.GetTemperature(1) / 10;
                    var message = new MqttApplicationMessageBuilder()
                           .WithTopic("Home/Hailin1/CurrentTemp")
                           .WithPayload(temp1.ToString())
                           .WithAtLeastOnceQoS()
                           .Build();
                    await _mqttHelper.Publish(message);
                    temp1 = TcpHelper.GetTemperatureSetResult(1) / 10;
                    message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin1/SetResult")
                   .WithPayload(temp1.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    await _mqttHelper.Publish(message);

                    ///客卧
                    temp1 = TcpHelper.GetTemperature(2) / 10;
                    message = new MqttApplicationMessageBuilder()
                           .WithTopic("Home/Hailin2/CurrentTemp")
                           .WithPayload(temp1.ToString())
                           .WithAtLeastOnceQoS()
                           .Build();
                    await _mqttHelper.Publish(message);
                    temp1 = TcpHelper.GetTemperatureSetResult(3) / 10;
                    message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin2/SetResult")
                   .WithPayload(temp1.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    await _mqttHelper.Publish(message);

                    ///主卧
                    temp1 = TcpHelper.GetTemperature(3) / 10;
                    message = new MqttApplicationMessageBuilder()
                           .WithTopic("Home/Hailin3/CurrentTemp")
                           .WithPayload(temp1.ToString())
                           .WithAtLeastOnceQoS()
                           .Build();
                    await _mqttHelper.Publish(message);
                    temp1 = TcpHelper.GetTemperatureSetResult(3) / 10;
                    message = new MqttApplicationMessageBuilder()
                   .WithTopic("Home/Hailin3/SetResult")
                   .WithPayload(temp1.ToString())
                   .WithAtLeastOnceQoS()
                   .Build();
                    await _mqttHelper.Publish(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                
            }
                
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

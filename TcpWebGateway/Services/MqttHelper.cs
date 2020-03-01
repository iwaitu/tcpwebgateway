
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using NLog;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpWebGateway.Tools;
using static TcpWebGateway.Tools.CurtainHelper;

namespace TcpWebGateway.Services
{
    /// <summary>
    /// mqtt 消息总线
    /// </summary>
    public class MqttHelper : BackgroundService
    {
        
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly CurtainHelper _curtainHelper;
        private readonly HvacHelper _hvacHelper;
        private readonly LightHelper _lightHelper;

        private bool Started = false;
        private IMqttClientOptions options;
        private MqttClient _mqttClient;

        public MqttHelper(CurtainHelper curtainHelper, HvacHelper hvacHelper, IConfiguration configuration,LightHelper lightHelper)
        {
            _config = configuration;
            _logger = LogManager.GetCurrentClassLogger();
            _curtainHelper = curtainHelper;
            _hvacHelper = hvacHelper;
            _lightHelper = lightHelper;
            _hvacHelper.SetMqttListener(this);
            _curtainHelper.SetMqttListener(this);
            _lightHelper.SetMqttListener(this);
            //_lightHelper.SetCurtainHelper(_curtainHelper);

            var mqtthost = _config.GetValue<string>("mqttBroken:Hostip");
            var port = _config.GetValue<int>("mqttBroken:port");

            _logger.Info("Connect to MQTT Broken：{0}:{1}", mqtthost, port);

            options = new MqttClientOptionsBuilder()
           .WithClientId(Guid.NewGuid().ToString())
           .WithTcpServer(mqtthost, port)
           .Build();

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

                /*   窗帘  */
                if (e.ApplicationMessage.Topic == "Home/Curtain/Set")
                {
                    var obj = JsonConvert.DeserializeObject<CurtainStateObject>(sVal);
                    Task.Run(async () => { await _curtainHelper.SetCurtain(obj.Id, obj.Status); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Curtain/Command")
                {
                    var obj = JsonConvert.DeserializeObject<CurtainStateObject>(sVal);
                    if (obj.Command == "open")
                    {
                        Task.Run(async () => { await _curtainHelper.Open(obj.Id); });
                    }
                    else if (obj.Command == "close")
                    {
                        Task.Run(async () => { await _curtainHelper.Close(obj.Id); });
                    }
                    else if (obj.Command == "stop")
                    {

                        var task1 = Task.Run(async () =>
                        {
                            await _curtainHelper.Stop(obj.Id);
                            await Task.Delay(100);
                            await _curtainHelper.GetCurtainStatus(obj.Id);
                        });
                    }
                }
                /*   空调  */
                else if (e.ApplicationMessage.Topic == "Home/Mitsubishi/Command")
                {
                    var obj = JsonConvert.DeserializeObject<HvacStateObject>(sVal);
                    Task.Run(async () => { await _hvacHelper.UpdateStateObject(obj); });

                }

                else if (e.ApplicationMessage.Topic == "Home/LightScene/Livingroom")
                {
                    int i = (int)float.Parse(sVal);
                    Task.Run(async () => { await _lightHelper.SceneLivingRoomSet((SceneState)i); });
                }
                else if (e.ApplicationMessage.Topic == "Home/LightScene/Bedroom")
                {
                    int i = (int)float.Parse(sVal);
                    Task.Run(async () => { await _lightHelper.SceneBedRoomSet((SceneState)i); });
                }
                else if (e.ApplicationMessage.Topic == "Home/LightScene/Guestroom")
                {
                    int i = (int)float.Parse(sVal);
                    Task.Run(async () => { await _lightHelper.SceneGuestRoomSet((SceneState)i); });
                }
                else if (e.ApplicationMessage.Topic == "Home/LightScene/Workroom")
                {
                    int i = (int)float.Parse(sVal);
                    Task.Run(async () => { await _lightHelper.SceneWorkRoomSet((SceneState)i); });
                }
                else if (e.ApplicationMessage.Topic == "Home/LightScene/Dinner")
                {
                    int i = (int)float.Parse(sVal);
                    Task.Run(async () => { await _lightHelper.SceneDinnerRoomSet((SceneState)i); });
                }
                else if (e.ApplicationMessage.Topic == "Home/Mode")
                {
                    int i = int.Parse(sVal);
                    _lightHelper.CurrentStateMode = (StateMode)i;
                    if (i == 0)
                    {
                        Task.Run(async () => { await _lightHelper.HomeMode(); });
                    } else if (i == 1)
                    {
                        Task.Run(async () => { await _lightHelper.OutMode(); });
                    } else if (i == 2)
                    {
                        Task.Run(async () => { await _lightHelper.ReadMode(); });
                    }
                }
                else if (e.ApplicationMessage.Topic == "Home/Sensor/Motion/1")
                {
                    if(sVal == "ON")
                    {
                        Task.Run(async () => { await _lightHelper.OpenWindowLight(1); });
                    }
                    else
                    {
                        Task.Run(async () => { await _lightHelper.CloseWindowLight(1); });
                    }
                    
                }
                else if (e.ApplicationMessage.Topic == "Home/Sensor/Motion/2")
                {
                    if (sVal == "ON")
                    {
                        Task.Run(async () => { await _lightHelper.OpenWindowLight(2); });
                    }
                    else
                    {
                        Task.Run(async () => { await _lightHelper.CloseWindowLight(2); });
                    }
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
                _logger.Info(message.Topic);
            }
            catch (Exception ex)
            {

                _logger.Error(ex.Message);
            }

        }

        private void SetupSubscribe()
        {
            Subscribe("Home/Curtain/Set"); //设置窗帘开合百分比
            Subscribe("Home/Curtain/GetStatus");
            Subscribe("Home/Curtain/Command"); //接收命令:open,close,stop
            Subscribe("Home/Hailin/GetState");
            Subscribe("Home/Hailin/Command");
            Subscribe("Home/Mitsubishi/Command");
            Subscribe("Home/LightScene/Livingroom");
            Subscribe("Home/LightScene/Workroom");
            Subscribe("Home/LightScene/Bedroom");
            Subscribe("Home/LightScene/Guestroom");
            Subscribe("Home/LightScene/Dinner");
            Subscribe("Home/Mode");
            Subscribe("Home/Sensor/Motion/1");
            Subscribe("Home/Sensor/Motion/2");
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


        public override void Dispose()
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

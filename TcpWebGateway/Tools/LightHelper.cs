using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TcpWebGateway.Services;

namespace TcpWebGateway.Tools
{
    /// <summary>
    /// 负责和智能面板通讯
    /// </summary>
    public class LightHelper
    {
        private readonly ILogger _logger;
        
        private SwitchListener _listener;
        private HVACSelected _hVacSelected = HVACSelected.None;
        private readonly HvacHelper _hvacHelper;
        private MqttHelper _mqttHelper;
        private SensorHelper _sensorHelper;
        private DateTime _lastHomeButonReceive;
        private DateTime _lastOutButonReceive;
        private DateTime _lastReadButonReceive;
        private readonly string API_PATH;

        public StateMode CurrentStateMode { get; set; }

        public LightHelper(ILogger<LightHelper> logger, HvacHelper hvacHelper, IConfiguration configuration)
        {
            _logger = logger;
            _hvacHelper = hvacHelper;
            _hvacHelper.SetLightHelper(this);
            API_PATH = configuration.GetValue<string>("RestApi:RestApiUrl");
        }

        public void SetListener(SwitchListener listener)
        {
            _listener = listener;
        }

        public void SetSensorHelper(SensorHelper sensorHelper)
        {
            _sensorHelper = sensorHelper;
        }

        public void SetMqttListener(MqttHelper helper)
        {
            _mqttHelper = helper;
        }

        public async Task OnReceiveCommand(string Command)
        {
            _logger.LogInformation("时间:" + DateTime.Now);
            _logger.LogInformation("ReceiveCommand : " + Command );
            if (string.IsNullOrEmpty(Command)) return;
            //面板OB
            if (Command.IndexOf("0B 20 10 11 00 01 00 FF") >= 0) //面板OB松开回家模式按键
            {
                
                
                await HomeMode();
            }
            else if (Command.IndexOf("0B 20 10 11 00 01 00 00 8F") >= 0) //点亮回家模式时再按回家按钮
            {
                if (_lastHomeButonReceive.Year == 1)
                {
                    _lastHomeButonReceive = DateTime.Now;
                }
                else
                {
                    var ts = DateTime.Now - _lastHomeButonReceive;
                    if (ts.TotalMilliseconds < 1000)
                    {
                        //表示双击
                        _logger.LogInformation("Home 双击" + ts.TotalMilliseconds);

                    }
                    else
                    {
                        _logger.LogInformation("Home 单击" + ts.TotalMilliseconds);
                    }
                    _lastHomeButonReceive = DateTime.Now;
                }
            }
            else if(Command.IndexOf("0B 20 10 12 00 01 00 FF")>= 0) //松开外出模式按键
            {
                if (_lastOutButonReceive.Year == 1)
                {
                    _lastOutButonReceive = DateTime.Now;
                }
                else
                {
                    var ts = DateTime.Now - _lastOutButonReceive;
                    if (ts.TotalMilliseconds < 1000)
                    {
                        //表示双击
                        _logger.LogInformation("Out 双击");
                    }
                    _lastOutButonReceive = DateTime.Now;
                }
                await OutMode();
            }
            else if (Command.IndexOf("0B 20 10 13 00 01 00 FF")>=0) //松开阅读模式按键
            {
                if (_lastReadButonReceive.Year == 1)
                {
                    _lastReadButonReceive = DateTime.Now;
                }
                else
                {
                    var ts = DateTime.Now - _lastOutButonReceive;
                    if (ts.TotalMilliseconds < 1000)
                    {
                        //表示双击
                        _logger.LogInformation("Read 双击");
                    }
                    _lastReadButonReceive = DateTime.Now;
                }
                await ReadMode();
            }
            //面板OC -------------------------------------------------
            else if (Command.IndexOf("0C 20 10 11 00 01 00 FF") >= 0) //松开全开按钮
            {
                await OpenAll();
            }
            else if(Command.IndexOf("0C 20 10 12 00 01 00 FF") >= 0) //松开全关按钮
            {
                await CloseAll();
            }
            else if (Command.IndexOf("0C 20 10 12 00 01 00 FF") >= 0) //松开全关按钮
            {
                await CloseAll();
            }
            else if (Command.IndexOf("0C 20 10 13 00 01 00 FF") >= 0) //松开观影按钮
            {
                //await CloseAll();
            }
            else if (Command.IndexOf("0C 20 10 14 00 01 00 FF") >= 0) //打开餐厨按钮
            {
                await OpenKitchen();
            }
            else if (Command.IndexOf("0C 20 10 14 00 01 00 7F") >= 0) //关闭餐厨按钮
            {
                await CloseKitchen();
            }
            else if (Command.IndexOf("0C 20 10 15 00 01 00 FF") >= 0) //筒灯-- 放松模式
            {
                
            }
            else if (Command.IndexOf("0C 20 10 15 00 01 00 FF") >= 0) //筒灯-- 取消放松模式
            {
                
            }
            else if (Command.IndexOf("0C 20 10 16 00 01 00 7F") >= 0) //主灯关
            {
                await CloseMainLight();
            }
            else if (Command.IndexOf("0C 20 10 16 00 01 00 FF") >= 0) //主灯开
            {
                await OpenMainLight();
            }

            //OD 面板 -----------------------------------------------------
            #region 书房空调按钮
            else if (Command.IndexOf("0D 20 10 15 00 01 00 FF") >= 0) //非点亮状态打开书房空调
            {
                await OpenWorkroomAC(); //打开空调
                
            }
            else if (Command.IndexOf("0D 20 10 15 00 01 00 7F") >= 0) //点亮状态下点击按钮
            {
                await CloseWorkroomAC();
            }
            else if( Command.IndexOf("0D 20 10 15 00 01 00 01") >= 0) // 点亮状态下长按
            {
                //切换温控面板数据连接到书房空调
                //_logger.LogInformation("长按切换温控板数据");
                await FlashWorkroomBackgroundLight();
                await OpenACPanel(2);
                _hVacSelected = HVACSelected.WorkRoom;
            }
            #endregion

            #region 客厅空调按钮
            else if (Command.IndexOf("0D 20 10 16 00 01 00 FF") >= 0) //打开客厅空调
            {
                await OpenLivingroomAC();
                
            }
            else if (Command.IndexOf("0D 20 10 16 00 01 00 7F") >= 0) //关闭客厅按钮
            {
                await CloseLivingroomAC();
                
            }
            else if (Command.IndexOf("0D 20 10 16 00 01 00 01") >= 0) // 点亮状态下长按
            {
                //切换温控面板数据连接到客厅空调
                //_logger.LogInformation("长按切换温控板数据");
                await FlashLivingroomBackgroundLight();
                await OpenACPanel(3);
                _hVacSelected = HVACSelected.LivingRoom;
            }
            #endregion

            else if (Command.IndexOf("0D 20 10 11 00 01 00 FF") >= 0) //新风开
            {
                //await CloseLivingroomAC();
                await OpenAirController();
            }
            else if (Command.IndexOf("0D 20 10 12 00 01 00 FF") >= 0) //新风关
            {
                //await CloseLivingroomAC();
                await CloseAirController();
            }

            else if (Command.IndexOf("0D 20 10 13 00 01 00 FF") >= 0) //地暖开
            {
                await OpenHeatSystem();
            }
            else if (Command.IndexOf("0D 20 10 14 00 01 00 FF") >= 0) //地暖关
            {
                await CloseHeatSystem();
            }



            //OF温控面板 -----------------------------------------------------
            else if(Command.IndexOf("0F 20 00 36 00 01 00 00") >= 0)    //关闭温控器 关闭全家空调
            {
                await _hvacHelper.TurnOffAC(0);
                await _hvacHelper.TurnOffAC(1);
                await CloseLivingroomAC();
                await CloseWorkroomAC();
            }
            else if(Command.IndexOf("0F 20 00 36 00 01 00 01") >= 0)   //打开空调温按钮,默认打开书房空调
            {
                await OpenWorkroomAC(); //打开空调
                
            }
            else if (Command.IndexOf("0F 20 00 32 00 01 00 00") >= 0)   //面板切换制热模式
            {
                _logger.LogInformation("面板切换制冷模式");
                if (_hVacSelected == HVACSelected.WorkRoom)
                {
                    await _hvacHelper.SetMode(2, WorkMode.Cool);
                    var obj = _hvacHelper.GetACStateObject(2);
                    obj.Mode = WorkMode.Cool;
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/02/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
                else if (_hVacSelected == HVACSelected.LivingRoom)
                {
                    await _hvacHelper.SetMode(3, WorkMode.Cool);
                    var obj = _hvacHelper.GetACStateObject(3);
                    obj.Mode = WorkMode.Cool;
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/03/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
            }
            else if (Command.IndexOf("0F 20 00 32 00 01 00 01") >= 0)   //面板切换制热模式
            {
                _logger.LogInformation("面板切换制热模式");
                if(_hVacSelected == HVACSelected.WorkRoom)
                {
                    await _hvacHelper.SetMode(2, WorkMode.Heat);
                    var obj = _hvacHelper.GetACStateObject(2);
                    obj.Mode = WorkMode.Heat;
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/02/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
                else if(_hVacSelected == HVACSelected.LivingRoom)
                {
                    await _hvacHelper.SetMode(3, WorkMode.Heat);
                    var obj = _hvacHelper.GetACStateObject(3);
                    obj.Mode = WorkMode.Heat;
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/03/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);

                }
            }
            else if (Command.IndexOf("0F 20 00 32 00 01 00 02") >= 0)   //面板切换换气模式
            {
                _logger.LogInformation("面板切换换气模式");
                if (_hVacSelected == HVACSelected.WorkRoom)
                {
                    await _hvacHelper.SetMode(2, WorkMode.Fan);
                    var obj = _hvacHelper.GetACStateObject(2);
                    obj.Mode = WorkMode.Fan;
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/02/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
                else if (_hVacSelected == HVACSelected.LivingRoom)
                {
                    await _hvacHelper.SetMode(3, WorkMode.Fan);
                    var obj = _hvacHelper.GetACStateObject(3);
                    obj.Mode = WorkMode.Fan;
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/03/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
            }
            else if (Command.IndexOf("0F 20 00 32 00 01 00 03") >= 0)   //面板切换抽湿模式
            {
                _logger.LogInformation("面板切换抽湿模式");
                if (_hVacSelected == HVACSelected.WorkRoom)
                {
                    await _hvacHelper.SetMode(2, WorkMode.Dry);
                    var obj = _hvacHelper.GetACStateObject(2);
                    obj.Mode = WorkMode.Dry;
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/02/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
                else if (_hVacSelected == HVACSelected.LivingRoom)
                {
                    await _hvacHelper.SetMode(3, WorkMode.Dry);
                    var obj = _hvacHelper.GetACStateObject(3);
                    obj.Mode = WorkMode.Dry;
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/03/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
            }
            else if(Command.IndexOf("0F 20 00 35 00 01 00") == 0)   //设置温度
            {
                var data = StringToByteArray(Command);
                
                if(_hVacSelected == HVACSelected.LivingRoom)
                {
                    _logger.LogInformation("设置客厅温度:" + data[7]);
                    await _hvacHelper.SetTemperature(3, (float)data[7]);
                    var obj = _hvacHelper.GetACStateObject(3);
                    obj.TemperatureSet = (int)data[7];
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/03/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
                else if(_hVacSelected == HVACSelected.WorkRoom)
                {
                    _logger.LogInformation("设置书房温度:" + data[7]);
                    await _hvacHelper.SetTemperature(2, (float)data[7]);
                    var obj = _hvacHelper.GetACStateObject(2);
                    obj.TemperatureSet = (int)data[7];
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/02/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
                
            }
            else if(Command.IndexOf("0F 20 00 34 00 01 00") == 0)
            {
                var data = StringToByteArray(Command);
                if (_mqttHelper != null)
                {
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Panel/Temperature")
                       .WithPayload(data[7].ToString())
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
            }
            

        }

        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        #region ===== 控制函数 =======
        public async Task HomeMode()
        {
            var cmds = new List<string>();
            cmds.Add("0B 06 10 22 00 00");
            cmds.Add("0B 06 10 23 00 00");
            cmds.Add("0B 06 10 21 00 01");
            await _listener.SendCommand(cmds);
            CurrentStateMode = StateMode.Home;
            var message = new MqttApplicationMessageBuilder().WithTopic("Home/Mode/Status")
                       .WithPayload("0")
                       .WithAtLeastOnceQoS()
                       .Build();
            await _mqttHelper.Publish(message);
        }


        public async Task OutMode()
        {
            var cmds = new List<string>();
            cmds.Add("0B 06 10 21 00 00");
            cmds.Add("0B 06 10 23 00 00");
            cmds.Add("0B 06 10 22 00 01");
            await _listener.SendCommand(cmds);
            CurrentStateMode = StateMode.Out;
            var message = new MqttApplicationMessageBuilder().WithTopic("Home/Mode/Status")
                       .WithPayload("1")
                       .WithAtLeastOnceQoS()
                       .Build();
            await _mqttHelper.Publish(message);
        }

        public async Task GetPanelTemperature()
        {
            var cmds = new List<string>();
            cmds.Add("0F 20 00 34 00 01");
            await _listener.SendCommand(cmds,false);
        }

        public async Task ReadMode()
        {
            var cmds = new List<string>();
            cmds.Add("0B 06 10 21 00 00");
            cmds.Add("0B 06 10 22 00 00");
            cmds.Add("0B 06 10 23 00 01");
            await _listener.SendCommand(cmds);
            CurrentStateMode = StateMode.Read;
            var message = new MqttApplicationMessageBuilder().WithTopic("Home/Mode/Status")
                       .WithPayload("2")
                       .WithAtLeastOnceQoS()
                       .Build();
            await _mqttHelper.Publish(message);
        }

        public async Task OpenAll()
        {
            await LightSwitch("LR1_Brightness", "100");
            await LightSwitch("LR1_ColorTemperature", "50");
            await LightSwitch("LR2_Brightness", "100");
            await LightSwitch("LR2_ColorTemperature", "50");
            await LightSwitch("LR3_Brightness", "100");
            await LightSwitch("LR3_ColorTemperature", "50");
            await LightSwitch("LR4_Brightness", "100");
            await LightSwitch("LR4_ColorTemperature", "50");
            await LightSwitch("LR5_Brightness", "100");
            await LightSwitch("LR5_ColorTemperature", "50");
            await LightSwitch("LR6_Brightness", "100");
            await LightSwitch("LR6_ColorTemperature", "50");
            await LightSwitch("LR7_Brightness", "100");
            await LightSwitch("LR7_ColorTemperature", "50");
            await LightSwitch("LR8_Brightness", "100");
            await LightSwitch("LR8_ColorTemperature", "50");
            await LightSwitch("LR9_Brightness", "100");
            await LightSwitch("LR9_ColorTemperature", "50");
            await LightSwitch("LR10_Brightness", "100");
            await LightSwitch("LR10_ColorTemperature", "50");

            await LightSwitch("LRStripTvColor", "35,80,100");
            await LightSwitch("LRStripTvColorTemperature", "50");
            await LightSwitch("LRStrip1Color", "35,80,100");
            await LightSwitch("LRStrip1ColorTemperature", "50");
            await LightSwitch("LRStrip2Color", "35,80,100");
            await LightSwitch("LRStrip2ColorTemperature", "50");
            await LightSwitch("LRStrip3Color", "35,80,100");
            await LightSwitch("LRStrip3ColorTemperature", "50");
            await LightSwitch("LRStrip4Color", "35,80,100");
            await LightSwitch("LRStrip4ColorTemperature", "50");
            await LightSwitch("LRStrip5Color", "35,80,100");
            await LightSwitch("LRStrip5ColorTemperature", "50");
            await LightSwitch("LRStrip6Color", "35,80,100");
            await LightSwitch("LRStrip6ColorTemperature", "50");
            await LightSwitch("KitchenSripColor", "35,80,100");
            await LightSwitch("KitchenSripColorTemperature", "50");

        }

        public async Task CloseAll()
        {
            await LightSwitch("LR1_Brightness", "OFF");
            await LightSwitch("LR2_Brightness", "OFF");
            await LightSwitch("LR3_Brightness", "OFF");
            await LightSwitch("LR4_Brightness", "OFF");
            await LightSwitch("LR5_Brightness", "OFF");
            await LightSwitch("LR6_Brightness", "OFF");
            await LightSwitch("LR7_Brightness", "OFF");
            await LightSwitch("LR8_Brightness", "OFF");
            await LightSwitch("LR9_Brightness", "OFF");
            await LightSwitch("LR10_Brightness", "OFF");
            await LightSwitch("LRStripTvColorTemperature", "OFF");
            await LightSwitch("LRStrip1ColorTemperature", "OFF");
            await LightSwitch("LRStrip2ColorTemperature", "OFF");
            await LightSwitch("LRStrip3ColorTemperature", "OFF");
            await LightSwitch("LRStrip4ColorTemperature", "OFF");
            await LightSwitch("LRStrip5ColorTemperature", "OFF");
            await LightSwitch("LRStrip6ColorTemperature", "OFF");
            await LightSwitch("KitchenSripColorTemperature", "OFF");
            await LightSwitch("HueGoColor", "OFF");
        }

        public async Task OpenKitchen()
        {
            var cmds = new List<string>();
            cmds.Add("0C 06 10 24 00 01");
            await _listener.SendCommand(cmds);
            await LightSwitch("KR1Brightness", "100");
            await LightSwitch("KR1ColorTemperature", "50");
            await LightSwitch("KR2Brightness", "100");
            await LightSwitch("KR2ColorTemperature", "50");
            await LightSwitch("KR3Brightness", "100");
            await LightSwitch("KR3ColorTemperature", "50");
            await LightSwitch("KR4Brightness", "100");
            await LightSwitch("KR4ColorTemperature", "50");
            await LightSwitch("Table1Brightness", "100");
            await LightSwitch("Table1ColorTemperature", "50");
            await LightSwitch("Table2Brightness", "100");
            await LightSwitch("Table2ColorTemperature", "50");
        }

        public async Task CloseKitchen()
        {
            var cmds = new List<string>();
            cmds.Add("0C 06 10 24 00 00");
            await _listener.SendCommand(cmds);
            await LightSwitch("KR1Brightness", "OFF");
            await LightSwitch("KR2Brightness", "OFF");
            await LightSwitch("KR3Brightness", "OFF");
            await LightSwitch("KR4Brightness", "OFF");
            await LightSwitch("Table1Brightness", "OFF");
            await LightSwitch("Table2Brightness", "OFF");
        }

        #region 空调
        public async Task OpenWorkroomAC()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 25 00 01");
            await _listener.SendCommand(cmds);
            _hVacSelected = HVACSelected.WorkRoom;
            await _hvacHelper.TurnOnAC(2);
            await OpenACPanel(2);
            
        }

        public async Task FlashWorkroomBackgroundLight()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 25 00 00");
            await _listener.SendCommand(cmds);
            await Task.Delay(500);
            cmds.Clear();
            cmds.Add("0D 06 10 25 00 01");
            await _listener.SendCommand(cmds);
        }

        public async Task CloseWorkroomAC()
        {
            ///点亮按键灯
            var cmds = new List<string>();
            cmds.Add("0D 06 10 25 00 00");
            await _listener.SendCommand(cmds);
            ///关闭空调
            await _hvacHelper.TurnOffAC(2);
            await CloseACPanel();
        }

        public async Task OpenLivingroomAC()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 26 00 01");
            await _listener.SendCommand(cmds);
            _hVacSelected = HVACSelected.LivingRoom;
            await _hvacHelper.TurnOnAC(3);
            await OpenACPanel(3);
        }

        public async Task FlashLivingroomBackgroundLight()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 26 00 00");
            await _listener.SendCommand(cmds);
            await Task.Delay(500);
            cmds.Clear();
            cmds.Add("0D 06 10 26 00 01");
            await _listener.SendCommand(cmds);
        }

        public async Task CloseLivingroomAC()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 26 00 00");
            await _listener.SendCommand(cmds);
            _hVacSelected = HVACSelected.WorkRoom;
            await _hvacHelper.TurnOffAC(3);
            await CloseACPanel();
        }

        public async Task OpenACPanel(int id)
        {
            var obj = _hvacHelper.GetACStateObject(id);
            if (obj != null)
            {
                var cmds = new List<string>();
                cmds.Add("0F 06 00 36 00 01");
                switch (obj.Mode)
                {
                    case WorkMode.Cool:
                        cmds.Add("0F 06 00 32 00 00");  //设定模式为制冷
                        break;
                    case WorkMode.Heat:
                        cmds.Add("0F 06 00 32 00 01");  //设定模式为制热
                        break;
                    case WorkMode.Fan:
                        cmds.Add("0F 06 00 32 00 02");  //设定模式为换气
                        break;
                    case WorkMode.Dry:
                        cmds.Add("0F 06 00 32 00 03");  //设定模式为除湿
                        break;
                    default:
                        cmds.Add("0F 06 00 32 00 00");  //设定模式为制冷
                        break;
                }
                cmds.Add("0F 06 00 35 00 " + obj.TemperatureSet.ToString("X2"));
                switch (obj.Fan)
                {
                    case Fanspeed.Hight:
                        cmds.Add("0F 06 00 33 00 03");  //设定风速为高
                        break;
                    case Fanspeed.Middle:
                        cmds.Add("0F 06 00 33 00 02");
                        break;
                    case Fanspeed.Low:
                        cmds.Add("0F 06 00 33 00 01");
                        break;
                    default:
                        cmds.Add("0F 06 00 33 00 01");
                        break;
                }
                await _listener.SendCommand(cmds);
            }
        }

        public async Task UpdateACPanel()
        {
            var selectedid = -1;
            if (_hVacSelected == HVACSelected.WorkRoom)
            {
                selectedid = 2;
            }
            else if (_hVacSelected == HVACSelected.LivingRoom)
            {
                selectedid = 3;
            }
            else
            {
                return;
            }
            var obj = _hvacHelper.GetACStateObject(selectedid);
            if (obj != null)
            {
                var cmds = new List<string>();
                cmds.Add("0F 06 00 36 00 01");
                switch (obj.Mode)
                {
                    case WorkMode.Cool:
                        cmds.Add("0F 06 00 32 00 00");  //设定模式为制冷
                        break;
                    case WorkMode.Heat:
                        cmds.Add("0F 06 00 32 00 01");  //设定模式为制热
                        break;
                    case WorkMode.Fan:
                        cmds.Add("0F 06 00 32 00 02");  //设定模式为换气
                        break;
                    case WorkMode.Dry:
                        cmds.Add("0F 06 00 32 00 03");  //设定模式为除湿
                        break;
                    default:
                        cmds.Add("0F 06 00 32 00 00");  //设定模式为制冷
                        break;
                }
                cmds.Add("0F 06 00 35 00 " + obj.TemperatureSet.ToString("X2"));
                switch (obj.Fan)
                {
                    case Fanspeed.Hight:
                        cmds.Add("0F 06 00 33 00 03");  //设定风速为高
                        break;
                    case Fanspeed.Middle:
                        cmds.Add("0F 06 00 33 00 02");
                        break;
                    case Fanspeed.Low:
                        cmds.Add("0F 06 00 33 00 01");
                        break;
                    default:
                        cmds.Add("0F 06 00 33 00 01");
                        break;
                }
                await _listener.SendCommand(cmds);
            }
        }


        public async Task CloseACPanel()
        {
            var obj1 = _hvacHelper.GetACStateObject(2);
            var obj2 = _hvacHelper.GetACStateObject(3);
            var cmds = new List<string>();

            if (obj1.Switch == SwitchState.close && obj2.Switch == SwitchState.close)
            {
                cmds.Add("0F 06 00 36 00 00");
                await _listener.SendCommand(cmds);
                _hVacSelected = HVACSelected.None;
                return;
            }
            else
            {
                if (obj1.Switch == SwitchState.open)
                {
                    _logger.LogInformation("书房");
                    await OpenACPanel(2);
                    _hVacSelected = HVACSelected.WorkRoom;
                    return;
                }
                if (obj2.Switch == SwitchState.open)
                {
                    _logger.LogInformation("客厅");
                    await OpenACPanel(3);
                    _hVacSelected = HVACSelected.LivingRoom;
                    return;
                }
                cmds.Add("0F 06 00 36 00 00");
                await _listener.SendCommand(cmds);
            }
        }
        #endregion

        /// <summary>
        /// 设置面板背景灯
        /// </summary>
        /// <param name="panelid">面板id,如：0B,0C,0D,0E,0F</param>
        /// <param name="buttonid">按键id,如21,22,23,24,25,26</param>
        /// <param name="value">01开,00关</param>
        /// <returns></returns>
        public async Task SetBackgroudLight(string panelid, string buttonid, int value)
        {
            var cmds = new List<string>();
            cmds.Add(string.Format("{0} 06 10 {1} 00 {2}", panelid, buttonid, value.ToString("X2")));
            await _listener.SendCommand(cmds);
        }

        public async Task OpenHeatSystem()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 24 00 00");
            cmds.Add("0D 06 10 23 00 01");
            await _listener.SendCommand(cmds);
        }

        public async Task CloseHeatSystem()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 23 00 00");
            cmds.Add("0D 06 10 24 00 01");
            await _listener.SendCommand(cmds);
        }

        public async Task OpenAirController()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 22 00 00");
            cmds.Add("0D 06 10 21 00 01");
            await _listener.SendCommand(cmds);
        }

        public async Task CloseAirController()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 21 00 00");
            cmds.Add("0D 06 10 22 00 01");
            await _listener.SendCommand(cmds);
        }

        public async Task OpenMainLight()
        {
            var cmds = new List<string>();
            cmds.Add("0C 06 10 26 00 01");
            await _listener.SendCommand(cmds);
            _sensorHelper.OpenMainLight();
        }

        public async Task CloseMainLight()
        {
            var cmds = new List<string>();
            cmds.Add("0C 06 10 26 00 00");
            await _listener.SendCommand(cmds);
            _sensorHelper.CloseMainLight();
        }

        /// <summary>
        /// 打开书房灯光
        /// </summary>
        /// <returns></returns>
        public async Task OpenWorkroom()
        {
            //var cmds = new List<string>();
            //cmds.Add("0D 06 10 26 00 00");
            //await _listener.SendCommand(cmds);
            await LightSwitch("WR1Brightness", "ON");
            await LightSwitch("WR2Brightness", "ON");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 关闭书房灯光
        /// </summary>
        /// <returns></returns>
        public async Task CloseWorkroom()
        {
            //var cmds = new List<string>();
            //cmds.Add("0D 06 10 26 00 00");
            //await _listener.SendCommand(cmds);
            await LightSwitch("WR1Brightness", "OFF");
            await LightSwitch("WR2Brightness", "OFF");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 打开过道灯光
        /// </summary>
        /// <returns></returns>
        public async Task OpenAisle(int val=100)
        {
            await LightSwitch("Aisle1Brightness", val.ToString());
            await LightSwitch("Aisle2Brightness", val.ToString());
        }

        public async Task CloseAisle()
        {
            await LightSwitch("Aisle1Brightness", "OFF");
            await LightSwitch("Aisle2Brightness", "OFF");
        }

        /// <summary>
        /// 打开门道灯光
        /// </summary>
        /// <returns></returns>
        public async Task OpenDoor(int val = 100)
        {
            await LightSwitch("Door1Brightness", val.ToString());
            await LightSwitch("Door2Brightness", val.ToString());
        }

        public async Task CloseDoor()
        {
            await LightSwitch("Door1Brightness", "OFF");
            await LightSwitch("Door2Brightness", "OFF");
        }

        public async Task LightLivingRoomSet(int brightness = 100,int temperature = 50)
        {
            await LightSwitch("LR1_Brightness", brightness.ToString());
            await LightSwitch("LR1_ColorTemperature", temperature.ToString());
            await LightSwitch("LR2_Brightness", brightness.ToString());
            await LightSwitch("LR2_ColorTemperature", temperature.ToString());
            await LightSwitch("LR3_Brightness", brightness.ToString());
            await LightSwitch("LR3_ColorTemperature", temperature.ToString());
            await LightSwitch("LR4_Brightness", brightness.ToString());
            await LightSwitch("LR4_ColorTemperature", temperature.ToString());
            await LightSwitch("LR5_Brightness", brightness.ToString());
            await LightSwitch("LR5_ColorTemperature", temperature.ToString());
            await LightSwitch("LR6_Brightness", brightness.ToString());
            await LightSwitch("LR6_ColorTemperature", temperature.ToString());
            await LightSwitch("LR7_Brightness", brightness.ToString());
            await LightSwitch("LR7_ColorTemperature", temperature.ToString());
            await LightSwitch("LR8_Brightness", brightness.ToString());
            await LightSwitch("LR8_ColorTemperature", temperature.ToString());
            await LightSwitch("LR9_Brightness", brightness.ToString());
            await LightSwitch("LR9_ColorTemperature", temperature.ToString());
            await LightSwitch("LR10_Brightness", brightness.ToString());
            await LightSwitch("LR10_ColorTemperature", temperature.ToString());

        }

        public async Task StripLivingRoomSet(int color=35,int saturation=80,int brightness=100)
        {
            await LightSwitch("HueGoColor", string.Format("{0},{1},{2}", color, saturation, brightness));//HueGoColorTemperature
            //await LightSwitch("HueGoColorTemperature", temperature);
            await LightSwitch("LRStripTvColor", string.Format("{0},{1},{2}",color,saturation,brightness));
            //await LightSwitch("LRStripTvColorTemperature", temperature);
            await LightSwitch("LRStrip1Color", string.Format("{0},{1},{2}",color,saturation,brightness));
            //await LightSwitch("LRStrip1ColorTemperature", temperature);
            await LightSwitch("LRStrip2Color", string.Format("{0},{1},{2}",color,saturation,brightness));
            //await LightSwitch("LRStrip2ColorTemperature", temperature);
            await LightSwitch("LRStrip3Color", string.Format("{0},{1},{2}",color,saturation,brightness));
            //await LightSwitch("LRStrip3ColorTemperature", temperature);
            await LightSwitch("LRStrip4Color", string.Format("{0},{1},{2}",color,saturation,brightness));
            //await LightSwitch("LRStrip4ColorTemperature", temperature);
            await LightSwitch("LRStrip5Color", string.Format("{0},{1},{2}",color,saturation,brightness));
            //await LightSwitch("LRStrip5ColorTemperature", temperature);
            await LightSwitch("LRStrip6Color", string.Format("{0},{1},{2}",color,saturation,brightness));
            //await LightSwitch("LRStrip6ColorTemperature", temperature);
            await LightSwitch("KitchenSripColor", string.Format("{0},{1},{2}",color,saturation,brightness));
            //await LightSwitch("KitchenSripColorTemperature", temperature);
        }

        public async Task LightBedRoomSet(int brightness = 100, int temperature = 50)
        {
            await LightSwitch("LR1_Brightness", brightness.ToString());
            await LightSwitch("LR1_ColorTemperature", temperature.ToString());
        }

        public async Task StripBedRoomSet(int color = 35, int saturation = 80, int brightness = 100, int temperature = 50)
        {
            await LightSwitch("CloakRoomStripColor", string.Format("{0},{1},{2}", color, saturation, brightness));
            await LightSwitch("BedHeadStripColor", string.Format("{0},{1},{2}", color, saturation, brightness));
            await LightSwitch("BedStripColor", string.Format("{0},{1},{2}", color, saturation, brightness));
        }

        public async Task StripGuestRoomSet(int color = 35, int saturation = 80, int brightness = 100, int temperature = 50)
        {
            await LightSwitch("GuestRoomStripColor", string.Format("{0},{1},{2}", color, saturation, brightness));
        }

        /// <summary>
        /// 客厅情景模式
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task SceneLivingRoomSet(SceneState state)
        {
            _logger.LogInformation("SceneLivingRoomSet :" + state.ToString());
            switch (state)
            {
                case SceneState.Brightness:
                    await OpenAll();
                    break;
                case SceneState.Relax:
                    await LightLivingRoomSet(60,60);
                    await StripLivingRoomSet(35, 60, 60);
                    break;
                case SceneState.TV:
                    await LightLivingRoomSet(0, 50);
                    await StripLivingRoomSet(270, 97, 25);
                    break;
                case SceneState.Sunset:
                    await LightLivingRoomSet(0, 50);
                    await StripLivingRoomSet(10, 63, 60);
                    break;
                case SceneState.Close:
                    await CloseAll();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 主卧衣帽间情景模式
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task SceneBedRoomSet(SceneState state)
        {
            _logger.LogInformation("SceneBedRoomSet :" + state.ToString());
            switch (state)
            {
                case SceneState.Brightness:
                    await LightSwitch("BedStripColor", "35,80,100");
                    await LightSwitch("BedHeadStripColor", "35,80,100");
                    await LightSwitch("CloakRoomStripColor", "35,80,100");
                    await LightSwitch("BedroomDoorBrightness", "100");
                    await LightSwitch("BedroomWnd1Brightness", "100");
                    await LightSwitch("BedroomWnd2Brightness", "100");
                    await LightSwitch("BedRoomMainBrightness", "100");
                    await LightSwitch("BedRoomHead1Brightness", "100");
                    await LightSwitch("BedRoomHead2Brightness", "100");
                    await LightSwitch("BedRoomTail1Brightness", "100");
                    await LightSwitch("BedRoomTail2Brightness", "100");
                    await LightSwitch("BedRoomTail3Brightness", "100");
                    await LightSwitch("CloakRoomWndBrightness", "100");
                    await LightSwitch("CloakRoom1Brightness", "100");
                    await LightSwitch("CloakRoom2Brightness", "100");
                    await LightSwitch("CloakRoom3Brightness", "100");
                    await LightSwitch("CloakRoom4Brightness", "100");

                    break;
                case SceneState.Relax:
                    await LightSwitch("BedStripColor", "35,80,60");
                    await LightSwitch("BedHeadStripColor", "35,80,60");
                    await LightSwitch("CloakRoomStripColor", "35,80,60");
                    await LightSwitch("BedroomDoorBrightness", "60");
                    await LightSwitch("BedroomWnd1Brightness", "60");
                    await LightSwitch("BedroomWnd2Brightness", "60");
                    await LightSwitch("BedRoomMainBrightness", "60");
                    await LightSwitch("BedRoomHead1Brightness", "60");
                    await LightSwitch("BedRoomHead2Brightness", "60");
                    await LightSwitch("BedRoomTail1Brightness", "60");
                    await LightSwitch("BedRoomTail2Brightness", "60");
                    await LightSwitch("BedRoomTail3Brightness", "60");
                    await LightSwitch("CloakRoomWndBrightness", "60");
                    await LightSwitch("CloakRoom1Brightness", "60");
                    await LightSwitch("CloakRoom2Brightness", "60");
                    await LightSwitch("CloakRoom3Brightness", "60");
                    await LightSwitch("CloakRoom4Brightness", "60");
                    break;
                case SceneState.TV:
                    await LightSwitch("BedStripColor", "270,97,25");
                    await LightSwitch("BedHeadStripColor", "260,80,25");
                    await LightSwitch("CloakRoomStripColor", "35,80,0");
                    await LightSwitch("BedroomDoorBrightness", "0");
                    await LightSwitch("BedroomWnd1Brightness", "0");
                    await LightSwitch("BedroomWnd2Brightness", "0");
                    await LightSwitch("BedRoomMainBrightness", "30");
                    await LightSwitch("BedRoomHead1Brightness", "0");
                    await LightSwitch("BedRoomHead2Brightness", "0");
                    await LightSwitch("BedRoomTail1Brightness", "0");
                    await LightSwitch("BedRoomTail2Brightness", "0");
                    await LightSwitch("BedRoomTail3Brightness", "0");
                    await LightSwitch("CloakRoomWndBrightness", "0");
                    await LightSwitch("CloakRoom1Brightness", "0");
                    await LightSwitch("CloakRoom2Brightness", "0");
                    await LightSwitch("CloakRoom3Brightness", "0");
                    await LightSwitch("CloakRoom4Brightness", "0");
                    break;
                case SceneState.Sunset:
                    await LightSwitch("BedStripColor", "10,63,60");
                    await LightSwitch("BedHeadStripColor", "10,63,60");
                    await LightSwitch("CloakRoomStripColor", "10,63,60");
                    await LightSwitch("BedroomDoorBrightness", "40");
                    await LightSwitch("BedroomWnd1Brightness", "40");
                    await LightSwitch("BedroomWnd2Brightness", "40");
                    await LightSwitch("BedRoomMainBrightness", "40");
                    await LightSwitch("BedRoomHead1Brightness", "40");
                    await LightSwitch("BedRoomHead2Brightness", "40");
                    await LightSwitch("BedRoomTail1Brightness", "40");
                    await LightSwitch("BedRoomTail2Brightness", "40");
                    await LightSwitch("BedRoomTail3Brightness", "40");
                    await LightSwitch("CloakRoomWndBrightness", "40");
                    await LightSwitch("CloakRoom1Brightness", "40");
                    await LightSwitch("CloakRoom2Brightness", "40");
                    await LightSwitch("CloakRoom3Brightness", "40");
                    await LightSwitch("CloakRoom4Brightness", "40");
                    break;
                case SceneState.Close:
                    await LightSwitch("BedStripColor", "35,80,0");
                    await LightSwitch("BedHeadStripColor", "35,80,0");
                    await LightSwitch("CloakRoomStripColor", "35,80,0");
                    await LightSwitch("BedroomDoorBrightness", "0");
                    await LightSwitch("BedroomWnd1Brightness", "0");
                    await LightSwitch("BedroomWnd2Brightness", "0");
                    await LightSwitch("BedRoomMainBrightness", "0");
                    await LightSwitch("BedRoomHead1Brightness", "0");
                    await LightSwitch("BedRoomHead2Brightness", "0");
                    await LightSwitch("BedRoomTail1Brightness", "0");
                    await LightSwitch("BedRoomTail2Brightness", "0");
                    await LightSwitch("BedRoomTail3Brightness", "0");
                    await LightSwitch("CloakRoomWndBrightness", "0");
                    await LightSwitch("CloakRoom1Brightness", "0");
                    await LightSwitch("CloakRoom2Brightness", "0");
                    await LightSwitch("CloakRoom3Brightness", "0");
                    await LightSwitch("CloakRoom4Brightness", "0");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 客房情景模式
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task SceneGuestRoomSet(SceneState state)
        {
            _logger.LogInformation("SceneGuestRoomSet :" + state.ToString());
            switch (state)
            {
                case SceneState.Brightness:
                    await LightSwitch("GuestRoomStripColor", "35,80,100");
                    await LightSwitch("GuestRoom1Brightness", "100");
                    await LightSwitch("GuestRoom2Brightness", "100");
                    await LightSwitch("GuestRoom3Brightness", "100");
                    await LightSwitch("GuestRoom4Brightness", "100");
                    await LightSwitch("GuestRoom5Brightness", "100");
                    await LightSwitch("GuestRoomMainBrightness", "100");

                    break;
                case SceneState.Relax:
                    await LightSwitch("GuestRoomStripColor", "35,80,60");
                    await LightSwitch("GuestRoom1Brightness", "60");
                    await LightSwitch("GuestRoom2Brightness", "60");
                    await LightSwitch("GuestRoom3Brightness", "60");
                    await LightSwitch("GuestRoom4Brightness", "60");
                    await LightSwitch("GuestRoom5Brightness", "60");
                    await LightSwitch("GuestRoomMainBrightness", "60");
                    break;
                case SceneState.TV:
                    await LightSwitch("GuestRoomStripColor", "270,97,25");
                    await LightSwitch("GuestRoom1Brightness", "0");
                    await LightSwitch("GuestRoom2Brightness", "0");
                    await LightSwitch("GuestRoom3Brightness", "0");
                    await LightSwitch("GuestRoom4Brightness", "0");
                    await LightSwitch("GuestRoom5Brightness", "0");
                    await LightSwitch("GuestRoomMainBrightness", "0");
                    break;
                case SceneState.Sunset:
                    await LightSwitch("GuestRoomStripColor", "10,63,60");
                    await LightSwitch("GuestRoom1Brightness", "30");
                    await LightSwitch("GuestRoom2Brightness", "30");
                    await LightSwitch("GuestRoom3Brightness", "30");
                    await LightSwitch("GuestRoom4Brightness", "30");
                    await LightSwitch("GuestRoom5Brightness", "30");
                    await LightSwitch("GuestRoomMainBrightness", "30");
                    break;
                case SceneState.Close:
                    await LightSwitch("GuestRoomStripColor", "35,80,0");
                    await LightSwitch("GuestRoom1Brightness", "0");
                    await LightSwitch("GuestRoom2Brightness", "0");
                    await LightSwitch("GuestRoom3Brightness", "0");
                    await LightSwitch("GuestRoom4Brightness", "0");
                    await LightSwitch("GuestRoom5Brightness", "0");
                    await LightSwitch("GuestRoomMainBrightness", "0");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 餐厨区情景模式
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task SceneDinnerRoomSet(SceneState state)
        {
            _logger.LogInformation("SceneDinnerRoomSet :" + state.ToString());
            switch (state)
            {
                case SceneState.Brightness:
                    await LightSwitch("Table1Brightness", "100");
                    await LightSwitch("Table2Brightness", "100");
                    await LightSwitch("KR1Brightness", "100");
                    await LightSwitch("KR2Brightness", "100");
                    await LightSwitch("KR3Brightness", "100");
                    await LightSwitch("KR4Brightness", "100");
                    break;
                case SceneState.Relax:
                    await LightSwitch("Table1Brightness", "60");
                    await LightSwitch("Table2Brightness", "60");
                    await LightSwitch("KR1Brightness", "60");
                    await LightSwitch("KR2Brightness", "60");
                    await LightSwitch("KR3Brightness", "60");
                    await LightSwitch("KR4Brightness", "60");
                    break;
                case SceneState.Close:
                    await LightSwitch("Table1Brightness", "0");
                    await LightSwitch("Table2Brightness", "0");
                    await LightSwitch("KR1Brightness", "0");
                    await LightSwitch("KR2Brightness", "0");
                    await LightSwitch("KR3Brightness", "0");
                    await LightSwitch("KR4Brightness", "0");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 书房情景模式
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task SceneWorkRoomSet(SceneState state)
        {
            _logger.LogInformation("SceneWorkRoomSet :" + state.ToString());
            switch (state)
            {
                case SceneState.Brightness:
                    await LightSwitch("WR1Brightness", "100");
                    await LightSwitch("WR2Brightness", "100");
                    break;
                case SceneState.Relax:
                    await LightSwitch("WR1Brightness", "60");
                    await LightSwitch("WR2Brightness", "60");
                    break;
                case SceneState.Close:
                    await LightSwitch("WR1Brightness", "0");
                    await LightSwitch("WR2Brightness", "0");
                    break;
                default:
                    break;
            }
        }

        #endregion

        private async Task LightSwitch (string itemname, string command)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("User-Agent", "HomeGaywate");

                    var content = new StringContent(command, Encoding.UTF8, "text/plain");
                    var result = await client.PostAsync(API_PATH + itemname, content);
                    if (!result.IsSuccessStatusCode)
                    {
                        _logger.LogError(result.StatusCode.ToString() + ":" + result.RequestMessage.ToString());
                    }
                }
                    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        public async Task CheckCurrentMode()
        {
            var ret = await _listener.SendCommand("0B 03 10 21 00 01");
            if (ret.IndexOf("0B 03 00 02 00 01") >= 0)
            {
                CurrentStateMode = StateMode.Home;
                var message = new MqttApplicationMessageBuilder().WithTopic("Home/Mode/Status")
                       .WithPayload("0")
                       .WithAtLeastOnceQoS()
                       .Build();
                await _mqttHelper.Publish(message);
                return;
            }
            ret = await _listener.SendCommand("0B 03 10 22 00 01");
            if (ret.IndexOf("0B 03 00 02 00 01") >= 0)
            {
                CurrentStateMode = StateMode.Out;
                var message = new MqttApplicationMessageBuilder().WithTopic("Home/Mode/Status")
                       .WithPayload("1")
                       .WithAtLeastOnceQoS()
                       .Build();
                await _mqttHelper.Publish(message);
                return;
            }
            ret = await _listener.SendCommand("0B 03 10 23 00 01");
            if (ret.IndexOf("0B 03 00 02 00 01") >= 0)
            {
                CurrentStateMode = StateMode.Read;
                var message = new MqttApplicationMessageBuilder().WithTopic("Home/Mode/Status")
                       .WithPayload("2")
                       .WithAtLeastOnceQoS()
                       .Build();
                await _mqttHelper.Publish(message);
                return;
            }
        }
    }

    public enum StateMode
    {
        Home,
        Out,
        Read,
        Alert
    }
    public enum HVACSelected
    {
        WorkRoom = 2,
        LivingRoom =3,
        Both = 1,
        None = 0
    }

    public enum SceneState
    {
        Brightness = 1,
        Relax = 2,
        TV = 3,
        Sunset = 4,
        Close = 0
    }
}

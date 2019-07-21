using Microsoft.Extensions.Logging;
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
        private static readonly HttpClient client = new HttpClient();
        private SwitchListener _listener;
        private HVACSelected _hVacSelected = HVACSelected.None;
        private readonly HvacHelper _hvacHelper;
        private SensorHelper _sensorHelper;
        private DateTime _lastHomeButonReceive;
        private DateTime _lastOutButonReceive;
        private DateTime _lastReadButonReceive;
        private DateTime _lastWorkRoomACButonReceive;
        private DateTime _lastLivingRoomACButonReceive;

        public StateMode CurrentStateMode { get; set; }

        public LightHelper(ILogger<LightHelper> logger, HvacHelper hvacHelper)
        {
            _logger = logger;
            _hvacHelper = hvacHelper;
        }

        public void SetListener(SwitchListener listener)
        {
            _listener = listener;
        }

        public void SetSensorHelper(SensorHelper sensorHelper)
        {
            _sensorHelper = sensorHelper;
        }

        public async Task OnReceiveCommand(string Command)
        {
            //面板OB
            if (Command.IndexOf("0B 20 10 11 00 01 00 FF") >= 0) //面板OB松开回家模式按键
            {
                _logger.LogInformation("时间:" + DateTime.Now);
                
                await HomeMode();
            }
            else if (Command.IndexOf("0B 20 10 11 00 01 00 00 8F") >= 0) //点亮回家模式时再按回家按钮
            {
                _logger.LogInformation("时间:" + DateTime.Now);
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
                await OpenACPanel(2);
                _hVacSelected = HVACSelected.WorkRoom;
            }
            else if (Command.IndexOf("0D 20 10 15 00 01 00 7F") >= 0) //点亮状态下点击按钮
            {
                await CloseWorkroomAC();
                await CloseACPanel();
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
                await OpenACPanel(3);
                _hVacSelected = HVACSelected.LivingRoom;
            }
            else if (Command.IndexOf("0D 20 10 16 00 01 00 7F") >= 0) //关闭客厅按钮
            {
                await CloseLivingroomAC();
                await CloseACPanel();
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
                await OpenACPanel(2);
            }
            else if(Command.IndexOf("0F 20 00 32 00 01 00 01") >= 0)   //面板切换制热模式
            {
                if(_hVacSelected == HVACSelected.WorkRoom)
                {
                    await _hvacHelper.SetMode(2, WorkMode.Heat);
                }else if(_hVacSelected == HVACSelected.LivingRoom)
                {
                    await _hvacHelper.SetMode(3, WorkMode.Heat);
                }
            }
            else if (Command.IndexOf("0F 20 00 32 00 01 00 02") >= 0)   //面板切换换气模式
            {
                if (_hVacSelected == HVACSelected.WorkRoom)
                {
                    await _hvacHelper.SetMode(2, WorkMode.Fan);
                }
                else if (_hVacSelected == HVACSelected.LivingRoom)
                {
                    await _hvacHelper.SetMode(3, WorkMode.Fan);
                }
            }
            else if (Command.IndexOf("0F 20 00 32 00 01 00 03") >= 0)   //面板切换抽湿模式
            {
                if (_hVacSelected == HVACSelected.WorkRoom)
                {
                    await _hvacHelper.SetMode(2, WorkMode.Dry);
                }
                else if (_hVacSelected == HVACSelected.LivingRoom)
                {
                    await _hvacHelper.SetMode(3, WorkMode.Dry);
                }
            }
            else if(Command.IndexOf("0F 20 00 35 00 01 00") == 0)   //设置温度
            {
                var data = StringToByteArray(Command);
                _logger.LogInformation("设置温度:" + data[7]);
                if(_hVacSelected == HVACSelected.LivingRoom)
                {
                    await _hvacHelper.SetTemperature(3, (float)data[7]);
                }
                else if(_hVacSelected == HVACSelected.WorkRoom)
                {
                    await _hvacHelper.SetTemperature(2, (float)data[7]);
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
        }


        public async Task OutMode()
        {
            var cmds = new List<string>();
            cmds.Add("0B 06 10 21 00 00");
            cmds.Add("0B 06 10 23 00 00");
            cmds.Add("0B 06 10 22 00 01");
            await _listener.SendCommand(cmds);
            CurrentStateMode = StateMode.Out;
        }


        public async Task ReadMode()
        {
            var cmds = new List<string>();
            cmds.Add("0B 06 10 21 00 00");
            cmds.Add("0B 06 10 22 00 00");
            cmds.Add("0B 06 10 23 00 01");
            await _listener.SendCommand(cmds);
            CurrentStateMode = StateMode.Read;
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

            await LightSwitch("LRStripTvColor", "35,50,100");
            await LightSwitch("LRStripTvColorTemperature", "50");
            await LightSwitch("LRStrip1Color", "35,50,100");
            await LightSwitch("LRStrip1ColorTemperature", "50");
            await LightSwitch("LRStrip2Color", "35,50,100");
            await LightSwitch("LRStrip2ColorTemperature", "50");
            await LightSwitch("LRStrip3Color", "35,50,100");
            await LightSwitch("LRStrip3ColorTemperature", "50");
            await LightSwitch("LRStrip4Color", "35,50,100");
            await LightSwitch("LRStrip4ColorTemperature", "50");
            await LightSwitch("LRStrip5Color", "35,50,100");
            await LightSwitch("LRStrip5ColorTemperature", "50");
            await LightSwitch("LRStrip6Color", "35,50,100");
            await LightSwitch("LRStrip6ColorTemperature", "50");
            await LightSwitch("KitchenSripColor", "35,50,100");
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
            var cmds = new List<string>();
            cmds.Add("0D 06 10 25 00 00");
            await _listener.SendCommand(cmds);
            _hVacSelected = HVACSelected.None;
            await _hvacHelper.TurnOffAC(2);
        }

        public async Task OpenLivingroomAC()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 26 00 01");
            await _listener.SendCommand(cmds);
            _hVacSelected = HVACSelected.LivingRoom;
            await _hvacHelper.TurnOnAC(3);
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
        }
        #endregion

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
        public async Task OpenAisle()
        {
            await LightSwitch("Aisle1Brightness", "ON");
            await LightSwitch("Aisle2Brightness", "ON");
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
        public async Task OpenDoor()
        {
            await LightSwitch("Door1Brightness", "ON");
            await LightSwitch("Door2Brightness", "ON");
        }

        public async Task CloseDoor()
        {
            await LightSwitch("Door1Brightness", "OFF");
            await LightSwitch("Door2Brightness", "OFF");
        }

        public async Task OpenACPanel(int id)
        {
            var obj = _hvacHelper.GetACStateObject(id);
            if(obj != null)
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

            if(obj1.Switch == SwitchState.close && obj2.Switch == SwitchState.close)
            {
                cmds.Add("0F 06 00 36 00 00");
                await _listener.SendCommand(cmds);
                return;
            }
            else
            {
                if(obj1.Switch == SwitchState.open)
                {
                    _logger.LogInformation("书房");
                    await OpenACPanel(2);
                    return;
                }
                if(obj2.Switch == SwitchState.open)
                {
                    _logger.LogInformation("客厅");
                    await OpenACPanel(3);
                    return;
                }
                cmds.Add("0F 06 00 36 00 00");
                await _listener.SendCommand(cmds);
            }
        }

        #endregion

        private async Task LightSwitch(string itemname, string command)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "HomeGaywate");

            var content = new StringContent(command, Encoding.UTF8, "text/plain");
            var result = await client.PostAsync("http://192.168.50.245:38080/rest/items/" + itemname, content);
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
}

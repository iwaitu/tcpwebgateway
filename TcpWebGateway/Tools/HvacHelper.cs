using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TcpWebGateway.Services;

namespace TcpWebGateway.Tools
{
    public class HvacHelper
    {
        private readonly ILogger _logger;
        private HvacListener _listener;
        private List<HvacStateObject> stateobjs = new List<HvacStateObject>();
        private MqttHelper _mqttHelper;
        private LightHelper _lightHelper;

        public HvacHelper(ILogger<HvacHelper> logger)
        {
            _logger = logger;
            
        }

        public void SetListener(HvacListener listener)
        {
            _listener = listener;
        }

        public void SetMqttListener(MqttHelper mqttHelper)
        {
            _mqttHelper = mqttHelper;
        }

        public void SetLightHelper(LightHelper lightHelper)
        {
            _lightHelper = lightHelper;
        }

        public async Task SyncAllState()
        {
            var cmds = new List<string>();
            cmds.Add("01 50 01 01 01 00");
            cmds.Add("01 50 01 01 01 01");
            cmds.Add("01 50 01 01 01 02");
            cmds.Add("01 50 01 01 01 03");
            await _listener.SendCommand(cmds);
        }

        public async Task OnReceiveData(string data)
        {
            var codes = new List<string>();
            if(data.IndexOf("01 50") ==0)
            {
                codes  = data.Split(" ").ToList();
                if(!stateobjs.Any(p=>p.Id == codes[5]))
                {
                    var obj = new HvacStateObject();
                    obj.Id = codes[5];
                    obj.Switch = (SwitchState)int.Parse(codes[6]);
                    obj.TemperatureSet = int.Parse(codes[7], System.Globalization.NumberStyles.HexNumber);
                    obj.Mode = (WorkMode)int.Parse(codes[8]);
                    obj.Fan = (Fanspeed)int.Parse(codes[9]);
                    obj.CurrentTemperature = int.Parse(codes[10], System.Globalization.NumberStyles.HexNumber);
                    stateobjs.Add(obj);
                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/" + codes[5] + "/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
                else
                {
                    var obj = stateobjs.FirstOrDefault(p => p.Id == codes[5]);
                    obj.Switch = (SwitchState)int.Parse(codes[6]);
                    obj.TemperatureSet = int.Parse(codes[7], System.Globalization.NumberStyles.HexNumber);
                    obj.Mode = (WorkMode)int.Parse(codes[8]);
                    obj.Fan = (Fanspeed)int.Parse(codes[9]);
                    obj.CurrentTemperature = int.Parse(codes[10], System.Globalization.NumberStyles.HexNumber);

                    var message = new MqttApplicationMessageBuilder().WithTopic("Home/Sanling/" + codes[5] + "/Status")
                       .WithPayload(JsonConvert.SerializeObject(obj))
                       .WithAtLeastOnceQoS()
                       .Build();
                    await _mqttHelper.Publish(message);
                }
                await _lightHelper.UpdateACPanel();
            }
        }

        public async Task TurnOnAC(int id)
        {
            await _listener.SendCommand(string.Format("01 31 01 01 01 {0}" , id.ToString("X2")));
            var obj = stateobjs.FirstOrDefault(p => p.Id == id.ToString("X2"));
            if(obj != null)
            {
                obj.Switch = SwitchState.open;
            }
        }

        public async Task TurnOffAC(int id)
        {
            await _listener.SendCommand(string.Format("01 31 02 01 01 {0}" , id.ToString("X2")));
            var obj = stateobjs.FirstOrDefault(p => p.Id == id.ToString("X2"));
            if (obj != null)
            {
                obj.Switch = SwitchState.close;
            }
        }

        public async Task SetTemperature(int id,float temperature)
        {
            int iTemperature = (int)temperature;
            var cmd = string.Format("01 32 {0} 01 01 {1}", iTemperature.ToString("X2"), id.ToString("X2"));
            //_logger.LogInformation("SendCMD:" + cmd);
            await _listener.SendCommand(cmd);
        }

        public async Task SetMode(int id, WorkMode mode)
        {
            int iMode = (int)mode;
            await _listener.SendCommand(string.Format("01 33 {0} 01 01 {1}", iMode.ToString("X2"), id.ToString("X2")));
        }

        public async Task SetFanspeed(int id, Fanspeed speed)
        {
            int iSpeed = (int)speed;
            await _listener.SendCommand(string.Format("01 34 {0} 01 01 {1}", iSpeed.ToString("X2"), id.ToString("X2")));
        }

        public HvacStateObject GetACStateObject(int id)
        {
            var obj = stateobjs.FirstOrDefault(p => p.Id == id.ToString("X2"));
            return obj;
        }

        public HvacStateObject GetACStateObject(string id)
        {
            var obj = stateobjs.FirstOrDefault(p => p.Id == id);
            return obj;
        }

        public async Task UpdateStateObject(HvacStateObject target)
        {
            var obj = stateobjs.FirstOrDefault(p => p.Id == target.Id);
            if(obj != null)
            {
                if(obj.Switch != target.Switch)
                {
                    obj.Switch = target.Switch;
                    await TurnOnAC(int.Parse(obj.Id));
                }else if(obj.Mode != target.Mode)
                {
                    obj.Mode = target.Mode;
                    await SetMode(int.Parse(obj.Id), (WorkMode)target.Mode);
                }else if(obj.Fan != target.Fan)
                {
                    obj.Fan = target.Fan;
                    await SetFanspeed(int.Parse(obj.Id), (Fanspeed)target.Fan);
                }else if(obj.TemperatureSet != target.TemperatureSet)
                {
                    obj.TemperatureSet = target.TemperatureSet;
                    await SetTemperature(int.Parse(obj.Id), target.TemperatureSet);
                }
            }
        }
    }

    /// <summary>
    /// 空调内机对象
    /// </summary>
    public class HvacStateObject
    {
        public string Id { get; set; }
        /// <summary>
        /// 开关状态
        /// </summary>
        public SwitchState Switch { get; set; }
        /// <summary>
        /// 工作模式
        /// </summary>
        public WorkMode Mode { get; set; }
        /// <summary>
        /// 风速
        /// </summary>
        public Fanspeed Fan { get; set; }
        /// <summary>
        /// 设定温度
        /// </summary>
        public int TemperatureSet { get; set; }
        /// <summary>
        /// 环境温度
        /// </summary>
        public int CurrentTemperature { get; set; }
    }

    public enum SwitchState
    {
        open = 1,
        close = 2
    }

    public enum WorkMode
    {
        Cool = 1,
        Heat = 8,
        Fan = 4,
        Dry = 2
    }

    public enum Fanspeed
    {
        Hight = 1,
        Middle = 2,
        Low = 4
    }
}

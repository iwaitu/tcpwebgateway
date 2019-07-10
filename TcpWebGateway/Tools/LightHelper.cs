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
    public class LightHelper
    {
        private static readonly HttpClient client = new HttpClient();
        private SwitchListener _listener;
        private HVACSelected _hVacSelected = HVACSelected.None;
        public LightHelper(SwitchListener listener)
        {
            _listener = listener;
        }

        public async Task OnReceiveCommand(string Command)
        {
            //面板OB
            if (Command.IndexOf("0B 20 10 11 00 01 00 FF") >= 0) //面板OB松开回家模式按键
            {
                await HomeMode();
            }
            else if(Command.IndexOf("0B 20 10 12 00 01 00 FF")>= 0) //松开外出模式按键
            {
                await OutMode();
            }
            else if (Command.IndexOf("0B 20 10 13 00 01 00 FF")>=0) //松开阅读模式按键
            {
                await ReadMode();
            }
            //面板OC
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
            //OD
            else if (Command.IndexOf("0D 20 10 15 00 01 00 FF") >= 0) //打开书房空调
            {
                await OpenWorkroomAC();
            }
            else if (Command.IndexOf("0D 20 10 15 00 01 00 7F") >= 0) //关闭书房按钮
            {
                await CloseWorkroomAC();
            }
            else if (Command.IndexOf("0D 20 10 16 00 01 00 FF") >= 0) //打开客厅空调
            {
                await OpenLivingroomAC();
            }
            else if (Command.IndexOf("0D 20 10 16 00 01 00 7F") >= 0) //关闭客厅按钮
            {
                await CloseLivingroomAC();
            }
            else if (Command.IndexOf("0D 20 10 11 00 01 00 FF") >= 0) //新风开
            {
                //await CloseLivingroomAC();
            }
            else if (Command.IndexOf("0D 20 10 12 00 01 00 FF") >= 0) //新风关
            {
                //await CloseLivingroomAC();
            }
            //OF温控面板
            else if(Command.IndexOf("0F 20 00 39 00 01 00 01 2D") >= 0)
            {
                
            }
        }

        public async Task HomeMode()
        {
            var cmds = new List<string>();
            cmds.Add("0B 06 10 22 00 00");
            cmds.Add("0B 06 10 23 00 00");
            cmds.Add("0B 06 10 21 00 01");
            await _listener.SendCommand(cmds);
        }


        public async Task OutMode()
        {
            var cmds = new List<string>();
            cmds.Add("0B 06 10 21 00 00");
            cmds.Add("0B 06 10 23 00 00");
            cmds.Add("0B 06 10 22 00 01");
            await _listener.SendCommand(cmds);
        }


        public async Task ReadMode()
        {
            var cmds = new List<string>();
            cmds.Add("0B 06 10 21 00 00");
            cmds.Add("0B 06 10 22 00 00");
            cmds.Add("0B 06 10 23 00 01");
            await _listener.SendCommand(cmds);
        }

        public async Task OpenAll()
        {
            await LightSwitch("LR1_Brightness", "ON");
            await LightSwitch("LR2_Brightness", "ON");
            await LightSwitch("LR3_Brightness", "ON");
            await LightSwitch("LR4_Brightness", "ON");
            await LightSwitch("LR5_Brightness", "ON");
            await LightSwitch("LR6_Brightness", "ON");
            await LightSwitch("LR7_Brightness", "ON");
            await LightSwitch("LR8_Brightness", "ON");
            await LightSwitch("LR9_Brightness", "ON");
            await LightSwitch("LR10_Brightness", "ON");
            await LightSwitch("LRStripTvColorTemperature", "ON");
            await LightSwitch("LRStrip1ColorTemperature", "ON");
            await LightSwitch("LRStrip2ColorTemperature", "ON");
            await LightSwitch("LRStrip3ColorTemperature", "ON");
            await LightSwitch("LRStrip4ColorTemperature", "ON");
            await LightSwitch("LRStrip5ColorTemperature", "ON");
            await LightSwitch("LRStrip6ColorTemperature", "ON");
            await LightSwitch("KitchenSripColorTemperature", "ON");

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
            await LightSwitch("KR1Brightness", "ON");
            await LightSwitch("KR2Brightness", "ON");
            await LightSwitch("KR3Brightness", "ON");
            await LightSwitch("KR4Brightness", "ON");
            await LightSwitch("Table1Brightness", "ON");
            await LightSwitch("Table2Brightness", "ON");
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

        public async Task OpenWorkroomAC()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 25 00 01");
            await _listener.SendCommand(cmds);
            await Task.CompletedTask;
        }

        public async Task CloseWorkroomAC()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 25 00 00");
            await _listener.SendCommand(cmds);
            await Task.CompletedTask;
        }

        public async Task OpenLivingroomAC()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 26 00 01");
            await _listener.SendCommand(cmds);
            await Task.CompletedTask;
        }

        public async Task CloseLivingroomAC()
        {
            var cmds = new List<string>();
            cmds.Add("0D 06 10 26 00 00");
            await _listener.SendCommand(cmds);
            await Task.CompletedTask;
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

    public enum HVACSelected
    {
        WorkRoom,
        LivingRoom,
        Both,
        None
    }
}

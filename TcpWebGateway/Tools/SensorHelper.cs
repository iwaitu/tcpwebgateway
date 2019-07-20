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
    public class SensorHelper
    {
        private static readonly HttpClient client = new HttpClient();
        private SensorListener _listener;
        private LightHelper _lightHelper;

        public SensorHelper(LightHelper lightHelper)
        {
            _lightHelper = lightHelper;
            _lightHelper.SetSensorHelper(this);
        }

        public void SetListener(SensorListener listener)
        {
            _listener = listener;
        }

        public async Task OnReceiveCommand(string Command)
        {
            if(Command == "01 01 0D") //门道感应器
            {
                await OpenDoor();
            }
            else if(Command == "01 00 0D")
            {
                await CloseDoor();
            }
            else if (Command == "02 01 0D") //过道探测器报警
            {
                await OpenAisle();
            }
            else if (Command == "02 00 0D") 
            {
                await CloseAisle();
            }
            else if (Command == "03 01 0D") //烟雾探测器报警
            {

            }
            else if (Command == "03 00 0D") 
            {

            }
            else if (Command == "04 01 0D") //主灯打开成功
            {

            }
            else if (Command == "04 00 0D") //主灯关闭成功
            {

            }
        }

        public void OpenMainLight()
        {
            _listener.PublishCommand("04 01 0D");
        }

        public void CloseMainLight()
        {
            _listener.PublishCommand("04 00 0D");
        }

        /// <summary>
        /// 打开过道灯光
        /// </summary>
        /// <returns></returns>
        public async Task OpenAisle()
        {

            switch (_lightHelper.CurrentStateMode)
            {
                case StateMode.Home:
                    await LightSwitch("Aisle1Brightness", "80");
                    await LightSwitch("Aisle2Brightness", "80");
                    break;
                case StateMode.Out:
                    //开始报警
                    await LightSwitch("Aisle1Brightness", "40");
                    await LightSwitch("Aisle2Brightness", "40");
                    break;
                case StateMode.Read:
                    await LightSwitch("Aisle1Brightness", "40");
                    await LightSwitch("Aisle2Brightness", "40");
                    break;
            }
            
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
            switch (_lightHelper.CurrentStateMode)
            {
                case StateMode.Home:
                    await LightSwitch("Door1Brightness", "80");
                    await LightSwitch("Door2Brightness", "80");
                    break;
                case StateMode.Out:
                    //开始报警
                    await LightSwitch("Door1Brightness", "40");
                    await LightSwitch("Door2Brightness", "40");
                    break;
                case StateMode.Read:
                    await LightSwitch("Door1Brightness", "40");
                    await LightSwitch("Door2Brightness", "40");
                    break;
            }
            
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
}

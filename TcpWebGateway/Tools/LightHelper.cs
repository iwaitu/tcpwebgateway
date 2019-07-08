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
        public LightHelper(SwitchListener listener)
        {
            _listener = listener;
        }

        public void OnReceiveCommand(string Command)
        {
            
        }

        public async Task HomeMode()
        {
            await _listener.SendCommand("0B 06 10 22 00 00");
            await Task.Delay(100);
            await _listener.SendCommand("0B 06 10 23 00 00");
            await Task.Delay(100);
            await _listener.SendCommand("0B 06 10 21 00 01");
        }


        public async Task OutMode()
        {
            await _listener.SendCommand("0B 06 10 21 00 00");
            await Task.Delay(100);
            await _listener.SendCommand("0B 06 10 23 00 00");
            await Task.Delay(100);
            await _listener.SendCommand("0B 06 10 22 00 01");
        }


        public async Task ReadMode()
        {
            await _listener.SendCommand("0B 06 10 21 00 00");
            await Task.Delay(100);
            await _listener.SendCommand("0B 06 10 22 00 00");
            await Task.Delay(100);
            await _listener.SendCommand("0B 06 10 23 00 01");
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
        }

        public async Task OpenKitchen()
        {
            await Task.CompletedTask;
        }

        public async Task CloseKitchen()
        {
            await Task.CompletedTask;
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

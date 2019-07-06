using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using TcpWebGateway.Services;

namespace TcpWebGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LightController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly TcpHelper _tcpHelper;

        private IMqttClient _mqttClient;

        private static readonly HttpClient client = new HttpClient();

        public LightController(ILogger<HeatSystemController> logger, TcpHelper tcpHelper)
        {
            _logger = logger;
            _tcpHelper = tcpHelper;
        }

        /// <summary>
        /// 明亮模式
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("OpenAll")]
        public async Task OpenAllLight()
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

        [HttpPost]
        [Route("CloseAll")]
        public async Task CloseAllLight()
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


        #region ## 厨房 ##
        [HttpPost]
        [Route("OpenKitchen")]
        public async Task OpenKitchen()
        {
            await LightSwitch("", "ON");
        }


        [HttpPost]
        [Route("CloseKitchen")]
        public async Task CloseKitchen()
        {
            await LightSwitch("", "ON");
        }

        #endregion


        #region ## 客厅 ##

        [HttpPost]
        [Route("OpenLivingRoom")]
        public async Task OpenLivingRoom()
        {
            await LightSwitch("", "ON");
        }

        [HttpPost]
        [Route("CloseLivingRoom")]
        public async Task CloseLivingRoom()
        {
            await LightSwitch("", "ON");
        }
        #endregion


        #region ## 书房 ##

        [HttpPost]
        [Route("OpenWorkRoom")]
        public async Task OpenWorkRoom()
        {
            await LightSwitch("", "ON");
        }

        [HttpPost]
        [Route("CloseWorkRoom")]
        public async Task CloseWorkRoom()
        {
            await LightSwitch("", "ON");
        }

        #endregion

        #region ## 餐厅 ##

        [HttpPost]
        [Route("OpenDiningTable")]
        public async Task OpenDiningTable()
        {
            await LightSwitch("", "ON");
        }

        [HttpPost]
        [Route("CloseDiningTable")]
        public async Task CloseDiningTable()
        {
            await LightSwitch("", "ON");
        }

        #endregion

        #region ## 门廊 ##

        [HttpPost]
        [Route("OpenDoorLight")]
        public async Task OpenDoorLight()
        {
            await LightSwitch("", "ON");
        }

        [HttpPost]
        [Route("CloseDoorLight")]
        public async Task CloseDoorLight()
        {
            await LightSwitch("", "ON");
        }

        #endregion

        #region ## 过道 ##

        [HttpPost]
        [Route("OpenAisleLight")]
        public async Task OpenAisleLight()
        {
            await LightSwitch("", "ON");
        }

        [HttpPost]
        [Route("CloseAisleLight")]
        public async Task CloseAisleLight()
        {
            await LightSwitch("", "ON");
        }

        #endregion




        private async Task LightSwitch(string itemname,string command)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "HomeGaywate");

            var content = new StringContent(command, Encoding.UTF8, "text/plain");
            var result  = await client.PostAsync("http://192.168.50.245:38080/rest/items/" + itemname, content);

        }


    }
}
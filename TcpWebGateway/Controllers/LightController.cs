using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TcpWebGateway.Services;
using TcpWebGateway.Tools;

namespace TcpWebGateway.Controllers
{
    /// <summary>
    /// 灯光控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LightController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly SwitchListener _listener;
        private readonly SensorListener _sensorlistener;

        private LightHelper _helper;
        private SensorHelper _sensorHelper;

        public LightController(ILogger<HeatSystemController> logger,
            LightHelper lightHelper,
            SensorHelper sensorHelper,
            IHostedService hostedService, 
            IHostedService hostedService1)
        {
            _logger = logger;
            _listener = hostedService as SwitchListener;
            _sensorlistener = hostedService1 as SensorListener;
            _helper = lightHelper;
            _sensorHelper = sensorHelper;
        }

        [HttpPost]
        [Route("LightupSwitch")]
        public async Task LightupSwitch([FromBody] string command)
        {
            await _listener.SendCommand(command);
        }

        [HttpPost]
        [Route("HomeMode")]
        public async Task HomeMode()
        {
            await _helper.HomeMode();
        }

        [HttpPost]
        [Route("OutMode")]
        public async Task OutMode()
        {
            await _helper.OutMode();
        }

        [HttpPost]
        [Route("ReadMode")]
        public async Task ReadMode()
        {
            await _helper.ReadMode();
        }

        /// <summary>
        /// 明亮模式
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("OpenAll")]
        public async Task OpenAllLight()
        {
            await _helper.OpenAll();
        }

        [HttpPost]
        [Route("CloseAll")]
        public async Task CloseAllLight()
        {
            await _helper.CloseAll();
        }


        #region ## 厨房 ##
        [HttpPost]
        [Route("OpenKitchen")]
        public async Task OpenKitchen()
        {
            await Task.CompletedTask;
        }


        [HttpPost]
        [Route("CloseKitchen")]
        public async Task CloseKitchen()
        {
            await Task.CompletedTask;
        }

        #endregion


        #region ## 客厅 ##

        [HttpPost]
        [Route("OpenLivingRoom")]
        public async Task OpenLivingRoom()
        {
            await Task.CompletedTask;
        }

        [HttpPost]
        [Route("CloseLivingRoom")]
        public async Task CloseLivingRoom()
        {
            await Task.CompletedTask;
        }
        #endregion


        #region ## 书房 ##

        [HttpPost]
        [Route("OpenWorkRoom")]
        public async Task OpenWorkRoom()
        {
            await Task.CompletedTask;
        }

        [HttpPost]
        [Route("CloseWorkRoom")]
        public async Task CloseWorkRoom()
        {
            await Task.CompletedTask;
        }

        #endregion

        #region ## 餐厅 ##

        [HttpPost]
        [Route("OpenDiningTable")]
        public async Task OpenDiningTable()
        {
            await Task.CompletedTask;
        }

        [HttpPost]
        [Route("CloseDiningTable")]
        public async Task CloseDiningTable()
        {
            await Task.CompletedTask;
        }

        #endregion

        #region ## 门廊 ##

        [HttpPost]
        [Route("OpenDoorLight")]
        public async Task OpenDoorLight()
        {
            await _helper.OpenDoor();
        }

        [HttpPost]
        [Route("CloseDoorLight")]
        public async Task CloseDoorLight()
        {
            await _helper.CloseDoor();
        }

        #endregion

        #region ## 过道 ##

        [HttpPost]
        [Route("OpenAisleLight")]
        public async Task OpenAisleLight()
        {
            await _helper.OpenAisle();
        }

        [HttpPost]
        [Route("CloseAisleLight")]
        public async Task CloseAisleLight()
        {
            await _helper.CloseAisle();
        }

        #endregion

        #region ## 主灯 ##

        [HttpPost]
        [Route("OpenMainLight")]
        public async Task OpenMainLight()
        {
            _sensorHelper.OpenMainLight();
            await Task.CompletedTask;
        }

        [HttpPost]
        [Route("CloseMainLight")]
        public async Task CloseMainLight()
        {
            _sensorHelper.CloseMainLight();
            await Task.CompletedTask;
        }

        #endregion




    }
}
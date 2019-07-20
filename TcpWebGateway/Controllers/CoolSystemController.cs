using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TcpWebGateway.Tools;

namespace TcpWebGateway.Controllers
{
    /// <summary>
    /// VRV多联机空调控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CoolSystemController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly HvacHelper _helper;

        public CoolSystemController(ILogger<CoolSystemController> logger,HvacHelper helper)
        {
            _logger = logger;
            _helper = helper;

        }

        /// <summary>
        /// 打开空调
        /// </summary>
        /// <param name="id">客房0,主卧1,书房2,客厅3</param>
        /// <returns></returns>
        [HttpPost]
        [Route("TurnOnAC")]
        public async Task TurnOnAC(int id)
        {
            await _helper.TurnOnAC(id);
            await _helper.SyncAllState();
        }

        /// <summary>
        /// 关闭空调
        /// </summary>
        /// <param name="id">客房0,主卧1,书房2,客厅3</param>
        /// <returns></returns>
        [HttpPost]
        [Route("TurnOffAC")]
        public async Task TurnOffAC(int id)
        {
            await _helper.TurnOffAC(id);
            await _helper.SyncAllState();
        }

        /// <summary>
        /// 设置工作模式
        /// </summary>
        /// <param name="id">客房0,主卧1,书房2,客厅3</param>
        /// <param name="mode">Cool = 1,Heat = 8,Fan = 4,Dry = 2</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetWorkMode")]
        public async Task SetWorkMode(int id,int mode)
        {
            await _helper.SetMode(id, (WorkMode)mode);
            await _helper.SyncAllState();
        }


        /// <summary>
        /// 设置风速
        /// </summary>
        /// <param name="id">客房0,主卧1,书房2,客厅3</param>
        /// <param name="speed">Hight = 1,Middle = 2,Low = 4</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetFanspeed")]
        public async Task SetFanspeed(int id, int speed)
        {
            await _helper.SetFanspeed(id, (Fanspeed)speed);
            await _helper.SyncAllState();
        }

        /// <summary>
        /// 设置温度
        /// </summary>
        /// <param name="id">客房0,主卧1,书房2,客厅3</param>
        /// <param name="speed"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetTemperature")]
        public async Task SetTemperature(int id, int speed)
        {
            await _helper.SetTemperature(id, speed);
            await _helper.SyncAllState();
        }

        /// <summary>
        /// 获取房间温度
        /// </summary>
        /// <param name="id">客房0,主卧1,书房2,客厅3</param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTemperature")]
        public int GetTemperature(int id)
        {
            var obj = _helper.GetACStateObject(id);
            return obj.CurrentTemperature;
        }
    }
}
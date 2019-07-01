using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TcpWebGateway.Services;

namespace TcpWebGateway.Controllers
{
    /// <summary>
    /// 水地暖控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HeatSystemController : ControllerBase
    {
        private readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public HeatSystemController(ILogger<HeatSystemController> logger)
        {
            _logger = logger;
            logger.LogDebug("test");
        }


        /// <summary>
        /// 获取房间温度
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTemperature")]
        public float GetTemperature(int id)
        {
            try
            {
                var ret = TcpHelper.GetTemperature(id) / 10;
                return ret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return 0;
            }
            
        }

        /// <summary>
        /// 设置地暖温控器温度
        /// </summary>
        /// <param name="id"></param>
        /// <param name="temp">5.0-35.0 之间</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetTemperature")]
        public bool SetTemperature(int id,float temp)
        {
            try
            {
                return TcpHelper.SetTemperature(id, (Int16)(temp * 10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            
        }
    }
}
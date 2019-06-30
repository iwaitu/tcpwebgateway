using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TcpWebGateway.Controllers
{
    /// <summary>
    /// 水地暖控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HeatSystemController : ControllerBase
    {

        /// <summary>
        /// 获取房间温度
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTemperature")]
        public int GetTemperature(int id)
        {
            return 0;
        }
    }
}
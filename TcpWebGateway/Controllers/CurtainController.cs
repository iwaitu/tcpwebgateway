using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using TcpWebGateway.Services;

namespace TcpWebGateway.Controllers
{
    /// <summary>
    /// 窗帘控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CurtainController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly TcpHelper _tcpHelper;

        private IMqttClient _mqttClient;

        public CurtainController(ILogger<HeatSystemController> logger, TcpHelper tcpHelper)
        {
            _logger = logger;
            _tcpHelper = tcpHelper;

        }


        /// <summary>
        /// 读取窗帘状态
        /// 布帘id=3 , 纱帘id=2
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "Get")]
        public async Task<int> Get(int id)
        {
            if (id < 2 || id > 3)
            {
                return 0;
            }
            int retPercent = await _tcpHelper.GetCurtainStatus(id);

            
            return retPercent;

        }

        /// <summary>
        /// 设置窗帘闭合度
        /// </summary>
        /// <param name="id">布帘id=3 , 纱帘id=2</param>
        /// <param name="value">百分比</param>
        [HttpPost]
        public async Task SetPosition(int id, int value)
        {
            await _tcpHelper.SetCurtainStatus(id, value);
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="id"></param>
        [HttpPost]
        [Route("Stop")]
        public async Task Stop(int id)
        {
            await _tcpHelper.StopCurtain(id);
        }

        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="id"></param>
        [HttpPost]
        [Route("Open")]
        public async Task Open(int id)
        {
            await _tcpHelper.OpenCurtain(id);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="id"></param>
        [HttpPost]
        [Route("Close")]
        public async Task Close(int id)
        {
            await _tcpHelper.CloseCurtain(id);
        }

    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using TcpWebGateway.Tools;
using static TcpWebGateway.Tools.CurtainHelper;

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
        private readonly IMemoryCache _cache;
        private readonly CurtainHelper _curtainHelper;

        private IMqttClient _mqttClient;

        public CurtainController(ILogger<CurtainController> logger,IMemoryCache cache, CurtainHelper curtainHelper)
        {
            _logger = logger;
            _cache = cache;
            _curtainHelper = curtainHelper;
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
            await _curtainHelper.GetCurtainStatus(id);
            await Task.Delay(500).ConfigureAwait(false);
            var obj = new CurtainStateObject();
            while(_cache.TryGetValue(id,out obj)==false)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
            //return retPercent;
            return obj.Status;

        }

        /// <summary>
        /// 设置窗帘闭合度
        /// </summary>
        /// <param name="id">布帘id=3 , 纱帘id=2</param>
        /// <param name="value">百分比</param>
        [HttpPost]
        public async Task SetPosition(int id, int value)
        {
            await _curtainHelper.SetCurtain(id, value);
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="id"></param>
        [HttpPost]
        [Route("Stop")]
        public async Task Stop(int id)
        {
            await _curtainHelper.Stop(id);
        }

        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="id"></param>
        [HttpPost]
        [Route("Open")]
        public async Task Open(int id)
        {
            await _curtainHelper.Open(id);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="id"></param>
        [HttpPost]
        [Route("Close")]
        public async Task Close(int id)
        {
            await _curtainHelper.Close(id);
        }

    }
}

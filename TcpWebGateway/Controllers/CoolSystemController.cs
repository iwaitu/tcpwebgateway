using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TcpWebGateway.Tools;

namespace TcpWebGateway.Controllers
{
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

        [HttpPost]
        [Route("TurnOnAC")]
        public async Task TurnOnAC(int id)
        {
            await _helper.TurnOnAC(id);
            await _helper.SyncAllState();
        }

        [HttpPost]
        [Route("TurnOffAC")]
        public async Task TurnOffAC(int id)
        {
            await _helper.TurnOffAC(id);
            await _helper.SyncAllState();
        }
    }
}
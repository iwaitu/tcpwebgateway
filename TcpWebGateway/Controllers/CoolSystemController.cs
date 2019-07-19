using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TcpWebGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoolSystemController : ControllerBase
    {
        private readonly ILogger _logger;

        public CoolSystemController(ILogger<CoolSystemController> logger)
        {
            _logger = logger;
        }
    }
}
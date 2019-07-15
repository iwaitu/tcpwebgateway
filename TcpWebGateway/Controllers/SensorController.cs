using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TcpWebGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorController : ControllerBase
    {
        private readonly ILogger _logger;
        public SensorController(ILogger<SensorController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("StateChange")]
        public async Task StateChange(string id)
        {
            _logger.LogInformation(id + " StateChanged");
        }
    }
}
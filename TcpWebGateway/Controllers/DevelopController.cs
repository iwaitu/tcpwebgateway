using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TcpWebGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevelopController : ControllerBase
    {
        [HttpGet]
        [Route("LoadRemote")]
        public async Task<string> LoadRemote()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            WebClient client = new WebClient();
            var str = client.DownloadString("http://cnblogs.com");
            sw.Stop();
            return sw.ElapsedMilliseconds.ToString();
        }
    }
}
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TcpWebGateway.Services
{
    public class HvacListener : BackgroundService
    {
        private const int port = 8008;  //空调

        private readonly ILogger _logger;
        public CancellationToken token;
        private IPEndPoint remoteEP;
        private IPAddress ipAddress;

        public HvacListener(ILogger<HvacListener> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}

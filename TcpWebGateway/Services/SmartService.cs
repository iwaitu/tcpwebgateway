﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TcpWebGateway.Tools;

namespace TcpWebGateway.Services
{
    public class SmartService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly LightHelper _lightHelper;
        private readonly HvacHelper _hvacHelper;


        private Timer _timer;

        public SmartService(ILogger<SmartService> logger,LightHelper lightHelper,HvacHelper hvacHelper)
        {
            _logger = logger;
            _lightHelper = lightHelper;
            _hvacHelper = hvacHelper;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Delay(3000);
            //_tcpHelper.InitData();
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            //_logger.LogInformation("sync ac stateobject.");
            Task.Run(async () => { await _hvacHelper.SyncAllState(); });
            //Task.Run(async () => { await _tcpHelper.PublishStatus(); });
            if(_lightHelper.CurrentStateMode == StateMode.Home)
            {

            }
            
        }
    }
}
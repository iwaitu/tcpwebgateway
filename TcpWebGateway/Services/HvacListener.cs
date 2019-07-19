﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpWebGateway.Tools;

namespace TcpWebGateway.Services
{
    public class HvacListener : BackgroundService
    {

        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public CancellationToken token;
        private IPEndPoint remoteEP;
        private IPAddress ipAddress;

        private HvacHelper _helper;

        public Socket _client;

        public HvacListener(ILogger<HvacListener> logger, IConfiguration configuration, HvacHelper helper)
        {
            _logger = logger;
            _config = configuration;

            var hostip = _config.GetValue<string>("ipGateway:Gateway");
            var port = _config.GetValue<int>("ipGateway:portAC");
            ipAddress = IPAddress.Parse(hostip);
            remoteEP = new IPEndPoint(ipAddress, port);

            _helper = helper;
            _helper.SetListener(this);
        }

        public async Task StartClient(CancellationToken cancellationToken)
        {

            _client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            _logger.LogInformation("Connecting to {0}:{1}", ipAddress.ToString(),remoteEP.Port);
            var isConnect = await ConnectAsync(_client, remoteEP);
            if (!isConnect)
            {
                _logger.LogError("Can not connect.");
                return;
            }

            //await _helper.SyncAllState();

            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await ReceiveAsync(_client,1);
                if (!string.IsNullOrWhiteSpace(response) && !string.IsNullOrEmpty(response))
                {
                    _logger.LogInformation("Receive:" + response);
                    await _helper.OnReceiveData(response);
                }
                await Task.Delay(50, cancellationToken);
            }

            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }

        private Task<bool> ConnectAsync(Socket client, IPEndPoint remoteEndPoint)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (remoteEndPoint == null) throw new ArgumentNullException(nameof(remoteEndPoint));

            return Task.Run(() => Connect(client, remoteEndPoint));
        }

        private bool Connect(Socket client, EndPoint remoteEndPoint)
        {
            if (client == null || remoteEndPoint == null)
                return false;

            try
            {
                client.Connect(remoteEndPoint);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<string> ReceiveAsync(Socket client, int waitForFirstDelaySeconds = 3)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            for (var i = 0; i < waitForFirstDelaySeconds; i++)
            {
                if (client.Available > 0)
                    break;
                await Task.Delay(100).ConfigureAwait(false);
            }

            if (client.Available < 1)
                return null;

            const int bufferSize = 1024;
            var buffer = new byte[bufferSize];

            // Get data
            var response = new StringBuilder(bufferSize);
            do
            {
                var size = Math.Min(bufferSize, client.Available);
                await Task.Run(() => client.Receive(buffer)).ConfigureAwait(false);
                response.Append(BitConverter.ToString(buffer, 0, size - 1)).Replace("-", " ");

            } while (client.Available > 0);

            return response.ToString();
        }

        

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public async Task<int> SendCommand(string data)
        {
            var cmd = StringToByteArray(data.Replace(" ", ""));
            var cmdCRC = CRCHelper.Checksum(cmd);
            var cmd1 = new byte[cmd.Length + 1];
            cmd.CopyTo(cmd1, 0);
            cmd1[cmd.Length] = (byte)cmdCRC;
            var str = BitConverter.ToString(cmd1, 0, cmd1.Length).Replace("-", " ");
            _logger.LogInformation("SendCmd : " + str);

            if (!_client.Connected)
            {
                _logger.LogError("Can not connect.");
                return 0;
            }
            var ret = await SendAsync(_client, cmd1, 0, cmd1.Length, 0).ConfigureAwait(false);

            return ret;
        }

        public async Task SendCommand(List<string> data)
        {

            if (!_client.Connected)
            {
                _logger.LogError("Can not connect.");
                return;
            }
            foreach (var singlecmd in data)
            {
                var cmd = StringToByteArray(singlecmd.Replace(" ", ""));
                var cmdCRC = CRCHelper.Checksum(cmd);
                var cmd1 = new byte[cmd.Length + 1];
                cmd.CopyTo(cmd1, 0);
                cmd1[cmd.Length] = (byte)cmdCRC;
                var str = BitConverter.ToString(cmd1, 0, cmd1.Length).Replace("-", " ");
                _logger.LogInformation("SendCmd : " + str);
                var ret = await SendAsync(_client, cmd1, 0, cmd1.Length, 0).ConfigureAwait(false);
                await Task.Delay(500).ConfigureAwait(false);
            }

        }

        private Task<int> SendAsync(Socket client, byte[] buffer, int offset,
            int size, SocketFlags socketFlags)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            return Task.Run(() => client.Send(buffer, offset, size, socketFlags));
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return StartClient(stoppingToken);
        }
    }
}
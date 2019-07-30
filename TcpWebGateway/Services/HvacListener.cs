using Microsoft.Extensions.Configuration;
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
            _logger.LogInformation("Connect to {0}:{1}", remoteEP.Address.ToString(), remoteEP.Port);
            //await _helper.SyncAllState();            
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
                response.Append(BitConverter.ToString(buffer, 0, size)).Replace("-", " ");

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

        public async Task SendCommand(string data,bool debug = false)
        {
            try
            {
                var cmd = StringToByteArray(data.Replace(" ", ""));
                var cmdCRC = CRCHelper.Checksum(cmd);
                var cmd1 = new byte[cmd.Length + 1];
                cmd.CopyTo(cmd1, 0);
                cmd1[cmd.Length] = (byte)cmdCRC;
                var str = BitConverter.ToString(cmd1, 0, cmd1.Length).Replace("-", " ");
                if (debug)
                {
                    _logger.LogInformation("SendCmd : " + str);
                }
                using (var s = SafeSocket.ConnectSocket(remoteEP))
                {
                    var ret = await SendAsync(s, cmd1, 0, cmd1.Length, 0).ConfigureAwait(false);

                    var response = await ReceiveAsync(s);
                    if (debug)
                    {
                        _logger.LogInformation("Receive : " + response);
                    }

                    if (!string.IsNullOrWhiteSpace(response) && !string.IsNullOrEmpty(response))
                    {
                        await _helper.OnReceiveData(response);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("err data: " + data);
                _logger.LogError(ex.ToString());
            }
            
        }

        public async Task SendCommand(List<string> data)
        {
            foreach (var cmd in data)
            {
                await SendCommand(cmd);
                await Task.Delay(100).ConfigureAwait(false);
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

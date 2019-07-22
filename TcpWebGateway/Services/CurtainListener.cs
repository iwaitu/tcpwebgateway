using Microsoft.Extensions.Caching.Memory;
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


    public class CurtainListener : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        private readonly CurtainHelper _curtainHelper;

        public CancellationToken token;
        private IPEndPoint remoteEP;
        private IPAddress ipAddress;

        private Socket _client;

        public CurtainListener(ILogger<CurtainListener> logger, IConfiguration configuration,CurtainHelper curtainHelper)
        {
            _config = configuration;
            _logger = logger;
            _curtainHelper = curtainHelper;

            _curtainHelper.SetListener(this);

            var hostip = _config.GetValue<string>("ipGateway:Gateway");
            var port = _config.GetValue<int>("ipGateway:portCurtain");
            ipAddress = IPAddress.Parse(hostip);
            remoteEP = new IPEndPoint(ipAddress, port);
        }


        private async Task StartClient(CancellationToken stoppingToken)
        {
            _client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            _logger.LogInformation("Connecting to {0}:{1}", ipAddress.ToString(), remoteEP.Port);
            var isConnect = await ConnectAsync(_client, remoteEP);
            if (!isConnect)
            {
                _logger.LogError("Can not connect.");
                return;
            }
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var response = await ReceiveAsync(_client, 1);
                    if (!string.IsNullOrWhiteSpace(response) && !string.IsNullOrEmpty(response))
                    {
                        //_logger.LogInformation("Receive:" + response);
                        await _curtainHelper.OnReceiveData(response);
                    }
                    await Task.Delay(50, stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                _client.Shutdown(SocketShutdown.Both);
                _client.Close();
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

        public async Task<int> SendCommand(string data)
        {
            var cmd = CRCHelper.StringToByteArray(data.Replace(" ", ""));

            var str = BitConverter.ToString(cmd, 0, cmd.Length).Replace("-", " ");
            _logger.LogInformation("SendCmd : " + str);

            if (!_client.Connected)
            {
                _logger.LogError("Can not connect.");
                return 0;
            }
            var ret = await SendAsync(_client, cmd, 0, cmd.Length, 0).ConfigureAwait(false);

            return ret;
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

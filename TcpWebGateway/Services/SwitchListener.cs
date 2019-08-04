using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
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

    /// <summary>
    /// 主要负责和智能面板通讯并监听按键事件
    /// 
    /// </summary>
    public class SwitchListener : BackgroundService
    {    
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public CancellationToken token;
        private IPEndPoint remoteEP;
        private IPAddress ipAddress;
        private LightHelper _helper;

        public SwitchListener(IConfiguration configuration, LightHelper helper)
        {
            _logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            _config = configuration;

            var hostip = _config.GetValue<string>("ipGateway:Gateway");
            var port = _config.GetValue<int>("ipGateway:portSwitch");
            ipAddress = IPAddress.Parse(hostip);
            remoteEP = new IPEndPoint(ipAddress, port);

            _helper = helper;
            _helper.SetListener(this);

        }

        public async Task StartClient(CancellationToken cancellationToken)
        {

            Socket client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            _logger.Info("Connecting to {0}:{1}", ipAddress.ToString(), remoteEP.Port);
            var isConnect = await ConnectAsync(client, remoteEP);
            int i = 0;
            if (!isConnect)
            {
                _logger.Error("Can not connect.");
                return;
            }
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var response = await ReceiveAsync(client);
                    if (!string.IsNullOrWhiteSpace(response) && !string.IsNullOrEmpty(response))
                    {
                        _logger.Info("Receive:" + response);
                        await _helper.OnReceiveCommand(response);
                    }
                    //每5秒发送一次心跳指令
                    i=i+1;
                    if(i%100 == 0)
                    {
                        client.Send(new byte[] { 0x01 });
                        i = 0;
                    }
                    await Task.Delay(50, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
            finally
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }

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

        private async Task<int> SendAsync(Socket client, string data)
        {
            var byteData = Encoding.ASCII.GetBytes(data);
            return await SendAsync(client, byteData, 0, byteData.Length, 0).ConfigureAwait(false);
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public async Task<string> SendCommand(string data,bool log = true)
        {
            try
            {
                var cmd = StringToByteArray(data.Replace(" ", ""));
                var cmdCRC = CRCHelper.get_CRC16_C(cmd);
                var cmd1 = new byte[cmd.Length + 2];
                cmd.CopyTo(cmd1, 0);
                cmdCRC.CopyTo(cmd1, cmd.Length);
                var str = CRCHelper.byteToHexStr(cmd1, cmd1.Length);
                if (log)
                {
                    _logger.Info("SendCmd : " + str);
                }
                using (var socket = SafeSocket.ConnectSocket(remoteEP))
                {
                    var ret = await SendAsync(socket, cmd1, 0, cmd1.Length, 0).ConfigureAwait(false);
                    var response = await ReceiveAsync(socket);
                    if (log)
                    {
                        _logger.Info("Cmd Receive : " + response);
                    }
                    if (_helper != null)
                    {
                        await _helper.OnReceiveCommand(response);
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.Info("err data:" + data);
                _logger.Error(ex.ToString());
                return string.Empty;
            }
            
            
        }

        public async Task SendCommand(List<string> data,bool debug = true)
        {
            foreach (var singlecmd in data)
            {
                await SendCommand(singlecmd,debug);
                await Task.Delay(200);
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

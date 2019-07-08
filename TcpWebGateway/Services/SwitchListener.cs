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

namespace TcpWebGateway.Services
{
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }
    public class SwitchListener : IHostedService,IDisposable
    {
        private const int port = 8002;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.  
        private static String response = String.Empty;

        private readonly ILogger _logger;
        private Socket _client;
        private bool bRunning = false;

        public SwitchListener()
        {
            _logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            
        }

        public async Task StartClient()
        {
            _logger.Info("Connecting to 192.168.50.17:8002");
            IPAddress ipAddress = IPAddress.Parse("192.168.50.17");
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            _client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            var isConnect = await ConnectAsync(_client,remoteEP).ConfigureAwait(false);
            if (!isConnect)
            {
                _logger.Error("Can not connect.");
                return;
            }

            _logger.Info("Start Listening to 192.168.50.17:8002");
            bRunning = true;

            while (bRunning)
            {
                var response = await ReceiveAsync(_client).ConfigureAwait(false);
                if(!string.IsNullOrWhiteSpace(response) && !string.IsNullOrEmpty(response))
                {
                    _logger.Info(response);
                }
                await Task.Delay(1000);
            }
            

        }

        private Task<bool> ConnectAsync(Socket client, IPEndPoint remoteEndPoint)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (remoteEndPoint == null) throw new ArgumentNullException(nameof(remoteEndPoint));

            return Task.Run(() => Connect(client, remoteEndPoint));
        }

        private bool Connect( Socket client, EndPoint remoteEndPoint)
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
                await Task.Delay(1000).ConfigureAwait(false);
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
                response.Append(BitConverter.ToString(buffer, 0,size-1)).Replace("-"," ");

            } while (client.Available > 0);

            return response.ToString();
        }

        private async Task<int> SendAsync(Socket client, string data)
        {
            var byteData = Encoding.ASCII.GetBytes(data);
            return await SendAsync(client, byteData, 0, byteData.Length, 0).ConfigureAwait(false);
        }

        private Task<int> SendAsync(Socket client, byte[] buffer, int offset,
            int size, SocketFlags socketFlags)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            return Task.Run(() => client.Send(buffer, offset, size, socketFlags));
        }

        public void Dispose()
        {
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
            _client.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await StartClient();
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            bRunning = false;
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }
    }
}

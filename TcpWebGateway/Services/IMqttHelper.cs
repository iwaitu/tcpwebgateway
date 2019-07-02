using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MQTTnet;

namespace TcpWebGateway.Services
{
    public interface IMqttHelper :  IDisposable
    {
        Task Publish(MqttApplicationMessage message);

        void Subscribe(string topic);
    }
}
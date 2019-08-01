using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TcpWebGateway.Services
{
    public class ModbusService
    {
        private readonly ILogger _logger;

        //private ModbusClient _modbusClient;

        private string serverIP { get; set; }
        private int serverPORT { get; set; }


        public ModbusService(ILogger<ModbusService> logger,IConfiguration configuration)
        {
            _logger = logger;
            serverIP = configuration.GetValue<string>("ipGateway:Gateway");
            serverPORT = configuration.GetValue<int>("ipGateway:portHeat");
        }

        public void Connect()
        {
            //_modbusClient = new ModbusClient(serverIP, serverPORT);
        }

        public Task<int> GetTemperature(int id)
        {
            return Task.FromResult(100);
        }

        public Task<bool> SetTemperature(int id,float temp)
        {
            return Task.FromResult(false);
        }


    }


}

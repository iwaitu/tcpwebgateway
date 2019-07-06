using Microsoft.Extensions.Logging;

namespace TcpWebGateway.Services
{
    public class ModbusService
    {
        private readonly ILogger _logger;

        //private ModbusClient _modbusClient;

        private string serverIP { get; set; }
        private int serverPORT { get; set; }


        public ModbusService(ILogger<ModbusService> logger)
        {
            _logger = logger;
        }

        public void Connect()
        {
            //_modbusClient = new ModbusClient(serverIP, serverPORT);
        }


    }


}

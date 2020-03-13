using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TcpWebGateway.models
{
    public class LogItem
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
    }
}

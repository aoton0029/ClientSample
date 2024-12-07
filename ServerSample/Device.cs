using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSample
{
    public class Device
    {
        public string Name { get; set; } = "";
        public List<ResponseCommand> Commands { get; set; } = new List<ResponseCommand>();

    }
}

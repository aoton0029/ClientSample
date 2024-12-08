using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSample
{
    public class ResponseCommand
    {
        public string Command { get; set; } = "";
        public string Value { get; set; } = "";

        public Device Device
        {
            get => default;
            set
            {
            }
        }

        public ResponseCommand()
        {

        }
    }
}

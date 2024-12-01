using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Commands
{
    public class CommandText : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("Setting Result to Fixed String...");
        }
    }
}

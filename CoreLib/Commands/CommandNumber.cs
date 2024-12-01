using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Commands
{
    // 各コマンドの具体的な実装
    public class CommandNumber : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("Generating Random Number...");
        }
    }

}

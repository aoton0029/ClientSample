using CoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Commands
{
    // 通常のコマンド
    public class RegularCommand : ICommand
    {
        private readonly string _data;

        public RegularCommand(string data)
        {
            _data = data;
        }

        public string Execute(Device device)
        {
            device.(_data);
            return null; // 結果を返さない
        }
    }
}

using CoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Commands
{
    // コマンドインターフェース
    public interface ICommand
    {
        string Execute(Device device);
    }
}

using ClientSample.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSample.Commands
{
    // コマンドインターフェース
    public interface ICommand
    {
        Task<string> ExecuteAsync(IDevice device);
    }
}

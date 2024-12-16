using ClientSample.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSample.Commands
{
    // 通常のコマンド
    public class RegularCommand : ICommand
    {
        private readonly string _message;

        public RegularCommand(string data)
        {
            _message = data;
        }

        public async Task<string> ExecuteAsync(IDevice device)
        {
            await device.SendCmdAsync(_message);
            return null; // 結果を返さない
        }
    }
}

using CoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Commands
{
    internal class ResultCommand : ICommand
    {
        private readonly string _message;

        public ResultCommand(string data)
        {
            _message = data;
        }

        public async Task<string> ExecuteAsync(IDevice device)
        {
            return await device.QueryAsync(_message);
        }
    }
}

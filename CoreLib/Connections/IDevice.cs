using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Models
{
    public interface IDevice
    {
        IConnection Connection { get; }

        Task SendCmdAsync(string command);

        Task<string> QueryAsync(string command, CancellationToken cancellationToken = default);


    }
}

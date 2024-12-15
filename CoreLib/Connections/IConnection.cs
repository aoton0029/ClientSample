using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Models
{
    public interface IConnection : IDisposable
    {
        int Timeout { get; set; }

        Task Open(CancellationToken cancellationToken = default);

        void Close();

        bool IsOpen { get; }

        Task Write(byte[] data, CancellationToken cancellationToken = default);

        Task<ReadResult> Read(byte[] buffer, int readLength = -1, int specialTimeout = 0, CancellationToken cancellationToken = default);

        Task ClearBuffers(CancellationToken cancellationToken = default);
    }

}

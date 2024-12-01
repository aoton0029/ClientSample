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

        string DevicePath { get; }

        Task Open(CancellationToken cancellationToken = default);

        void Close();

        bool IsOpen { get; }

        Task Write(byte[] data, CancellationToken cancellationToken = default);

        Task<ReadResult> Read(byte[] buffer, int readLength = -1, int specialTimeout = 0, CancellationToken cancellationToken = default);

        Task ClearBuffers(CancellationToken cancellationToken = default);
    }

	public static class ScpiConnectionExtensions
    {
        public static async Task WriteString(this IConnection conn, string data, bool addNewLine = true, CancellationToken cancellationToken = default)
        {
            string msg = data;

            if (addNewLine && msg.Last() != '\n')
            {
                msg += '\n';
            }

            byte[] binaryData = Encoding.ASCII.GetBytes(msg);
            await conn.Write(binaryData, cancellationToken);
        }

        public static async Task<string> ReadString(this IConnection conn, int specialTimeout = 0, CancellationToken cancellationToken = default)
        {
            ReadResult chunk;
            StringBuilder response = new();

            // Some devices (such as the Keysight multimeters) do not like when we require reading longer than some specific constraint.
            // Therefore we will use only 128-bytes long buffer for generic string reads.
            byte[] buffer = new byte[128];

            do
            {
                chunk = await conn.Read(buffer, buffer.Length, specialTimeout, cancellationToken);
                response.Append(Encoding.ASCII.GetString(chunk.Data, 0, chunk.Length));
            } while (!chunk.Eof && chunk.Length > 0);

            return response.ToString().TrimEnd('\r', '\n');
        }

        public static async Task<byte[]> ReadBytes(this IConnection conn, int specialTimeout = 0, CancellationToken cancellationToken = default)
        {
            var result = new MemoryStream();
            var buffer = new byte[1024];

            ReadResult chunk;
            do
            {
                chunk = await conn.Read(buffer, -1, specialTimeout, cancellationToken);
                result.Write(buffer, 0, chunk.Length);
            } while (!chunk.Eof && chunk.Length > 0);

            return result.ToArray();
        }

        public static async Task<string> GetId(this IConnection conn, CancellationToken cancellationToken = default)
        {
            await conn.WriteString("*IDN?", true, cancellationToken);
            return await conn.ReadString(0, cancellationToken);
        }
    }

    public readonly struct ReadResult
    {
        public int Length { get; }
        public bool Eof { get; }
        public byte[] Data { get; }
        public ReadResult(int length, bool eof, byte[] data)
        {
            Length = length;
            Eof = eof;
            Data = data;
        }
    }
}

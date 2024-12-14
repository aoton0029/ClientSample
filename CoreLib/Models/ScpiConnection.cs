using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Models
{
    public static class ScpiConnection
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
}

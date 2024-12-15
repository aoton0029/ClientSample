using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Models
{
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

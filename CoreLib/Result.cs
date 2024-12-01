using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    public class Result<T>
    {
        public string NextTemplateId { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public T Value { get; set; } // ジェネリック型の値を保持

        public Result(string nextTemplateId, string message, bool isSuccess, T value)
        {
            NextTemplateId = nextTemplateId;
            Message = message;
            IsSuccess = isSuccess;
            Value = value;
        }
    }
}

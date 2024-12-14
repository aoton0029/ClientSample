using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    // Result構造体
    public struct Result
    {
        public string NextTemplate { get; set; }
        public string Message { get; set; }
        public int NumericResult { get; set; }
        public string TextResult { get; set; }

        public Result(string nextTemplate, string message, int numericResult = 0, string textResult = null)
        {
            NextTemplate = nextTemplate;
            Message = message;
            NumericResult = numericResult;
            TextResult = textResult;
        }
    }
}

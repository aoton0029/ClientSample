using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSample.Templates
{
    // Result構造体
    public struct TemplateResult
    {
        public string NextTemplate { get; }
        public string Message { get; }
        public bool IsSuccess { get; }

        public TemplateResult(string nextTemplate, string message, bool isSuccess)
        {
            NextTemplate = nextTemplate;
            Message = message;
            IsSuccess = isSuccess;
        }
    }
}

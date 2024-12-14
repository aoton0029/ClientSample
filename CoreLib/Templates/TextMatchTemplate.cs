using CoreLib.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreLib.Templates
{
    // パターンマッチテンプレート
    public class TextMatchTemplate : Template
    {
        protected override async Task InitializeAsync(params object[] paramters)
        {
            Console.WriteLine("Initializing CaseTemplate...");
            await Task.CompletedTask;
        }

        protected override void FinalizeResult(string commandResult)
        {
            Console.WriteLine($"Received String: {commandResult}");

            switch (commandResult)
            {
                case "SUCCESS":
                    Result = new TemplateResult("EndTemplate", "Case: SUCCESS.", true);
                    break;
                case "FAILURE":
                    Result = new TemplateResult("EndTemplate", "Case: FAILURE.", false);
                    break;
                default:
                    Result = new TemplateResult(null, "Case: UNKNOWN.", false);
                    break;
            }
        }
    }
}

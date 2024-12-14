using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Templates
{
    // 終了テンプレート
    public class EndTemplate : Template
    {
        protected override async Task InitializeAsync()
        {
            Console.WriteLine("Initializing EndTemplate...");
            await Task.CompletedTask;
        }

        protected override void FinalizeResult(string commandResult)
        {
            Console.WriteLine("Finalizing EndTemplate...");
            Result = new TemplateResult(null, "Process ended.", true);
        }
    }
}

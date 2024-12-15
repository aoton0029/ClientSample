using CoreLib.Commands;
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
        public EndTemplate(string id) : base(id) { }

        public override void Initialize(List<ICommand> commands, Dictionary<string, object> parameters)
        {
            Console.WriteLine($"Initializing EndTemplate: {ID}");
            Commands = commands;
        }

        protected override void FinalizeResult(string commandResult)
        {
            Console.WriteLine("Finalizing EndTemplate...");
            Result = new TemplateResult(null, "Process ended.", true);
        }
    }
}

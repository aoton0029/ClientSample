using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Templates
{
    // 終了テンプレート
    public class EndTemplate : Template<object>
    {
        public EndTemplate(string id, string name)
        {
            Id = id;
            Name = name;
        }

        protected override void Initialize()
        {
            Console.WriteLine($"{Name} (ID: {Id}) Initialization");
        }

        protected override void FinalizeResult()
        {
            Console.WriteLine($"{Name} (ID: {Id}) Finalization");
            Result = new Result<object>(null, "Process ended.", true, null);
        }
    }
}

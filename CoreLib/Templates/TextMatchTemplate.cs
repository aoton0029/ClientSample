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
    public class TextMatchTemplate : Template<string>
    {
        public TextMatchTemplate(string id, string name, dynamic nextTemplateInfo)
        {
            Id = id;
            Name = name;
            NextTemplateInfo = nextTemplateInfo;
        }

        protected override void Initialize()
        {
            Console.WriteLine($"{Name} (ID: {Id}) Initialization");
            Commands.Add(new CommandText());
        }

        protected override void FinalizeResult()
        {
            Console.WriteLine($"{Name} (ID: {Id}) Finalization");
            string inputString = "MatchThis"; // 固定文字列として仮定
            Console.WriteLine($"Input String: {inputString}");

            string nextTemplateId = null;
            foreach (var condition in NextTemplateInfo)
            {
                string pattern = condition.pattern;
                if (Regex.IsMatch(inputString, pattern))
                {
                    nextTemplateId = condition.next;
                    break;
                }
            }

            Result = new Result<string>(nextTemplateId, $"Pattern match evaluated for input: {inputString}", true, inputString);
        }
    }
}

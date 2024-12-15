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
    public class CaseTemplate : Template
    {
        private Dictionary<string, string> _caseMapping;

        public CaseTemplate(string id) : base(id) { }

        public override void Initialize(List<ICommand> commands, Dictionary<string, object> parameters)
        {
            Console.WriteLine($"Initializing CaseTemplate: {ID}");

            // パラメータからケースマッピングを取得
            _caseMapping = (Dictionary<string, string>)parameters["cases"];
            Commands = commands;
        }

        protected override void FinalizeResult(string commandResult)
        {
            Console.WriteLine($"Received String: {commandResult}");

            if (_caseMapping != null && _caseMapping.TryGetValue(commandResult, out string nextTemplateID))
            {
                Result = new TemplateResult(nextTemplateID, $"Case: {commandResult}.", true);
            }
            else
            {
                Result = new TemplateResult(null, "Case: UNKNOWN.", false);
            }
        }
    }
}

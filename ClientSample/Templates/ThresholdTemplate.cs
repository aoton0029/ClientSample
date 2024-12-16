using ClientSample.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSample.Templates
{
    public class ThresholdTemplate : Template
    {
        private decimal _threshold;
        private string _aboveThresholdID;
        private string _belowThresholdID;

        public ThresholdTemplate(string id) : base(id) { }

        public override void Initialize(List<ICommand> commands, Dictionary<string, object> parameters)
        {
            Console.WriteLine($"Initializing ThresholdTemplate: {ID}");

            // パラメータから閾値と遷移先IDを取得
            if (parameters != null)
            {
                _threshold = (decimal)parameters["threshold"];
                _aboveThresholdID = (string)parameters["aboveThresholdID"];
                _belowThresholdID = (string)parameters["belowThresholdID"];
            }

            Commands = commands;
        }

        protected override void FinalizeResult(string commandResult)
        {
            if (decimal.TryParse(commandResult, out decimal value))
            {
                Console.WriteLine($"Received Value: {value}");
                Result = value > _threshold
                    ? new TemplateResult(_aboveThresholdID, $"Value exceeds {_threshold}.", true)
                    : new TemplateResult(_belowThresholdID, $"Value is below {_threshold}.", true);
            }
            else
            {
                Result = new TemplateResult(null, "Failed to parse value.", false);
            }
        }
    }
}

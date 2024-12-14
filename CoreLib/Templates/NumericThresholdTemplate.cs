using CoreLib.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Templates
{
    public class ThresholdTemplate : Template
    {
        private decimal _threshold;

        public ThresholdTemplate()
        {
            
        }

        protected override async Task InitializeAsync(params object[] parameters)
        {
            Console.WriteLine("Initializing ThresholdTemplate...");
            _threshold = (decimal)parameters[0];
            await Task.CompletedTask;
        }

        protected override void FinalizeResult(string commandResult)
        {
            if (decimal.TryParse(commandResult, out decimal value))
            {
                Console.WriteLine($"Received Value: {value}");
                Result = value > _threshold
                    ? new TemplateResult("CaseTemplate", "Value exceeds threshold.", true)
                    : new TemplateResult("EndTemplate", "Value is below threshold.", true);
            }
            else
            {
                Result = new TemplateResult(null, "Failed to parse value.", false);
            }
        }
    }

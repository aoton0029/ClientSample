using CoreLib.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Templates
{
    public class NumericThresholdTemplate : Template<int>
    {
        public NumericThresholdTemplate(string id, string name, dynamic nextTemplateInfo)
        {
            Id = id;
            Name = name;
            NextTemplateInfo = nextTemplateInfo;
        }

        protected override void Initialize()
        {
            Console.WriteLine($"{Name} (ID: {Id}) Initialization");
            Commands.Add(new CommandNumber());
        }

        protected override void FinalizeResult()
        {
            Console.WriteLine($"{Name} (ID: {Id}) Finalization");
            Random random = new Random();
            int randomValue = random.Next(1, 101);
            Console.WriteLine($"Generated Number: {randomValue}");

            string nextTemplateId = randomValue > NextTemplateInfo.threshold
                ? NextTemplateInfo.aboveThreshold
                : NextTemplateInfo.belowThreshold;

            Result = new Result<int>(nextTemplateId, "Number evaluated.", true, randomValue);
        }
    }
}

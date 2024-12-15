using CoreLib.Dbs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    internal class main
    {
        public void Main()
        {
            using (var context = new WorkflowDbContext())
            {
                // テンプレートの取得
                var templates = context.Templates
                    .Include(t => t.TemplateType)
                    .Include(t => t.ConditionType)
                    .Include(t => t.Conditions)
                    .Include(t => t.Commands)
                    .ToList();

                foreach (var template in templates)
                {
                    Console.WriteLine($"Template ID: {template.TemplateID}");
                    Console.WriteLine($"Template Type: {template.TemplateType.Name}");
                    if (template.ConditionType != null)
                    {
                        Console.WriteLine($"Condition Type: {template.ConditionType.Name}");
                    }

                    Console.WriteLine("Conditions:");
                    foreach (var condition in template.Conditions)
                    {
                        Console.WriteLine($"  - {condition.KeyMaster.Name}: {condition.Value}");
                    }

                    Console.WriteLine("Commands:");
                    foreach (var command in template.Commands)
                    {
                        Console.WriteLine($"  - Command Type: {command.CommandType.Name}, Device: {command.TargetDeviceID}, Data: {command.Data}");
                    }

                    Console.WriteLine();
                }
            }
        }
    }
}

using ClientSample.Dbs;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ClientSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

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
                    Debug.WriteLine($"Template ID: {template.TemplateID}");
                    Debug.WriteLine($"Template Type: {template.TemplateType.Name}");
                    if (template.ConditionType != null)
                    {
                        Debug.WriteLine($"Condition Type: {template.ConditionType.Name}");
                    }

                    Debug.WriteLine("Conditions:");
                    foreach (var condition in template.Conditions)
                    {
                        Debug.WriteLine($"  - {condition.KeyMaster.Name}: {condition.Value}");
                    }

                    Debug.WriteLine("Commands:");
                    foreach (var command in template.Commands)
                    {
                        Debug.WriteLine($"  - Command Type: {command.CommandType.Name}, Device: {command.TargetDeviceID}, Data: {command.Data}");
                    }

                    Console.WriteLine();
                }
            }
        }
    }
}

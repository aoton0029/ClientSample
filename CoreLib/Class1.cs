using CoreLib.Commands;
using CoreLib.Models;
using CoreLib.Templates;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CoreLib
{
    public class Class1
    {
        public async void Main(string[] args)
        {
            // デバイスの設定
            Device device = new Device(new TcpConnection("", 8080), "");

            // 全テンプレートの作成
            var templates = new Dictionary<string, Template>
        {
            { "Threshold1", new ThresholdTemplate("Threshold1") },
            { "Case1", new CaseTemplate("Case1") },
            { "End1", new EndTemplate("End1") }
        };

            // 各テンプレートの初期化
            templates["Threshold1"].Initialize(
                new List<ICommand> { new ReceiveCommand() },
                new Dictionary<string, object>
                {
                { "threshold", 50.0m },
                { "aboveThresholdID", "Case1" },
                { "belowThresholdID", "End1" }
                }
            );

            templates["Case1"].Initialize(
                new List<ICommand> { new ReceiveCommand() },
                new Dictionary<string, object>
                {
                { "cases", new Dictionary<string, string>
                    {
                        { "SUCCESS", "End1" },
                        { "FAILURE", "End1" }
                    }
                }
                }
            );

            templates["End1"].Initialize(
                new List<ICommand>(),
                null
            );

            // 初期テンプレート
            Template currentTemplate = templates["Threshold1"];

            while (currentTemplate != null)
            {
                await currentTemplate.RunAsync(device);

                Console.WriteLine($"Message: {currentTemplate.Result.Message}");

                string nextTemplateID = currentTemplate.Result.NextTemplate;
                if (string.IsNullOrEmpty(nextTemplateID) || !templates.ContainsKey(nextTemplateID))
                {
                    break;
                }

                currentTemplate = templates[nextTemplateID];
            }

            Console.WriteLine("Process Completed.");
        }
    }
}

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
            Device device = new Device(new TcpConnection());

            // テンプレートの構築
            var templates = new Dictionary<string, Template>
        {
            { "ThresholdTemplate", new ThresholdTemplate() },
            { "CaseTemplate", new CaseTemplate() },
            { "EndTemplate", new EndTemplate() }
        };

            // 初期テンプレートとコマンドリスト
            Template currentTemplate = templates["ThresholdTemplate"];
            var initialCommands = new List<ICommand> { new ReceiveCommand() };

            while (currentTemplate != null)
            {
                // テンプレートごとにパラメータを渡す
                var parameters = new Dictionary<string, object>();

                if (currentTemplate is ThresholdTemplate)
                {
                    parameters["threshold"] = 50.0m;
                }

                await currentTemplate.InitializeAsync(initialCommands, parameters);
                await currentTemplate.RunAsync(device);

                Console.WriteLine($"Message: {currentTemplate.Result.Message}");

                string nextTemplate = currentTemplate.Result.NextTemplate;
                if (string.IsNullOrEmpty(nextTemplate) || !templates.ContainsKey(nextTemplate))
                {
                    break;
                }

                currentTemplate = templates[nextTemplate];
            }

            Console.WriteLine("Process Completed.");
        }
    }
}

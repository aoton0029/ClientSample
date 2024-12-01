using CoreLib.Templates;
using System.Text.Json;

namespace CoreLib
{
    public class Class1
    {
        public static void Main(string[] args)
        {
            // JSONファイルの読み込み
            string jsonFilePath = "templates.json";
            string jsonContent = File.ReadAllText(jsonFilePath);
            var templatesConfig = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(jsonContent);

            Dictionary<string, BaseTemplate> templates = new Dictionary<string, BaseTemplate>();

            // テンプレート構築
            foreach (var templateInfo in templatesConfig["templates"])
            {
                string id = templateInfo["id"].ToString();
                string name = templateInfo["name"].ToString();
                dynamic nextTemplate = templateInfo["nextTemplate"];

                BaseTemplate template = name switch
                {
                    "NumericThresholdTemplate" => new NumericThresholdTemplate(id, name, nextTemplate),
                    "TextMatchTemplate" => new TextMatchTemplate(id, name, nextTemplate),
                    "EndTemplate" => new EndTemplate(id, name),
                    _ => null
                };

                if (template != null)
                {
                    templates[id] = template;
                }
            }

            // 実行開始
            BaseTemplate currentTemplate = templates["1"]; // 最初のテンプレート
            while (currentTemplate != null)
            {
                currentTemplate.Run();
                Console.WriteLine($"Message: {currentTemplate.Name}");

                string nextTemplateId = currentTemplate.GetNextTemplateId();
                if (string.IsNullOrEmpty(nextTemplateId) || !templates.ContainsKey(nextTemplateId))
                {
                    break;
                }

                currentTemplate = templates[nextTemplateId];
            }

            Console.WriteLine("Process Completed.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerSample
{
    internal class FileUtils
    {
        /// <summary>
        /// オブジェクトをJSONファイルに書き込む
        /// </summary>
        public static void WriteToJsonFile<T>(string filePath, T obj, bool indent = false)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indent // 整形されたJSONを出力するかどうか
            };

            string json = JsonSerializer.Serialize(obj, options);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// JSONファイルからオブジェクトを読み込む
        /// </summary>
        public static T ReadFromJsonFile<T>(string filePath, T defaultValue=null) where T : class, new()
        {
            if (!File.Exists(filePath))
            {
                defaultValue ??= new T(); // Tのデフォルト値を作成
                WriteToJsonFile(filePath, defaultValue, indent: true);
                return defaultValue;
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}

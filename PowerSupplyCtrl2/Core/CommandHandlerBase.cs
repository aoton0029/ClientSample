using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowersupplyCtrl2
{
    public abstract class CommandHandlerBase
    {
        protected readonly IConnection _connection;
        protected readonly Encoding _encoding;
        protected readonly string _termination;

        public CommandHandlerBase(IConnection connection, Encoding encoding = null, string termination = "\n")
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _encoding = encoding ?? Encoding.ASCII;
            _termination = termination;
        }

        /// <summary>
        /// SCPIコマンドを送信します
        /// </summary>
        /// <param name="command">送信するコマンド</param>
        /// <returns>送信タスク</returns>
        public async Task SendCommandAsync(string command)
        {
            if (!_connection.IsConnected)
                throw new InvalidOperationException("接続が確立されていません。");

            // 終端文字を追加
            if (!command.EndsWith(_termination))
                command += _termination;

            byte[] data = _encoding.GetBytes(command);
            await _connection.SendAsync(data);
        }
        /// <summary>
        /// SCPIコマンドを送信し、応答を受信します
        /// </summary>
        /// <param name="command">送信するクエリコマンド</param>
        /// <param name="timeout">受信タイムアウト（ミリ秒）</param>
        /// <returns>デバイスからの応答</returns>
        public async Task<string> QueryAsync(string command, int timeout = 5000)
        {
            if (!_connection.IsConnected)
                throw new InvalidOperationException("接続が確立されていません。");

            // クエリコマンドであることを確認（?で終わるか確認）
            if (!command.TrimEnd(_termination.ToCharArray()).EndsWith("?"))
                throw new ArgumentException("クエリコマンドは '?' で終わる必要があります。", nameof(command));

            // コマンド送信
            await SendCommandAsync(command);

            // 応答受信
            byte[] responseData = await _connection.ReceiveAsync(timeout);
            string response = _encoding.GetString(responseData);

            // 終端文字を削除して返す
            return response.TrimEnd(_termination.ToCharArray());
        }

        /// <summary>
        /// SCPIコマンドを送信し、応答を指定した型に変換して返します
        /// </summary>
        /// <typeparam name="T">変換先の型</typeparam>
        /// <param name="command">送信するクエリコマンド</param>
        /// <param name="timeout">受信タイムアウト（ミリ秒）</param>
        /// <returns>変換された応答</returns>
        public async Task<T> QueryAsync<T>(string command, int timeout = 5000)
        {
            string response = await QueryAsync(command, timeout);

            // 文字列を指定された型に変換
            return ConvertResponse<T>(response);
        }

        /// <summary>
        /// 文字列応答を指定した型に変換します
        /// </summary>
        /// <typeparam name="T">変換先の型</typeparam>
        /// <param name="response">変換する文字列</param>
        /// <returns>変換された値</returns>
        private T ConvertResponse<T>(string response)
        {
            // null または空文字列の場合
            if (string.IsNullOrWhiteSpace(response))
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)string.Empty;
                else
                    return default;
            }

            // 文字列をトリミング
            response = response.Trim();

            // 型に応じて変換
            Type targetType = typeof(T);

            if (targetType == typeof(string))
                return (T)(object)response;

            else if (targetType == typeof(double))
                return (T)(object)double.Parse(response, CultureInfo.InvariantCulture);

            else if (targetType == typeof(float))
                return (T)(object)float.Parse(response, CultureInfo.InvariantCulture);

            else if (targetType == typeof(int))
                return (T)(object)int.Parse(response, CultureInfo.InvariantCulture);

            else if (targetType == typeof(bool))
            {
                // 数値の場合（0=false, 非0=true）
                if (int.TryParse(response, out int intValue))
                    return (T)(object)(intValue != 0);

                // 文字列の場合
                return (T)(object)(response.ToUpper() == "TRUE" || response.ToUpper() == "ON" || response.ToUpper() == "YES" || response == "1");
            }

            else if (targetType == typeof(DateTime))
            {
                // SCPIで一般的な日付フォーマット
                if (DateTime.TryParse(response, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
                    return (T)(object)dateValue;
            }

            else if (targetType.IsEnum)
            {
                // 数値によるEnum変換
                if (int.TryParse(response, out int enumValue))
                    return (T)Enum.ToObject(targetType, enumValue);

                // 文字列によるEnum変換
                return (T)Enum.Parse(targetType, response, true);
            }

            else if (targetType == typeof(byte[]))
            {
                // カンマ区切りの数値をバイト配列に変換
                string[] values = response.Split(',');
                byte[] bytes = new byte[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    bytes[i] = byte.Parse(values[i].Trim());
                }

                return (T)(object)bytes;
            }

            // 対応していない型の場合
            throw new NotSupportedException($"型 {targetType.Name} への変換はサポートされていません。");
        }

        /// <summary>
        /// 複数のコンマ区切り値を配列として取得します
        /// </summary>
        /// <typeparam name="T">要素の型</typeparam>
        /// <param name="command">クエリコマンド</param>
        /// <returns>変換された配列</returns>
        public async Task<T[]> GetArrayAsync<T>(string command)
        {
            string response = await QueryAsync(command);
            string[] values = response.Split(',');
            T[] result = new T[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                // 各値を個別に変換
                result[i] = ConvertResponse<T>(values[i]);
            }

            return result;
        }
    }
}

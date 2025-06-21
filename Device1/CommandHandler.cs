using Device1.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Device1
{
    /// <summary>
    /// SCPIコマンド送受信を管理するクラス
    /// </summary>
    public class CommandHandler : CommandHandlerBase
    {
        public CommandHandler(IConnection connection, Encoding encoding = null, string termination = "\n") : base(connection, encoding, termination)
        {
        }

        /// <summary>
        /// 機器識別情報を取得します
        /// </summary>
        /// <returns>機器識別情報</returns>
        public async Task<string> GetIdentificationAsync()
        {
            return await QueryAsync("*IDN?");
        }

        /// <summary>
        /// 機器をリセットします
        /// </summary>
        public async Task ResetInstrumentAsync()
        {
            await SendCommandAsync("*RST");
        }

        /// <summary>
        /// 機器の操作完了を待機します
        /// </summary>
        public async Task WaitForOperationCompleteAsync()
        {
            await QueryAsync("*OPC?");
        }

        /// <summary>
        /// エラーメッセージを取得します
        /// </summary>
        /// <returns>エラーメッセージ（エラーがない場合は空文字）</returns>
        public async Task<string> GetErrorMessageAsync()
        {
            string response = await QueryAsync("SYST:ERR?");
            // 典型的なエラー応答形式: "0, No error" または "[エラーコード], [エラーメッセージ]"
            Match match = Regex.Match(response, @"(\d+),\s*(.*)");
            if (match.Success && match.Groups[1].Value != "0")
            {
                return match.Groups[2].Value.Trim();
            }
            return string.Empty;
        }

        /// <summary>
        /// ローカル操作モード（フロントパネル操作可能）に設定します
        /// </summary>
        public async Task SetLocalModeAsync()
        {
            await SendCommandAsync("SYST:LOC");
        }

        /// <summary>
        /// リモート操作モードに設定します
        /// </summary>
        public async Task SetRemoteModeAsync()
        {
            await SendCommandAsync("SYST:REM");
        }

        /// <summary>
        /// 電圧測定値を取得します
        /// </summary>
        /// <returns>電圧測定値</returns>
        public async Task<double> GetVolt()
        {
            return await QueryAsync<double>("MEAS:VOLT?");
        }

        /// <summary>
        /// 電流測定値を取得します
        /// </summary>
        /// <returns>電流測定値</returns>
        public async Task<double> GetCurrentAsync()
        {
            return await QueryAsync<double>("MEAS:CURR?");
        }

        /// <summary>
        /// 抵抗測定値を取得します
        /// </summary>
        /// <returns>抵抗測定値</returns>
        public async Task<double> GetResistanceAsync()
        {
            return await QueryAsync<double>("MEAS:RES?");
        }

        /// <summary>
        /// 温度測定値を取得します
        /// </summary>
        /// <returns>温度測定値</returns>
        public async Task<double> GetTemperatureAsync()
        {
            return await QueryAsync<double>("MEAS:TEMP?");
        }

        /// <summary>
        /// 出力状態を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>出力状態（true=ON, false=OFF）</returns>
        public async Task<bool> GetOutputStateAsync(int? channel = null)
        {
            string command = channel.HasValue ? $"OUTP:STAT? (@{channel.Value})" : "OUTP:STAT?";
            return await QueryAsync<bool>(command);
        }

        /// <summary>
        /// 出力状態を設定します
        /// </summary>
        /// <param name="enable">有効にする場合はtrue</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task SetOutputStateAsync(bool enable, int? channel = null)
        {
            string state = enable ? "ON" : "OFF";
            string command = channel.HasValue ? $"OUTP:STAT {state},(@{channel.Value})" : $"OUTP:STAT {state}";
            await SendCommandAsync(command);
        }

    }
}

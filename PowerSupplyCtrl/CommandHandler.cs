using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PowersupplyCtrl
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

        #region 松定プレシジョン電源装置の制御コマンド

        /// <summary>
        /// 電圧値を設定します
        /// </summary>
        /// <param name="voltage">設定電圧値（単位: V）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task SetVoltageAsync(double voltage, int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"SOUR{channel}:VOLT {voltage.ToString(CultureInfo.InvariantCulture)}"
                : $"SOUR:VOLT {voltage.ToString(CultureInfo.InvariantCulture)}";

            await SendCommandAsync(cmd);
        }

        /// <summary>
        /// 設定されている電圧値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定電圧値（単位: V）</returns>
        public async Task<double> GetVoltageSettingAsync(int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"SOUR{channel}:VOLT?"
                : "SOUR:VOLT?";

            string response = await QueryAsync(cmd);
            return double.Parse(response, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 電流値を設定します
        /// </summary>
        /// <param name="current">設定電流値（単位: A）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task SetCurrentAsync(double current, int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"SOUR{channel}:CURR {current.ToString(CultureInfo.InvariantCulture)}"
                : $"SOUR:CURR {current.ToString(CultureInfo.InvariantCulture)}";

            await SendCommandAsync(cmd);
        }

        /// <summary>
        /// 設定されている電流値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定電流値（単位: A）</returns>
        public async Task<double> GetCurrentSettingAsync(int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"SOUR{channel}:CURR?"
                : "SOUR:CURR?";

            string response = await QueryAsync(cmd);
            return double.Parse(response, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 出力状態を設定します
        /// </summary>
        /// <param name="enable">出力を有効にする場合はtrue</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task SetOutputStateAsync(bool enable, int? channel = null)
        {
            string state = enable ? "ON" : "OFF";
            string cmd = channel.HasValue
                ? $"OUTP{channel} {state}"
                : $"OUTP {state}";

            await SendCommandAsync(cmd);
        }

        /// <summary>
        /// 出力状態を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>出力状態（true=ON, false=OFF）</returns>
        public async Task<bool> GetOutputStateAsync(int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"OUTP{channel}?"
                : "OUTP?";

            string response = await QueryAsync(cmd);
            return response.Trim() == "1";
        }

        /// <summary>
        /// 実測電圧値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>実測電圧値（単位: V）</returns>
        public async Task<double> GetMeasuredVoltageAsync(int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"MEAS{channel}:VOLT?"
                : "MEAS:VOLT?";

            string response = await QueryAsync(cmd);
            return double.Parse(response, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 実測電流値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>実測電流値（単位: A）</returns>
        public async Task<double> GetMeasuredCurrentAsync(int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"MEAS{channel}:CURR?"
                : "MEAS:CURR?";

            string response = await QueryAsync(cmd);
            return double.Parse(response, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 現在の動作モード（CC/CV）を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>動作モード（"CV"または"CC"）</returns>
        public async Task<string> GetOperationModeAsync(int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"STAT{channel}:OPER:COND?"
                : "STAT:OPER:COND?";

            string response = await QueryAsync(cmd);
            int statusBits = int.Parse(response);

            // 通常、CCモードとCVモードはステータスレジスタの特定ビットで表される
            // ビット位置はデバイスによって異なる場合があるため、必要に応じて調整
            bool isCV = (statusBits & 0x01) != 0;  // 例: ビット0がCVモード
            bool isCC = (statusBits & 0x02) != 0;  // 例: ビット1がCCモード

            if (isCV) return "CV";
            if (isCC) return "CC";
            return "Unknown";
        }

        /// <summary>
        /// 電圧上限値を設定します
        /// </summary>
        /// <param name="voltageLimit">電圧上限値（単位: V）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task SetVoltageLimitAsync(double voltageLimit, int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"SOUR{channel}:VOLT:PROT {voltageLimit.ToString(CultureInfo.InvariantCulture)}"
                : $"SOUR:VOLT:PROT {voltageLimit.ToString(CultureInfo.InvariantCulture)}";

            await SendCommandAsync(cmd);
        }

        /// <summary>
        /// 電流上限値を設定します
        /// </summary>
        /// <param name="currentLimit">電流上限値（単位: A）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task SetCurrentLimitAsync(double currentLimit, int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"SOUR{channel}:CURR:PROT {currentLimit.ToString(CultureInfo.InvariantCulture)}"
                : $"SOUR:CURR:PROT {currentLimit.ToString(CultureInfo.InvariantCulture)}";

            await SendCommandAsync(cmd);
        }

        /// <summary>
        /// 保護機能をリセットします
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task ResetProtectionAsync(int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"OUTP{channel}:PROT:CLE"
                : "OUTP:PROT:CLE";

            await SendCommandAsync(cmd);
        }

        /// <summary>
        /// 保護状態を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>保護機能の状態コード</returns>
        public async Task<int> GetProtectionStatusAsync(int? channel = null)
        {
            string cmd = channel.HasValue
                ? $"STAT{channel}:QUES:COND?"
                : "STAT:QUES:COND?";

            string response = await QueryAsync(cmd);
            return int.Parse(response);
        }

        /// <summary>
        /// エラーメッセージを取得します
        /// </summary>
        /// <returns>エラーメッセージ（エラーがない場合は空文字）</returns>
        public async Task<string> GetErrorMessageAsync()
        {
            string response = await QueryAsync("SYST:ERR?");
            // 典型的なエラー応答形式: "0, No error" または "[エラーコード], [エラーメッセージ]"
            Match match = Regex.Match(response, @"(-?\d+),\s*(.*)");
            if (match.Success && match.Groups[1].Value != "0")
            {
                return match.Groups[2].Value.Trim();
            }
            return string.Empty;
        }

        /// <summary>
        /// 電圧測定を行います (GetMeasuredVoltageAsync のエイリアス)
        /// </summary>
        /// <returns>電圧測定値</returns>
        public async Task<double> GetVolt()
        {
            return await GetMeasuredVoltageAsync();
        }

        /// <summary>
        /// 電流測定を行います (GetMeasuredCurrentAsync のエイリアス)
        /// </summary>
        /// <returns>電流測定値</returns>
        public async Task<double> GetCurrentAsync()
        {
            return await GetMeasuredCurrentAsync();
        }

        #endregion
    }
}

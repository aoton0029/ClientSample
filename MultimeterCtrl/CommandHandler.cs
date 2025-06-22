using MultimeterCtrl.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultimeterCtrl
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

        #region DMM6500特有の測定機能

        /// <summary>
        /// 現在の機能（測定モード）を設定します
        /// </summary>
        /// <param name="function">測定機能（"VOLT:DC", "VOLT:AC", "CURR:DC", "CURR:AC", "RES", "FRES", "TEMP", etc.）</param>
        public async Task SetFunctionAsync(string function)
        {
            await SendCommandAsync($"SENS:FUNC '{function}'");
        }

        /// <summary>
        /// 現在の機能（測定モード）を取得します
        /// </summary>
        /// <returns>現在の測定機能</returns>
        public async Task<string> GetFunctionAsync()
        {
            return await QueryAsync("SENS:FUNC?");
        }

        /// <summary>
        /// 電圧測定レンジを設定します（DC電圧モード用）
        /// </summary>
        /// <param name="range">レンジ値（単位: V）、AUTO の場合は null</param>
        public async Task SetVoltageRangeAsync(double? range)
        {
            if (range.HasValue)
            {
                await SendCommandAsync($"SENS:VOLT:DC:RANG {range.Value}");
            }
            else
            {
                await SendCommandAsync("SENS:VOLT:DC:RANG:AUTO ON");
            }
        }

        /// <summary>
        /// 電流測定レンジを設定します（DC電流モード用）
        /// </summary>
        /// <param name="range">レンジ値（単位: A）、AUTO の場合は null</param>
        public async Task SetCurrentRangeAsync(double? range)
        {
            if (range.HasValue)
            {
                await SendCommandAsync($"SENS:CURR:DC:RANG {range.Value}");
            }
            else
            {
                await SendCommandAsync("SENS:CURR:DC:RANG:AUTO ON");
            }
        }

        /// <summary>
        /// 抵抗測定レンジを設定します
        /// </summary>
        /// <param name="range">レンジ値（単位: Ω）、AUTO の場合は null</param>
        public async Task SetResistanceRangeAsync(double? range)
        {
            if (range.HasValue)
            {
                await SendCommandAsync($"SENS:RES:RANG {range.Value}");
            }
            else
            {
                await SendCommandAsync("SENS:RES:RANG:AUTO ON");
            }
        }

        /// <summary>
        /// NPLCを設定します（積分時間、電源周期数）
        /// </summary>
        /// <param name="nplc">NPLC値（0.0005～15）</param>
        public async Task SetNPLCAsync(double nplc)
        {
            string function = await GetFunctionAsync();
            function = function.Trim('\'');
            await SendCommandAsync($"SENS:{function}:NPLC {nplc}");
        }

        /// <summary>
        /// 現在のNPLC値を取得します
        /// </summary>
        /// <returns>NPLC値</returns>
        public async Task<double> GetNPLCAsync()
        {
            string function = await GetFunctionAsync();
            function = function.Trim('\'');
            return await QueryAsync<double>($"SENS:{function}:NPLC?");
        }

        /// <summary>
        /// 電圧測定を実行します
        /// </summary>
        /// <returns>電圧測定値</returns>
        public async Task<double> GetVolt()
        {
            // 先に機能を切り替えてから測定
            await SetFunctionAsync("VOLT:DC");
            return await QueryAsync<double>("READ?");
        }

        /// <summary>
        /// AC電圧測定を実行します
        /// </summary>
        /// <returns>AC電圧測定値</returns>
        public async Task<double> GetAcVoltAsync()
        {
            await SetFunctionAsync("VOLT:AC");
            return await QueryAsync<double>("READ?");
        }

        /// <summary>
        /// DC電流測定を実行します
        /// </summary>
        /// <returns>電流測定値</returns>
        public async Task<double> GetCurrentAsync()
        {
            await SetFunctionAsync("CURR:DC");
            return await QueryAsync<double>("READ?");
        }

        /// <summary>
        /// AC電流測定を実行します
        /// </summary>
        /// <returns>AC電流測定値</returns>
        public async Task<double> GetAcCurrentAsync()
        {
            await SetFunctionAsync("CURR:AC");
            return await QueryAsync<double>("READ?");
        }

        /// <summary>
        /// 2線式抵抗測定を実行します
        /// </summary>
        /// <returns>抵抗測定値</returns>
        public async Task<double> GetResistanceAsync()
        {
            await SetFunctionAsync("RES");
            return await QueryAsync<double>("READ?");
        }

        /// <summary>
        /// 4線式抵抗測定を実行します
        /// </summary>
        /// <returns>抵抗測定値</returns>
        public async Task<double> Get4WireResistanceAsync()
        {
            await SetFunctionAsync("FRES");
            return await QueryAsync<double>("READ?");
        }

        /// <summary>
        /// 温度測定を実行します
        /// </summary>
        /// <returns>温度測定値</returns>
        public async Task<double> GetTemperatureAsync()
        {
            await SetFunctionAsync("TEMP");
            return await QueryAsync<double>("READ?");
        }

        #endregion

        #region DMM6500のデジタイザー機能

        /// <summary>
        /// デジタイザーサンプリングレートを設定します
        /// </summary>
        /// <param name="rate">サンプリングレート（Hz）</param>
        public async Task SetDigitizerSampleRateAsync(double rate)
        {
            await SendCommandAsync($"DIG:FUNC:RATE {rate}");
        }

        /// <summary>
        /// デジタイザーカウント（サンプル数）を設定します
        /// </summary>
        /// <param name="count">サンプル数</param>
        public async Task SetDigitizerCountAsync(int count)
        {
            await SendCommandAsync($"DIG:COUN {count}");
        }

        /// <summary>
        /// デジタイザーを使って電圧波形を取得します
        /// </summary>
        /// <returns>電圧サンプル配列</returns>
        public async Task<double[]> GetDigitizerVoltageWaveformAsync()
        {
            await SendCommandAsync("DIG:FUNC \"VOLT\"");
            await SendCommandAsync("INIT");
            await SendCommandAsync("*WAI");
            return await GetArrayAsync<double>("TRAC:DATA? 1, 100000");
        }

        /// <summary>
        /// デジタイザーを使って電流波形を取得します
        /// </summary>
        /// <returns>電流サンプル配列</returns>
        public async Task<double[]> GetDigitizerCurrentWaveformAsync()
        {
            await SendCommandAsync("DIG:FUNC \"CURR\"");
            await SendCommandAsync("INIT");
            await SendCommandAsync("*WAI");
            return await GetArrayAsync<double>("TRAC:DATA? 1, 100000");
        }

        #endregion

        #region DMM6500のシステム制御

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
        /// リモートロック操作モードに設定します（フロントパネルロック）
        /// </summary>
        public async Task SetRemoteLockModeAsync()
        {
            await SendCommandAsync("SYST:RWL");
        }

        /// <summary>
        /// 現在の日時を取得します
        /// </summary>
        /// <returns>デバイスの現在時刻</returns>
        public async Task<DateTime> GetSystemTimeAsync()
        {
            string dateTimeStr = await QueryAsync("SYST:DATE?; TIME?");
            string[] parts = dateTimeStr.Split(';');

            if (parts.Length >= 2)
            {
                string[] dateParts = parts[0].Split(',');
                string[] timeParts = parts[1].Split(',');

                if (dateParts.Length >= 3 && timeParts.Length >= 3)
                {
                    int year = int.Parse(dateParts[0]);
                    int month = int.Parse(dateParts[1]);
                    int day = int.Parse(dateParts[2]);

                    int hour = int.Parse(timeParts[0]);
                    int minute = int.Parse(timeParts[1]);
                    int second = int.Parse(timeParts[2]);

                    return new DateTime(year, month, day, hour, minute, second);
                }
            }

            throw new FormatException("デバイスから無効な日時形式が返されました。");
        }

        #endregion

        #region DMM6500のトリガーシステム

        /// <summary>
        /// トリガーモデルをセットアップします
        /// </summary>
        /// <param name="count">トリガーカウント数</param>
        public async Task SetupTriggerModelAsync(int count)
        {
            await SendCommandAsync($"TRIG:LOAD \"SimpleLoop\", {count}");
        }

        /// <summary>
        /// トリガーを開始します
        /// </summary>
        public async Task InitiateTriggerAsync()
        {
            await SendCommandAsync("INIT");
        }

        /// <summary>
        /// 指定時間待機します
        /// </summary>
        /// <param name="seconds">待機時間（秒）</param>
        public async Task DelayAsync(double seconds)
        {
            await SendCommandAsync($"TRIG:TIM {seconds}");
        }

        /// <summary>
        /// マルチメーターの操作完了を待機します
        /// </summary>
        public async Task WaitForOperationCompleteAsync()
        {
            await QueryAsync("*OPC?");
        }

        #endregion

        #region DMM6500のスキャナー制御（オプションのスキャナーカード用）

        /// <summary>
        /// 指定されたチャンネルをクローズします
        /// </summary>
        /// <param name="channel">チャンネル番号または範囲（例: "1", "1:5"）</param>
        public async Task CloseChannelAsync(string channel)
        {
            await SendCommandAsync($"ROUT:CLOS (@{channel})");
        }

        /// <summary>
        /// すべてのチャンネルをオープンします
        /// </summary>
        public async Task OpenAllChannelsAsync()
        {
            await SendCommandAsync("ROUT:OPEN:ALL");
        }

        /// <summary>
        /// 現在閉じているチャンネルを取得します
        /// </summary>
        /// <returns>閉じているチャンネルのリスト</returns>
        public async Task<string> GetClosedChannelsAsync()
        {
            return await QueryAsync("ROUT:CLOS:STAT?");
        }

        /// <summary>
        /// スキャン設定を構成します
        /// </summary>
        /// <param name="channels">スキャンするチャンネル（例: "1:4"）</param>
        /// <param name="count">スキャン回数</param>
        public async Task ConfigureScanAsync(string channels, int count)
        {
            await SendCommandAsync($"ROUT:SCAN:CRE (@{channels})");
            await SendCommandAsync($"ROUT:SCAN:COUN:SCAN {count}");
        }

        /// <summary>
        /// スキャンを開始します
        /// </summary>
        public async Task StartScanAsync()
        {
            await SendCommandAsync("ROUT:SCAN:INIT");
        }

        #endregion

    }
}

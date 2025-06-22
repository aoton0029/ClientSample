using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowersupplyCtrl2;

namespace InspectionApp
{
    public class PowerSupplyCtrl2Adapter : IPowerSupply
    {
        private readonly PowersupplyCtrl2.PSCtrl _powerSupply;

        /// <summary>
        /// 指定した接続を使用してアダプターを初期化します
        /// </summary>
        /// <param name="connection">使用する接続インターフェース</param>
        public PowerSupplyCtrl2Adapter(PowersupplyCtrl2.IConnection connection)
        {
            _powerSupply = new PowersupplyCtrl2.PSCtrl(connection);
            _powerSupply.ConnectionStatusChanged += OnPowerSupplyConnectionChanged;
            _powerSupply.ErrorOccurred += OnPowerSupplyErrorOccurred;
        }

        /// <summary>
        /// TCP接続を使用してアダプターを初期化します
        /// </summary>
        /// <param name="ipAddress">接続先IPアドレス</param>
        /// <param name="port">接続先ポート番号</param>
        public PowerSupplyCtrl2Adapter(string ipAddress, int port)
        {
            _powerSupply = new PowersupplyCtrl2.PSCtrl(ipAddress, port);
            _powerSupply.ConnectionStatusChanged += OnPowerSupplyConnectionChanged;
            _powerSupply.ErrorOccurred += OnPowerSupplyErrorOccurred;
        }

        /// <summary>
        /// シリアルポート接続を使用してアダプターを初期化します
        /// </summary>
        /// <param name="portName">シリアルポート名（例: "COM1"）</param>
        /// <param name="baudRate">ボーレート（デフォルト: 9600）</param>
        /// <param name="parity">パリティ設定（デフォルト: None）</param>
        /// <param name="dataBits">データビット（デフォルト: 8）</param>
        /// <param name="stopBits">ストップビット（デフォルト: One）</param>
        public PowerSupplyCtrl2Adapter(string portName, int baudRate = 9600, Parity parity = Parity.None,
                                     int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _powerSupply = new PowersupplyCtrl2.PSCtrl(portName, baudRate, parity, dataBits, stopBits);
            _powerSupply.ConnectionStatusChanged += OnPowerSupplyConnectionChanged;
            _powerSupply.ErrorOccurred += OnPowerSupplyErrorOccurred;
        }

        /// <summary>
        /// 接続状態を取得します
        /// </summary>
        public bool IsConnected => _powerSupply.IsConnected;

        /// <summary>
        /// 初期化状態を取得します
        /// </summary>
        public bool IsInitialized => _powerSupply.IsInitialized;

        /// <summary>
        /// 接続状態が変化したときに発生するイベント
        /// </summary>
        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        /// <summary>
        /// エラーが発生したときに発生するイベント
        /// </summary>
        public event EventHandler<PowerSupplyErrorEventArgs>? ErrorOccurred;

        /// <summary>
        /// デバイスに接続します
        /// </summary>
        /// <returns>接続結果</returns>
        public async Task<bool> ConnectAsync()
        {
            return await _powerSupply.ConnectAsync();
        }

        /// <summary>
        /// デバイスから切断します
        /// </summary>
        public async Task DisconnectAsync()
        {
            await _powerSupply.DisconnectAsync();
        }

        /// <summary>
        /// デバイスを初期化します
        /// </summary>
        public async Task InitializeAsync()
        {
            await _powerSupply.InitializeAsync();
        }

        /// <summary>
        /// 電圧値を設定します
        /// </summary>
        /// <param name="voltage">設定電圧値（単位: V）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task SetVoltageAsync(double voltage, int? channel = null)
        {
            await _powerSupply.SetVoltageAsync(voltage, channel);
        }

        /// <summary>
        /// 設定されている電圧値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定電圧値（単位: V）</returns>
        public async Task<double> GetVoltageSettingAsync(int? channel = null)
        {
            return await _powerSupply.GetVoltageSettingAsync(channel);
        }

        /// <summary>
        /// 電流値を設定します
        /// </summary>
        /// <param name="current">設定電流値（単位: A）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task SetCurrentAsync(double current, int? channel = null)
        {
            await _powerSupply.SetCurrentAsync(current, channel);
        }

        /// <summary>
        /// 設定されている電流値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定電流値（単位: A）</returns>
        public async Task<double> GetCurrentSettingAsync(int? channel = null)
        {
            return await _powerSupply.GetCurrentSettingAsync(channel);
        }

        /// <summary>
        /// 出力状態を設定します
        /// </summary>
        /// <param name="enable">出力を有効にする場合はtrue</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        public async Task SetOutputStateAsync(bool enable, int? channel = null)
        {
            await _powerSupply.SetOutputStateAsync(enable, channel);
        }

        /// <summary>
        /// 出力状態を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>出力状態（true=ON, false=OFF）</returns>
        public async Task<bool> GetOutputStateAsync(int? channel = null)
        {
            return await _powerSupply.GetOutputStateAsync(channel);
        }

        /// <summary>
        /// 実測電圧値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>実測電圧値（単位: V）</returns>
        public async Task<double> MeasureVoltageAsync(int? channel = null)
        {
            return await _powerSupply.MeasureVoltageAsync(channel);
        }

        /// <summary>
        /// 実測電流値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>実測電流値（単位: A）</returns>
        public async Task<double> MeasureCurrentAsync(int? channel = null)
        {
            return await _powerSupply.MeasureCurrentAsync(channel);
        }

        /// <summary>
        /// デバイスのエラーメッセージを取得します
        /// </summary>
        /// <returns>エラーメッセージ（エラーがない場合は空文字）</returns>
        public async Task<string> GetErrorMessageAsync()
        {
            return await _powerSupply.GetErrorMessageAsync();
        }

        private void OnPowerSupplyConnectionChanged(object? sender, PowersupplyCtrl2.ConnectionEventArgs e)
        {
            ConnectionStatusChanged?.Invoke(this,
                new ConnectionStatusChangedEventArgs(e.IsConnected, e.Message));
        }

        private void OnPowerSupplyErrorOccurred(object? sender, PowersupplyCtrl2.PowerSupplyErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this,
                new PowerSupplyErrorEventArgs(e.Exception, (int)e.ErrorCode, e.Message));
        }
    }
}

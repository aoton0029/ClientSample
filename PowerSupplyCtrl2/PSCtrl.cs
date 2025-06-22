using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowersupplyCtrl2
{
    /// <summary>
    /// Device1を制御するためのクラス
    /// </summary>
    public class PSCtrl
    {
        private readonly CommandHandler _commandHandler;
        private readonly IConnection _connection;
        private bool _isInitialized = false;

        /// <summary>
        /// PSCtrlを初期化します
        /// </summary>
        /// <param name="connection">使用する接続インターフェース</param>
        public PSCtrl(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _commandHandler = new CommandHandler(connection);

            // 接続状態の変更イベントをハンドリング
            _connection.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        /// <summary>
        /// TCP接続を使用してPSCtrlを初期化します
        /// </summary>
        /// <param name="ipAddress">接続先IPアドレス</param>
        /// <param name="port">接続先ポート番号</param>
        public PSCtrl(string ipAddress, int port)
            : this(new TcpConnection(ipAddress, port))
        {
        }

        /// <summary>
        /// シリアルポート接続を使用してPSCtrlを初期化します
        /// </summary>
        /// <param name="portName">シリアルポート名（例: "COM1"）</param>
        /// <param name="baudRate">ボーレート（デフォルト: 9600）</param>
        /// <param name="parity">パリティ設定（デフォルト: None）</param>
        /// <param name="dataBits">データビット（デフォルト: 8）</param>
        /// <param name="stopBits">ストップビット（デフォルト: One）</param>
        public PSCtrl(string portName, int baudRate = 9600, Parity parity = Parity.None,
                     int dataBits = 8, StopBits stopBits = StopBits.One)
            : this(new SerialPortConnection(portName, baudRate, parity, dataBits, stopBits))
        {
        }

        /// <summary>
        /// デバイスの接続状態が変化したときに発生するイベント
        /// </summary>
        public event EventHandler<ConnectionEventArgs>? ConnectionStatusChanged;

        /// <summary>
        /// エラー発生時のイベント
        /// </summary>
        public event EventHandler<PowerSupplyErrorEventArgs>? ErrorOccurred;

        /// <summary>
        /// デバイスが現在接続されているかどうかを取得します
        /// </summary>
        public bool IsConnected => _connection?.IsConnected ?? false;

        /// <summary>
        /// デバイスが初期化されているかどうかを取得します
        /// </summary>
        public bool IsInitialized => _isInitialized;

        #region 例外をスローする通常のメソッド

        /// <summary>
        /// デバイスに接続します
        /// </summary>
        /// <returns>接続に成功した場合はtrue</returns>
        /// <exception cref="CommunicationException">接続に失敗した場合</exception>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                return await _connection.ConnectAsync();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new CommunicationException("デバイスへの接続中にエラーが発生しました",
                    ErrorCode.ConnectionFailed, ex);
            }
        }

        /// <summary>
        /// デバイスから切断します
        /// </summary>
        /// <exception cref="CommunicationException">切断に失敗した場合</exception>
        public async Task DisconnectAsync()
        {
            try
            {
                if (IsConnected && _isInitialized)
                {
                    // 切断前にローカルモードに設定しておく
                    try
                    {
                        await _commandHandler.SetLocalModeAsync();
                    }
                    catch
                    {
                        // ローカルモード設定の失敗は無視
                    }
                }

                await _connection.DisconnectAsync();
                _isInitialized = false;
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new CommunicationException("デバイスからの切断中にエラーが発生しました",
                    ErrorCode.CommunicationError, ex);
            }
        }

        /// <summary>
        /// デバイスを初期化します
        /// </summary>
        /// <exception cref="InvalidOperationException">デバイスに接続されていない場合</exception>
        /// <exception cref="DeviceErrorException">初期化中にエラーが発生した場合</exception>
        public async Task InitializeAsync()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("デバイスに接続されていません。");
            }

            try
            {
                // デバイスをリセット
                await _commandHandler.ResetInstrumentAsync();

                // リモートモードに設定
                await _commandHandler.SetRemoteModeAsync();

                // デバイスID情報を取得して確認
                string idInfo = await _commandHandler.GetIdentificationAsync();
                if (string.IsNullOrEmpty(idInfo))
                {
                    throw new DeviceErrorException("デバイスからの応答がありません。",
                        ErrorCode.InvalidResponse);
                }

                _isInitialized = true;
            }
            catch (DeviceErrorException)
            {
                _isInitialized = false;
                throw;
            }
            catch (Exception ex)
            {
                _isInitialized = false;
                throw new DeviceErrorException("デバイス初期化中にエラーが発生しました",
                    ErrorCode.InitializationFailed, ex);
            }
        }

        /// <summary>
        /// 電圧値を設定します
        /// </summary>
        /// <param name="voltage">設定電圧値（単位: V）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">設定中にエラーが発生した場合</exception>
        public async Task SetVoltageAsync(double voltage, int? channel = null)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                await _commandHandler.SetVoltageAsync(voltage, channel);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("電圧設定中にエラーが発生しました",
                    ErrorCode.DeviceError, ex);
            }
        }

        /// <summary>
        /// 設定されている電圧値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定電圧値（単位: V）</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">取得中にエラーが発生した場合</exception>
        public async Task<double> GetVoltageSettingAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetVoltageSettingAsync(channel);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("電圧設定値取得中にエラーが発生しました",
                    ErrorCode.DeviceError, ex);
            }
        }

        /// <summary>
        /// 電流値を設定します
        /// </summary>
        /// <param name="current">設定電流値（単位: A）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">設定中にエラーが発生した場合</exception>
        public async Task SetCurrentAsync(double current, int? channel = null)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                await _commandHandler.SetCurrentAsync(current, channel);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("電流設定中にエラーが発生しました",
                    ErrorCode.DeviceError, ex);
            }
        }

        /// <summary>
        /// 設定されている電流値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定電流値（単位: A）</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">取得中にエラーが発生した場合</exception>
        public async Task<double> GetCurrentSettingAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetCurrentSettingAsync(channel);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("電流設定値取得中にエラーが発生しました",
                    ErrorCode.DeviceError, ex);
            }
        }

        /// <summary>
        /// 出力状態を設定します
        /// </summary>
        /// <param name="enable">出力を有効にする場合はtrue</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">設定中にエラーが発生した場合</exception>
        public async Task SetOutputStateAsync(bool enable, int? channel = null)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                await _commandHandler.SetOutputStateAsync(enable, channel);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("出力状態設定中にエラーが発生しました",
                    ErrorCode.DeviceError, ex);
            }
        }

        /// <summary>
        /// 出力状態を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>出力状態（true=ON, false=OFF）</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">取得中にエラーが発生した場合</exception>
        public async Task<bool> GetOutputStateAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetOutputStateAsync(channel);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("出力状態取得中にエラーが発生しました",
                    ErrorCode.DeviceError, ex);
            }
        }

        /// <summary>
        /// 実測電圧値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>実測電圧値（単位: V）</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">測定中にエラーが発生した場合</exception>
        public async Task<double> MeasureVoltageAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetMeasuredVoltageAsync(channel);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("電圧測定中にエラーが発生しました",
                    ErrorCode.MeasurementError, ex);
            }
        }

        /// <summary>
        /// 実測電流値を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>実測電流値（単位: A）</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">測定中にエラーが発生した場合</exception>
        public async Task<double> MeasureCurrentAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetMeasuredCurrentAsync(channel);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("電流測定中にエラーが発生しました",
                    ErrorCode.MeasurementError, ex);
            }
        }

        /// <summary>
        /// 動作モード（CC/CV）を取得します
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>動作モード（"CV"または"CC"）</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">取得中にエラーが発生した場合</exception>
        public async Task<string> GetOperationModeAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetOperationModeAsync(channel);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("動作モード取得中にエラーが発生しました",
                    ErrorCode.DeviceError, ex);
            }
        }

        /// <summary>
        /// 保護機能をリセットします
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">リセット中にエラーが発生した場合</exception>
        public async Task ResetProtectionAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                await _commandHandler.ResetProtectionAsync(channel);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("保護機能リセット中にエラーが発生しました",
                    ErrorCode.DeviceError, ex);
            }
        }

        #endregion

        #region 結果クラスを返す代替メソッド（予期される状態をエラーコードで返す）

        /// <summary>
        /// デバイスに接続します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>接続操作の結果</returns>
        public async Task<Result<bool>> TryConnectAsync()
        {
            try
            {
                bool success = await _connection.ConnectAsync();
                if (success)
                {
                    return Result<bool>.Success(true);
                }
                else
                {
                    return Result<bool>.Error(ErrorCode.ConnectionFailed, "デバイスへの接続に失敗しました");
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.ConnectionFailed, "デバイスへの接続中にエラーが発生しました");
                return Result<bool>.FromException(ex);
            }
        }

        /// <summary>
        /// デバイスから切断します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>切断操作の結果</returns>
        public async Task<Result> TryDisconnectAsync()
        {
            try
            {
                if (IsConnected && _isInitialized)
                {
                    // 切断前にローカルモードに設定しておく
                    try
                    {
                        await _commandHandler.SetLocalModeAsync();
                    }
                    catch
                    {
                        // ローカルモード設定の失敗は無視
                    }
                }

                await _connection.DisconnectAsync();
                _isInitialized = false;

                return Result.Success();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.CommunicationError, "デバイスからの切断中にエラーが発生しました");
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// デバイスを初期化します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>初期化操作の結果</returns>
        public async Task<Result> TryInitializeAsync()
        {
            if (!IsConnected)
            {
                return Result.Error(ErrorCode.NotConnected, "デバイスに接続されていません");
            }

            try
            {
                // デバイスをリセット
                await _commandHandler.ResetInstrumentAsync();

                // リモートモードに設定
                await _commandHandler.SetRemoteModeAsync();

                // デバイスID情報を取得して確認
                string idInfo = await _commandHandler.GetIdentificationAsync();
                if (string.IsNullOrEmpty(idInfo))
                {
                    _isInitialized = false;
                    return Result.Error(ErrorCode.InvalidResponse, "デバイスからの応答がありません");
                }

                _isInitialized = true;
                return Result.Success();
            }
            catch (Exception ex)
            {
                _isInitialized = false;
                OnErrorOccurred(ex, ErrorCode.InitializationFailed, "デバイス初期化中にエラーが発生しました");
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 電圧値を設定します（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="voltage">設定電圧値（単位: V）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定操作の結果</returns>
        public async Task<Result> TrySetVoltageAsync(double voltage, int? channel = null)
        {
            if (!IsInitialized)
            {
                return Result.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                await _commandHandler.SetVoltageAsync(voltage, channel);
                return Result.Success();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.DeviceError, "電圧設定中にエラーが発生しました");
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 設定されている電圧値を取得します（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>取得操作の結果と設定電圧値</returns>
        public async Task<Result<double>> TryGetVoltageSettingAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.GetVoltageSettingAsync(channel);
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.DeviceError, "電圧設定値取得中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        /// <summary>
        /// 電流値を設定します（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="current">設定電流値（単位: A）</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定操作の結果</returns>
        public async Task<Result> TrySetCurrentAsync(double current, int? channel = null)
        {
            if (!IsInitialized)
            {
                return Result.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                await _commandHandler.SetCurrentAsync(current, channel);
                return Result.Success();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.DeviceError, "電流設定中にエラーが発生しました");
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 設定されている電流値を取得します（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>取得操作の結果と設定電流値</returns>
        public async Task<Result<double>> TryGetCurrentSettingAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.GetCurrentSettingAsync(channel);
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.DeviceError, "電流設定値取得中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        /// <summary>
        /// 出力状態を設定します（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="enable">出力を有効にする場合はtrue</param>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>設定操作の結果</returns>
        public async Task<Result> TrySetOutputStateAsync(bool enable, int? channel = null)
        {
            if (!IsInitialized)
            {
                return Result.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                await _commandHandler.SetOutputStateAsync(enable, channel);
                return Result.Success();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.DeviceError, "出力状態設定中にエラーが発生しました");
                return Result.FromException(ex);
            }
        }

        /// <summary>
        /// 出力状態を取得します（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>取得操作の結果と出力状態</returns>
        public async Task<Result<bool>> TryGetOutputStateAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                return Result<bool>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                bool state = await _commandHandler.GetOutputStateAsync(channel);
                return Result<bool>.Success(state);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.DeviceError, "出力状態取得中にエラーが発生しました");
                return Result<bool>.FromException(ex);
            }
        }

        /// <summary>
        /// 実測電圧値を取得します（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>測定操作の結果と測定電圧値</returns>
        public async Task<Result<double>> TryMeasureVoltageAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.GetMeasuredVoltageAsync(channel);
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.MeasurementError, "電圧測定中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        /// <summary>
        /// 実測電流値を取得します（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>測定操作の結果と測定電流値</returns>
        public async Task<Result<double>> TryMeasureCurrentAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.GetMeasuredCurrentAsync(channel);
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.MeasurementError, "電流測定中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        /// <summary>
        /// 動作モード（CC/CV）を取得します（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>取得操作の結果と動作モード</returns>
        public async Task<Result<string>> TryGetOperationModeAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                return Result<string>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                string mode = await _commandHandler.GetOperationModeAsync(channel);
                return Result<string>.Success(mode);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.DeviceError, "動作モード取得中にエラーが発生しました");
                return Result<string>.FromException(ex);
            }
        }

        /// <summary>
        /// 保護機能をリセットします（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="channel">チャンネル番号（省略可）</param>
        /// <returns>リセット操作の結果</returns>
        public async Task<Result> TryResetProtectionAsync(int? channel = null)
        {
            if (!IsInitialized)
            {
                return Result.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                await _commandHandler.ResetProtectionAsync(channel);
                return Result.Success();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.DeviceError, "保護機能リセット中にエラーが発生しました");
                return Result.FromException(ex);
            }
        }

        #endregion

        /// <summary>
        /// デバイスのエラーメッセージを取得します
        /// </summary>
        /// <returns>エラーメッセージ（エラーがない場合は空文字）</returns>
        public async Task<string> GetErrorMessageAsync()
        {
            if (!IsConnected || !IsInitialized)
                return string.Empty;

            try
            {
                return await _commandHandler.GetErrorMessageAsync();
            }
            catch
            {
                // エラーメッセージ取得時のエラーは無視
                return string.Empty;
            }
        }

        private void OnConnectionStatusChanged(object? sender, ConnectionEventArgs e)
        {
            if (!e.IsConnected)
                _isInitialized = false;

            ConnectionStatusChanged?.Invoke(this, e);
        }

        private void OnErrorOccurred(Exception ex, ErrorCode code, string message)
        {
            ErrorOccurred?.Invoke(this, new PowerSupplyErrorEventArgs(ex, code, message));
        }
    }

    /// <summary>
    /// 電源装置エラーイベントの引数クラス
    /// </summary>
    public class PowerSupplyErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public ErrorCode ErrorCode { get; }
        public string Message { get; }

        public PowerSupplyErrorEventArgs(Exception exception, ErrorCode errorCode, string message)
        {
            Exception = exception;
            ErrorCode = errorCode;
            Message = message;
        }
    }
}


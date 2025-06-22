using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultimeterCtrl
{


    /// <summary>
    /// マルチメーターを制御するためのクラス
    /// </summary>
    public class MMCtrl
    {
        private readonly CommandHandler _commandHandler;
        private readonly IConnection _connection;
        private bool _isInitialized = false;

        /// <summary>
        /// MMCtrlを初期化します
        /// </summary>
        /// <param name="connection">使用する接続インターフェース</param>
        public MMCtrl(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _commandHandler = new CommandHandler(connection);

            // 接続状態の変更イベントをハンドリング
            _connection.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        /// <summary>
        /// デバイスの接続状態が変化したときに発生するイベント
        /// </summary>
        public event EventHandler<ConnectionEventArgs>? ConnectionStatusChanged;

        /// <summary>
        /// エラー発生時のイベント
        /// </summary>
        public event EventHandler<MultimeterErrorEventArgs>? ErrorOccurred;

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
        /// 電圧測定を実行します
        /// </summary>
        /// <returns>測定された電圧値</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">測定中にエラーが発生した場合</exception>
        public async Task<double> MeasureVoltageAsync()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetVolt();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("電圧測定中にエラーが発生しました",
                    ErrorCode.MeasurementError, ex);
            }
        }

        /// <summary>
        /// 電流測定を実行します
        /// </summary>
        /// <returns>測定された電流値</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">測定中にエラーが発生した場合</exception>
        public async Task<double> MeasureCurrentAsync()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetCurrentAsync();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("電流測定中にエラーが発生しました",
                    ErrorCode.MeasurementError, ex);
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
        /// 電圧測定を実行します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>測定操作の結果と測定値</returns>
        public async Task<Result<double>> TryMeasureVoltageAsync()
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.GetVolt();
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.MeasurementError, "電圧測定中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        /// <summary>
        /// 電流測定を実行します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>測定操作の結果と測定値</returns>
        public async Task<Result<double>> TryMeasureCurrentAsync()
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.GetCurrentAsync();
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.MeasurementError, "電流測定中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        #endregion

        #region 測定モード設定関連

        /// <summary>
        /// 現在の機能（測定モード）を設定します
        /// </summary>
        /// <param name="function">測定機能（"VOLT:DC", "VOLT:AC", "CURR:DC", "CURR:AC", "RES", "FRES", "TEMP", etc.）</param>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">設定中にエラーが発生した場合</exception>
        public async Task SetFunctionAsync(string function)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                await _commandHandler.SetFunctionAsync(function);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException($"測定モード({function})設定中にエラーが発生しました",
                    ErrorCode.ConfigurationError, ex);
            }
        }

        /// <summary>
        /// 現在の機能（測定モード）を取得します
        /// </summary>
        /// <returns>現在の測定機能</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">取得中にエラーが発生した場合</exception>
        public async Task<string> GetFunctionAsync()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetFunctionAsync();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("測定モード取得中にエラーが発生しました",
                    ErrorCode.ConfigurationError, ex);
            }
        }

        /// <summary>
        /// 電圧測定レンジを設定します（DC電圧モード用）
        /// </summary>
        /// <param name="range">レンジ値（単位: V）、AUTO の場合は null</param>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">設定中にエラーが発生した場合</exception>
        public async Task SetVoltageRangeAsync(double? range)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                await _commandHandler.SetVoltageRangeAsync(range);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("電圧測定レンジ設定中にエラーが発生しました",
                    ErrorCode.ConfigurationError, ex);
            }
        }

        /// <summary>
        /// 電流測定レンジを設定します（DC電流モード用）
        /// </summary>
        /// <param name="range">レンジ値（単位: A）、AUTO の場合は null</param>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">設定中にエラーが発生した場合</exception>
        public async Task SetCurrentRangeAsync(double? range)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                await _commandHandler.SetCurrentRangeAsync(range);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("電流測定レンジ設定中にエラーが発生しました",
                    ErrorCode.ConfigurationError, ex);
            }
        }

        /// <summary>
        /// 抵抗測定レンジを設定します
        /// </summary>
        /// <param name="range">レンジ値（単位: Ω）、AUTO の場合は null</param>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">設定中にエラーが発生した場合</exception>
        public async Task SetResistanceRangeAsync(double? range)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                await _commandHandler.SetResistanceRangeAsync(range);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("抵抗測定レンジ設定中にエラーが発生しました",
                    ErrorCode.ConfigurationError, ex);
            }
        }

        #endregion

        #region 追加測定機能

        /// <summary>
        /// AC電圧測定を実行します
        /// </summary>
        /// <returns>測定されたAC電圧値</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">測定中にエラーが発生した場合</exception>
        public async Task<double> MeasureAcVoltageAsync()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetAcVoltAsync();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("AC電圧測定中にエラーが発生しました",
                    ErrorCode.MeasurementError, ex);
            }
        }

        /// <summary>
        /// AC電流測定を実行します
        /// </summary>
        /// <returns>測定されたAC電流値</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">測定中にエラーが発生した場合</exception>
        public async Task<double> MeasureAcCurrentAsync()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetAcCurrentAsync();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("AC電流測定中にエラーが発生しました",
                    ErrorCode.MeasurementError, ex);
            }
        }

        /// <summary>
        /// 2線式抵抗測定を実行します
        /// </summary>
        /// <returns>測定された抵抗値</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">測定中にエラーが発生した場合</exception>
        public async Task<double> MeasureResistanceAsync()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetResistanceAsync();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("抵抗測定中にエラーが発生しました",
                    ErrorCode.MeasurementError, ex);
            }
        }

        /// <summary>
        /// 4線式抵抗測定を実行します
        /// </summary>
        /// <returns>測定された抵抗値</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">測定中にエラーが発生した場合</exception>
        public async Task<double> Measure4WireResistanceAsync()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.Get4WireResistanceAsync();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("4線式抵抗測定中にエラーが発生しました",
                    ErrorCode.MeasurementError, ex);
            }
        }

        /// <summary>
        /// 温度測定を実行します
        /// </summary>
        /// <returns>測定された温度値</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">測定中にエラーが発生した場合</exception>
        public async Task<double> MeasureTemperatureAsync()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetTemperatureAsync();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("温度測定中にエラーが発生しました",
                    ErrorCode.MeasurementError, ex);
            }
        }

        #endregion

        #region NPLC設定

        /// <summary>
        /// NPLCを設定します（積分時間、電源周期数）
        /// </summary>
        /// <param name="nplc">NPLC値（0.0005～15）</param>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">設定中にエラーが発生した場合</exception>
        public async Task SetNPLCAsync(double nplc)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                await _commandHandler.SetNPLCAsync(nplc);
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("NPLC設定中にエラーが発生しました",
                    ErrorCode.ConfigurationError, ex);
            }
        }

        /// <summary>
        /// 現在のNPLC値を取得します
        /// </summary>
        /// <returns>NPLC値</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">取得中にエラーが発生した場合</exception>
        public async Task<double> GetNPLCAsync()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetNPLCAsync();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("NPLC取得中にエラーが発生しました",
                    ErrorCode.ConfigurationError, ex);
            }
        }

        #endregion

        #region 例外をスローしない追加測定メソッド

        /// <summary>
        /// AC電圧測定を実行します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>測定操作の結果と測定値</returns>
        public async Task<Result<double>> TryMeasureAcVoltageAsync()
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.GetAcVoltAsync();
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.MeasurementError, "AC電圧測定中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        /// <summary>
        /// AC電流測定を実行します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>測定操作の結果と測定値</returns>
        public async Task<Result<double>> TryMeasureAcCurrentAsync()
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.GetAcCurrentAsync();
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.MeasurementError, "AC電流測定中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        /// <summary>
        /// 2線式抵抗測定を実行します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>測定操作の結果と測定値</returns>
        public async Task<Result<double>> TryMeasureResistanceAsync()
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.GetResistanceAsync();
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.MeasurementError, "抵抗測定中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        /// <summary>
        /// 4線式抵抗測定を実行します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>測定操作の結果と測定値</returns>
        public async Task<Result<double>> TryMeasure4WireResistanceAsync()
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.Get4WireResistanceAsync();
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.MeasurementError, "4線式抵抗測定中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        /// <summary>
        /// 温度測定を実行します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>測定操作の結果と測定値</returns>
        public async Task<Result<double>> TryMeasureTemperatureAsync()
        {
            if (!IsInitialized)
            {
                return Result<double>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                double value = await _commandHandler.GetTemperatureAsync();
                return Result<double>.Success(value);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.MeasurementError, "温度測定中にエラーが発生しました");
                return Result<double>.FromException(ex);
            }
        }

        /// <summary>
        /// 測定機能を設定します（例外をスローしない代替バージョン）
        /// </summary>
        /// <param name="function">測定機能（"VOLT:DC", "VOLT:AC", "CURR:DC", etc.）</param>
        /// <returns>設定操作の結果</returns>
        public async Task<Result> TrySetFunctionAsync(string function)
        {
            if (!IsInitialized)
            {
                return Result.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                await _commandHandler.SetFunctionAsync(function);
                return Result.Success();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.ConfigurationError, $"測定モード({function})設定中にエラーが発生しました");
                return Result.FromException(ex);
            }
        }

        #endregion

        #region システム時間取得

        /// <summary>
        /// 現在のデバイス日時を取得します
        /// </summary>
        /// <returns>デバイスの現在時刻</returns>
        /// <exception cref="InvalidOperationException">デバイスが初期化されていない場合</exception>
        /// <exception cref="DeviceErrorException">取得中にエラーが発生した場合</exception>
        public async Task<DateTime> GetSystemTimeAsync()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("デバイスが初期化されていません。先に初期化を行ってください。");
            }

            try
            {
                return await _commandHandler.GetSystemTimeAsync();
            }
            catch (Exception ex) when (!(ex is DeviceErrorException))
            {
                throw new DeviceErrorException("システム時間取得中にエラーが発生しました",
                    ErrorCode.CommunicationError, ex);
            }
        }

        /// <summary>
        /// 現在のデバイス日時を取得します（例外をスローしない代替バージョン）
        /// </summary>
        /// <returns>取得操作の結果とデバイスの現在時刻</returns>
        public async Task<Result<DateTime>> TryGetSystemTimeAsync()
        {
            if (!IsInitialized)
            {
                return Result<DateTime>.Error(ErrorCode.NotInitialized,
                    "デバイスが初期化されていません。先に初期化を行ってください");
            }

            try
            {
                DateTime time = await _commandHandler.GetSystemTimeAsync();
                return Result<DateTime>.Success(time);
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex, ErrorCode.CommunicationError, "システム時間取得中にエラーが発生しました");
                return Result<DateTime>.FromException(ex);
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
            ErrorOccurred?.Invoke(this, new MultimeterErrorEventArgs(ex, code, message));
        }
    }

    /// <summary>
    /// マルチメーターエラーイベントの引数クラス
    /// </summary>
    public class MultimeterErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public ErrorCode ErrorCode { get; }
        public string Message { get; }

        public MultimeterErrorEventArgs(Exception exception, ErrorCode errorCode, string message)
        {
            Exception = exception;
            ErrorCode = errorCode;
            Message = message;
        }
    }
}

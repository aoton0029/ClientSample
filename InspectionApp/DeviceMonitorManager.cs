using InspectionApp.DataProcessors;
using InspectionApp.Multimeters;
using InspectionApp.PowerSupplys;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionApp
{
    /// <summary>
    /// 複数のデバイス（電源装置、マルチメーター）の測定値とステータスをモニタリングする
    /// </summary>
    public class DeviceMonitoringManager : IDisposable
    {
        /// <summary>
        /// 登録された電源装置
        /// </summary>
        private readonly ConcurrentDictionary<string, DeviceMeasurementInfo<IPowerSupply>> _powerSupplies = new();

        /// <summary>
        /// 登録されたマルチメーター
        /// </summary>
        private readonly ConcurrentDictionary<string, DeviceMeasurementInfo<IMultimeter>> _multimeters = new();

        /// <summary>
        /// ステータス更新用タイマー
        /// </summary>
        private System.Threading.Timer? _statusUpdateTimer;

        /// <summary>
        /// データ処理用プロセッサ
        /// </summary>
        private readonly IDataProcessor _dataProcessor;

        /// <summary>
        /// デバイスステータス更新イベント
        /// </summary>
        public event EventHandler<DeviceStatusUpdatedEventArgs>? DeviceStatusUpdated;

        /// <summary>
        /// 測定エラーイベント
        /// </summary>
        public event EventHandler<DeviceMeasurementErrorEventArgs>? MeasurementError;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataProcessor">データ処理用プロセッサ</param>
        public DeviceMonitoringManager(IDataProcessor dataProcessor)
        {
            _dataProcessor = dataProcessor ?? throw new ArgumentNullException(nameof(dataProcessor));
        }

        /// <summary>
        /// モニタリングを開始する
        /// </summary>
        /// <param name="statusUpdateInterval">ステータス更新間隔（ミリ秒）</param>
        public void StartMonitoring(int statusUpdateInterval = 1000)
        {
            // タイマーが既に動作している場合は停止
            _statusUpdateTimer?.Dispose();

            // 新しいタイマーを作成して開始
            _statusUpdateTimer = new System.Threading.Timer(UpdateAllDeviceStatus, null, 0, statusUpdateInterval);
        }

        /// <summary>
        /// モニタリングを停止する
        /// </summary>
        public void StopMonitoring()
        {
            _statusUpdateTimer?.Dispose();
            _statusUpdateTimer = null;
        }

        /// <summary>
        /// 電源装置を登録する
        /// </summary>
        /// <param name="deviceId">デバイスID（ユニーク識別子）</param>
        /// <param name="powerSupply">電源装置</param>
        /// <param name="displayName">表示名</param>
        /// <param name="settings">測定設定（電圧、電流など）</param>
        /// <returns>登録が成功したかどうか</returns>
        public bool RegisterPowerSupply(string deviceId, IPowerSupply powerSupply, string displayName, IEnumerable<MeasurementSettings>? settings = null)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentException("デバイスIDは空にできません", nameof(deviceId));

            if (powerSupply == null)
                throw new ArgumentNullException(nameof(powerSupply));

            // 測定情報を作成
            var measurementInfo = new DeviceMeasurementInfo<IPowerSupply>(deviceId, powerSupply, displayName, DeviceType.PowerSupply);

            // 測定ランナーを作成
            var runner = new PowerSupplyMeasurementRunner(powerSupply, _dataProcessor);
            runner.MeasurementError += (sender, ex) => OnDeviceMeasurementError(deviceId, DeviceType.PowerSupply, ex);

            measurementInfo.MeasurementRunner = runner;

            // 測定設定を追加
            if (settings != null)
            {
                foreach (var setting in settings)
                {
                    runner.StartMeasurement(setting);
                }
            }

            // 電源装置コレクションに追加
            return _powerSupplies.TryAdd(deviceId, measurementInfo);
        }

        /// <summary>
        /// マルチメーターを登録する
        /// </summary>
        /// <param name="deviceId">デバイスID（ユニーク識別子）</param>
        /// <param name="multimeter">マルチメーター</param>
        /// <param name="displayName">表示名</param>
        /// <param name="settings">測定設定</param>
        /// <returns>登録が成功したかどうか</returns>
        public bool RegisterMultimeter(string deviceId, IMultimeter multimeter, string displayName, IEnumerable<MeasurementSettings>? settings = null)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentException("デバイスIDは空にできません", nameof(deviceId));

            if (multimeter == null)
                throw new ArgumentNullException(nameof(multimeter));

            // 測定情報を作成
            var measurementInfo = new DeviceMeasurementInfo<IMultimeter>(deviceId, multimeter, displayName, DeviceType.Multimeter);

            // 測定ランナーを作成
            var runner = new MultimeterMeasurementRunner(multimeter, _dataProcessor);
            runner.MeasurementError += (sender, ex) => OnDeviceMeasurementError(deviceId, DeviceType.Multimeter, ex);

            measurementInfo.MeasurementRunner = runner;

            // 測定設定を追加
            if (settings != null)
            {
                foreach (var setting in settings)
                {
                    runner.StartMeasurement(setting);
                }
            }

            // マルチメーターコレクションに追加
            return _multimeters.TryAdd(deviceId, measurementInfo);
        }

        /// <summary>
        /// 電源装置の測定設定を追加または更新する
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        /// <param name="settings">測定設定</param>
        /// <returns>設定の追加または更新が成功したかどうか</returns>
        public bool UpdatePowerSupplyMeasurementSettings(string deviceId, MeasurementSettings settings)
        {
            if (_powerSupplies.TryGetValue(deviceId, out var info) && info.MeasurementRunner is PowerSupplyMeasurementRunner runner)
            {
                return runner.StartMeasurement(settings);
            }
            return false;
        }

        /// <summary>
        /// マルチメーターの測定設定を追加または更新する
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        /// <param name="settings">測定設定</param>
        /// <returns>設定の追加または更新が成功したかどうか</returns>
        public bool UpdateMultimeterMeasurementSettings(string deviceId, MeasurementSettings settings)
        {
            if (_multimeters.TryGetValue(deviceId, out var info) && info.MeasurementRunner is MultimeterMeasurementRunner runner)
            {
                return runner.StartMeasurement(settings);
            }
            return false;
        }

        /// <summary>
        /// 電源装置の測定を停止する
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        /// <param name="measurementKey">測定キー</param>
        public void StopPowerSupplyMeasurement(string deviceId, int measurementKey)
        {
            if (_powerSupplies.TryGetValue(deviceId, out var info) && info.MeasurementRunner is PowerSupplyMeasurementRunner runner)
            {
                runner.StopMeasurement(measurementKey);
            }
        }

        /// <summary>
        /// マルチメーターの測定を停止する
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        /// <param name="measurementKey">測定キー</param>
        public void StopMultimeterMeasurement(string deviceId, int measurementKey)
        {
            if (_multimeters.TryGetValue(deviceId, out var info) && info.MeasurementRunner is MultimeterMeasurementRunner runner)
            {
                runner.StopMeasurement(measurementKey);
            }
        }

        /// <summary>
        /// デバイスの登録を解除する
        /// </summary>
        /// <param name="deviceId">デバイスID</param>
        /// <param name="deviceType">デバイスタイプ</param>
        /// <returns>登録解除が成功したかどうか</returns>
        public bool UnregisterDevice(string deviceId, DeviceType deviceType)
        {
            bool result = false;

            switch (deviceType)
            {
                case DeviceType.PowerSupply:
                    if (_powerSupplies.TryRemove(deviceId, out var psInfo))
                    {
                        if (psInfo.MeasurementRunner is PowerSupplyMeasurementRunner psRunner)
                        {
                            psRunner.StopAllMeasurements();
                            psRunner.Dispose();
                        }
                        result = true;
                    }
                    break;

                case DeviceType.Multimeter:
                    if (_multimeters.TryRemove(deviceId, out var mmInfo))
                    {
                        if (mmInfo.MeasurementRunner is MultimeterMeasurementRunner mmRunner)
                        {
                            mmRunner.StopAllMeasurements();
                            mmRunner.Dispose();
                        }
                        result = true;
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// すべてのデバイスのステータスを更新する
        /// </summary>
        private async void UpdateAllDeviceStatus(object? state)
        {
            try
            {
                // 電源装置のステータス更新
                foreach (var pair in _powerSupplies)
                {
                    await UpdatePowerSupplyStatus(pair.Key, pair.Value);
                }

                // マルチメーターのステータス更新
                foreach (var pair in _multimeters)
                {
                    await UpdateMultimeterStatus(pair.Key, pair.Value);
                }
            }
            catch (Exception ex)
            {
                // モニタリング中のグローバルエラーハンドリング
                Console.WriteLine($"デバイスステータス更新中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 電源装置のステータスを更新する
        /// </summary>
        private async Task UpdatePowerSupplyStatus(string deviceId, DeviceMeasurementInfo<IPowerSupply> info)
        {
            var powerSupply = info.Device;

            if (!powerSupply.IsConnected)
            {
                RaiseDeviceStatusUpdatedEvent(new DeviceStatusUpdatedEventArgs(
                    deviceId,
                    info.DisplayName,
                    DeviceType.PowerSupply,
                    isConnected: false,
                    new Dictionary<string, object>()));
                return;
            }

            try
            {
                var statusValues = new Dictionary<string, object>();

                // 出力状態を取得
                var isOutputOn = await powerSupply.GetOutputStateAsync();
                statusValues["OutputState"] = isOutputOn;

                // 電圧設定値を取得
                var voltageSettingValue = await powerSupply.GetVoltageSettingAsync();
                statusValues["VoltageSetting"] = voltageSettingValue;

                // 電流設定値を取得
                var currentSettingValue = await powerSupply.GetCurrentSettingAsync();
                statusValues["CurrentSetting"] = currentSettingValue;

                // 実測値（チャンネル1のデフォルト値）
                var measuredVoltage = await powerSupply.MeasureVoltageAsync();
                statusValues["MeasuredVoltage"] = measuredVoltage;

                var measuredCurrent = await powerSupply.MeasureCurrentAsync();
                statusValues["MeasuredCurrent"] = measuredCurrent;

                // 電力計算（W）
                statusValues["Power"] = measuredVoltage * measuredCurrent;

                // ステータス更新イベントを発行
                RaiseDeviceStatusUpdatedEvent(new DeviceStatusUpdatedEventArgs(
                    deviceId,
                    info.DisplayName,
                    DeviceType.PowerSupply,
                    isConnected: true,
                    statusValues));
            }
            catch (Exception ex)
            {
                // エラー情報を含めてステータス更新
                var statusValues = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message
                };

                RaiseDeviceStatusUpdatedEvent(new DeviceStatusUpdatedEventArgs(
                    deviceId,
                    info.DisplayName,
                    DeviceType.PowerSupply,
                    isConnected: true,
                    statusValues));

                // エラーイベントも発行
                OnDeviceMeasurementError(deviceId, DeviceType.PowerSupply, ex);
            }
        }

        /// <summary>
        /// マルチメーターのステータスを更新する
        /// </summary>
        private async Task UpdateMultimeterStatus(string deviceId, DeviceMeasurementInfo<IMultimeter> info)
        {
            var multimeter = info.Device;

            if (!multimeter.IsConnected)
            {
                RaiseDeviceStatusUpdatedEvent(new DeviceStatusUpdatedEventArgs(
                    deviceId,
                    info.DisplayName,
                    DeviceType.Multimeter,
                    isConnected: false,
                    new Dictionary<string, object>()));
                return;
            }

            try
            {
                var statusValues = new Dictionary<string, object>();

                // 現在の測定機能を取得
                var currentFunction = await multimeter.GetFunctionAsync();
                statusValues["Function"] = currentFunction;

                // 機能に応じた測定
                if (currentFunction.Contains("VOLT", StringComparison.OrdinalIgnoreCase))
                {
                    var voltage = await multimeter.MeasureVoltageAsync();
                    statusValues["MeasuredValue"] = voltage;
                    statusValues["Unit"] = "V";
                }
                else if (currentFunction.Contains("CURR", StringComparison.OrdinalIgnoreCase))
                {
                    var current = await multimeter.MeasureCurrentAsync();
                    statusValues["MeasuredValue"] = current;
                    statusValues["Unit"] = "A";
                }
                else
                {
                    // その他の機能は一般的な測定値として処理
                    var value = await multimeter.MeasureVoltageAsync();
                    statusValues["MeasuredValue"] = value;
                    statusValues["Unit"] = "?";
                }

                // ステータス更新イベントを発行
                RaiseDeviceStatusUpdatedEvent(new DeviceStatusUpdatedEventArgs(
                    deviceId,
                    info.DisplayName,
                    DeviceType.Multimeter,
                    isConnected: true,
                    statusValues));
            }
            catch (Exception ex)
            {
                // エラー情報を含めてステータス更新
                var statusValues = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message
                };

                RaiseDeviceStatusUpdatedEvent(new DeviceStatusUpdatedEventArgs(
                    deviceId,
                    info.DisplayName,
                    DeviceType.Multimeter,
                    isConnected: true,
                    statusValues));

                // エラーイベントも発行
                OnDeviceMeasurementError(deviceId, DeviceType.Multimeter, ex);
            }
        }

        /// <summary>
        /// デバイスステータス更新イベントを発行する
        /// </summary>
        private void RaiseDeviceStatusUpdatedEvent(DeviceStatusUpdatedEventArgs args)
        {
            DeviceStatusUpdated?.Invoke(this, args);
        }

        /// <summary>
        /// デバイス測定エラーイベントを発行する
        /// </summary>
        private void OnDeviceMeasurementError(string deviceId, DeviceType deviceType, Exception exception)
        {
            MeasurementError?.Invoke(this, new DeviceMeasurementErrorEventArgs(deviceId, deviceType, exception));
        }

        /// <summary>
        /// リソースを破棄する
        /// </summary>
        public void Dispose()
        {
            // タイマーを停止して破棄
            _statusUpdateTimer?.Dispose();

            // 全デバイスランナーを停止して破棄
            foreach (var pair in _powerSupplies)
            {
                if (pair.Value.MeasurementRunner is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            foreach (var pair in _multimeters)
            {
                if (pair.Value.MeasurementRunner is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // コレクションをクリア
            _powerSupplies.Clear();
            _multimeters.Clear();

            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// デバイスタイプ
    /// </summary>
    public enum DeviceType
    {
        PowerSupply,
        Multimeter
    }

    /// <summary>
    /// デバイスステータス更新イベント引数
    /// </summary>
    public class DeviceStatusUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// デバイスID
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// デバイスタイプ
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        /// 接続状態
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// ステータス値の辞書
        /// </summary>
        public IReadOnlyDictionary<string, object> StatusValues { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DeviceStatusUpdatedEventArgs(
            string deviceId,
            string displayName,
            DeviceType deviceType,
            bool isConnected,
            IReadOnlyDictionary<string, object> statusValues)
        {
            DeviceId = deviceId;
            DisplayName = displayName;
            DeviceType = deviceType;
            IsConnected = isConnected;
            StatusValues = statusValues;
        }
    }

    /// <summary>
    /// デバイス測定エラーイベント引数
    /// </summary>
    public class DeviceMeasurementErrorEventArgs : EventArgs
    {
        /// <summary>
        /// デバイスID
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// デバイスタイプ
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        /// 発生したエラー
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DeviceMeasurementErrorEventArgs(string deviceId, DeviceType deviceType, Exception error)
        {
            DeviceId = deviceId;
            DeviceType = deviceType;
            Error = error;
        }
    }

    /// <summary>
    /// デバイス測定情報
    /// </summary>
    internal class DeviceMeasurementInfo<T>
    {
        /// <summary>
        /// デバイスID
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// デバイス本体
        /// </summary>
        public T Device { get; }

        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// デバイスタイプ
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        /// 測定ランナー
        /// </summary>
        public object? MeasurementRunner { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DeviceMeasurementInfo(string deviceId, T device, string displayName, DeviceType deviceType)
        {
            DeviceId = deviceId;
            Device = device;
            DisplayName = displayName;
            DeviceType = deviceType;
        }
    }
}

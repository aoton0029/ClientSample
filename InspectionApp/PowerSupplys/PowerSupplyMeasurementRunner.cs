using InspectionApp.DataProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionApp.PowerSupplys
{


    /// <summary>
    /// IPowerSupplyを使用して定期的に測定を行うクラス
    /// </summary>
    public class PowerSupplyMeasurementRunner : IDisposable
    {
        private readonly IPowerSupply _powerSupply;
        private readonly IDataProcessor _dataProcessor;
        private CancellationTokenSource? _cts;
        private Task? _measurementTask;
        private bool _disposed;
        private readonly List<MeasurementSettings> _activeSettings = new();
        private readonly object _lockObject = new();

        /// <summary>
        /// 測定中かどうかを示す値を取得します
        /// </summary>
        public bool IsMeasuring { get; private set; }

        /// <summary>
        /// 測定エラーが発生した時に発生するイベント
        /// </summary>
        public event EventHandler<Exception>? MeasurementError;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="powerSupply">電源装置</param>
        /// <param name="dataProcessor">データプロセッサ</param>
        public PowerSupplyMeasurementRunner(IPowerSupply powerSupply, IDataProcessor dataProcessor)
        {
            _powerSupply = powerSupply ?? throw new ArgumentNullException(nameof(powerSupply));
            _dataProcessor = dataProcessor ?? throw new ArgumentNullException(nameof(dataProcessor));

            _powerSupply.ErrorOccurred += (sender, args) =>
                MeasurementError?.Invoke(this, args.Exception ?? new Exception(args.Message));
        }

        /// <summary>
        /// 定期測定を開始します
        /// </summary>
        /// <param name="settings">測定設定</param>
        /// <returns>測定開始の成功状態</returns>
        public bool StartMeasurement(MeasurementSettings settings)
        {
            if (!_powerSupply.IsConnected)
            {
                return false;
            }

            lock (_lockObject)
            {
                // 既存の測定をチェック
                var existingSetting = _activeSettings.Find(s => s.MeasurementKey == settings.MeasurementKey);
                if (existingSetting != null)
                {
                    _activeSettings.Remove(existingSetting);
                }

                _activeSettings.Add(settings);

                // まだ測定が実行されていない場合は開始
                if (_measurementTask == null)
                {
                    StartMeasurementTask();
                }

                return true;
            }
        }

        /// <summary>
        /// 指定されたキーの測定を停止します
        /// </summary>
        /// <param name="measurementKey">停止する測定のキー</param>
        public void StopMeasurement(int measurementKey)
        {
            lock (_lockObject)
            {
                var setting = _activeSettings.Find(s => s.MeasurementKey == measurementKey);
                if (setting != null)
                {
                    _activeSettings.Remove(setting);
                }

                // 測定設定が空になったら測定タスクを停止
                if (_activeSettings.Count == 0)
                {
                    StopAllMeasurements();
                }
            }
        }

        /// <summary>
        /// すべての測定を停止します
        /// </summary>
        public void StopAllMeasurements()
        {
            lock (_lockObject)
            {
                _activeSettings.Clear();

                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts = null;
                }

                _measurementTask = null;
                IsMeasuring = false;
            }
        }

        /// <summary>
        /// 測定タスクを開始します
        /// </summary>
        private void StartMeasurementTask()
        {
            _cts = new CancellationTokenSource();

            _measurementTask = Task.Run(async () =>
            {
                IsMeasuring = true;
                try
                {
                    // 最後の測定実行時間を格納するディクショナリ
                    var lastMeasurementTimes = new Dictionary<int, DateTime>();

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        // 各測定設定に対して処理
                        foreach (var setting in _activeSettings.ToArray()) // ToArray()でロックなしでコピーを処理
                        {
                            try
                            {
                                // 測定間隔のチェック
                                if (!lastMeasurementTimes.TryGetValue(setting.MeasurementKey, out var lastTime) ||
                                    DateTime.UtcNow - lastTime >= setting.Interval)
                                {
                                    await PerformMeasurementAsync(setting);
                                    lastMeasurementTimes[setting.MeasurementKey] = DateTime.UtcNow;
                                }
                            }
                            catch (Exception ex)
                            {
                                OnMeasurementError(ex);
                            }
                        }

                        // スレッド負荷を減らすため、短い間隔で待機
                        await Task.Delay(100, _cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // キャンセル例外は正常終了と見なす
                }
                catch (Exception ex)
                {
                    OnMeasurementError(ex);
                }
                finally
                {
                    IsMeasuring = false;
                }
            }, _cts.Token);
        }

        /// <summary>
        /// 設定に基づいて測定を実行します
        /// </summary>
        /// <param name="settings">測定設定</param>
        private async Task PerformMeasurementAsync(MeasurementSettings settings)
        {
            double measuredValue;
            string source = settings.SourceIdentifier;

            switch (settings.MeasurementType)
            {
                case MeasurementType.Voltage:
                    measuredValue = await _powerSupply.MeasureVoltageAsync(settings.PowerSupplyChannel);
                    if (string.IsNullOrEmpty(source))
                        source = $"PowerSupply_Voltage_CH{settings.PowerSupplyChannel ?? 1}";
                    break;

                case MeasurementType.Current:
                    measuredValue = await _powerSupply.MeasureCurrentAsync(settings.PowerSupplyChannel);
                    if (string.IsNullOrEmpty(source))
                        source = $"PowerSupply_Current_CH{settings.PowerSupplyChannel ?? 1}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // 測定データをプロセッサに渡す
            var data = new MeasurementDataWithKey
            {
                Key = settings.MeasurementKey,
                Value = measuredValue,
                Timestamp = DateTime.UtcNow,
                Source = source
            };

            await _dataProcessor.ScheduleDataProcessing(data);
        }

        /// <summary>
        /// 測定エラーを処理します
        /// </summary>
        /// <param name="ex">発生した例外</param>
        private void OnMeasurementError(Exception ex)
        {
            MeasurementError?.Invoke(this, ex);
        }

        /// <summary>
        /// リソースを破棄します
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを破棄します
        /// </summary>
        /// <param name="disposing">マネージドリソースを破棄するかどうか</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StopAllMeasurements();
                    _cts?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}

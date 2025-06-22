using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace InspectionApp.DataProcessors
{
    /// <summary>
    /// 複数のキー固有プロセッサーを管理するバックグラウンドデータプロセッサー
    /// </summary>
    public class BackgroundDataProcessor : IDataProcessor
    {
        /// <summary>
        /// 内部処理キュー
        /// </summary>
        private readonly Channel<MeasurementDataWithKey> _internalQueue;

        /// <summary>
        /// キー別データプロセッサーのコレクション
        /// </summary>
        private readonly Dictionary<int, KeySpecificDataProcessor> _dataProcessors;

        /// <summary>
        /// プロセッサー作成時の同時アクセス制御用ロック
        /// </summary>
        private readonly SemaphoreSlim _processorsLock;

        /// <summary>
        /// 処理モニター
        /// </summary>
        private BackgroundDataProcessorMonitor? _monitor;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BackgroundDataProcessor()
        {
            _internalQueue = Channel.CreateUnbounded<MeasurementDataWithKey>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            _dataProcessors = new Dictionary<int, KeySpecificDataProcessor>();
            _processorsLock = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// バックグラウンド処理を実行する
        /// </summary>
        /// <param name="stoppingToken">キャンセルトークン</param>
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _monitor?.OnProcessingStarted();

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    MeasurementDataWithKey? data = null;

                    try
                    {
                        // キューからデータを読み取る
                        data = await _internalQueue.Reader.ReadAsync(stoppingToken);
                        _monitor?.OnDataReceived(data);

                        // 対応するプロセッサーを取得または作成
                        var processor = GetOrCreateDataProcessor(data, stoppingToken);

                        // データ処理をスケジュール
                        await processor.ScheduleDataProcessing(data);
                        _monitor?.OnDataProcessed(data);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _monitor?.OnError(ex, data);
                        Console.WriteLine($"バックグラウンド処理中にエラーが発生しました: {ex.Message}");
                    }
                }
            }
            finally
            {
                // すべてのプロセッサーを停止
                await _processorsLock.WaitAsync();
                try
                {
                    var stopTasks = _dataProcessors.Values.Select(p => p.StopProcessing());
                    await Task.WhenAll(stopTasks);
                    _dataProcessors.Clear();
                }
                finally
                {
                    _processorsLock.Release();
                }

                _monitor?.OnProcessingStopped();
            }
        }

        /// <summary>
        /// データのキーに対応するプロセッサーを取得または作成する
        /// </summary>
        /// <param name="dataWithKey">処理対象データ</param>
        /// <param name="newProcessorCancellationToken">新規プロセッサー用キャンセルトークン</param>
        /// <returns>キー固有のデータプロセッサー</returns>
        public KeySpecificDataProcessor GetOrCreateDataProcessor(MeasurementDataWithKey dataWithKey,
            CancellationToken newProcessorCancellationToken = default)
        {
            // ロックなしで既存プロセッサーの確認を試みる
            if (_dataProcessors.TryGetValue(dataWithKey.Key, out var existingProcessor))
            {
                return existingProcessor;
            }

            // 存在しない場合、ロックを取得して再確認
            _processorsLock.Wait();
            try
            {
                if (_dataProcessors.TryGetValue(dataWithKey.Key, out existingProcessor))
                {
                    return existingProcessor;
                }

                // 新しいプロセッサーを作成
                var newProcessor = CreateNewProcessor(dataWithKey.Key, newProcessorCancellationToken);
                _dataProcessors.Add(dataWithKey.Key, newProcessor);
                _monitor?.OnProcessorCreated(newProcessor);

                return newProcessor;
            }
            finally
            {
                _processorsLock.Release();
            }
        }

        /// <summary>
        /// 新しいキー固有プロセッサーを作成する
        /// </summary>
        /// <param name="dataKey">データキー</param>
        /// <param name="processorCancellationToken">キャンセルトークン</param>
        /// <returns>作成されたプロセッサー</returns>
        protected virtual KeySpecificDataProcessor CreateNewProcessor(int dataKey,
            CancellationToken processorCancellationToken = default)
        {
            var processor = new KeySpecificDataProcessor(dataKey);
            processor.StartProcessing();
            return processor;
        }

        /// <summary>
        /// データ処理をスケジュールする
        /// </summary>
        /// <param name="dataWithKey">処理対象のデータ</param>
        public async Task ScheduleDataProcessing(MeasurementDataWithKey dataWithKey)
        {
            await _internalQueue.Writer.WriteAsync(dataWithKey);
        }

        /// <summary>
        /// 処理モニターを設定する
        /// </summary>
        /// <param name="monitor">モニター</param>
        public void SetMonitor(BackgroundDataProcessorMonitor monitor)
        {
            _monitor = monitor;
        }
    }

    /// <summary>
    /// バックグラウンドデータプロセッサーのモニタリングクラス
    /// </summary>
    public class BackgroundDataProcessorMonitor
    {
        /// <summary>
        /// プロセッサー管理用の同時アクセス制御用ロック
        /// </summary>
        private readonly SemaphoreSlim _processorsLock = new(1, 1);

        /// <summary>
        /// モニタリングタスク情報
        /// </summary>
        private MonitoringTask? _monitoringTask;

        /// <summary>
        /// プロセッサーの期限切れしきい値
        /// </summary>
        private readonly TimeSpan _processorExpiryThreshold;

        /// <summary>
        /// プロセッサーの期限切れスキャン周期
        /// </summary>
        private readonly TimeSpan _processorExpiryScanningPeriod;

        /// <summary>
        /// キー別データプロセッサーのコレクション
        /// </summary>
        private readonly Dictionary<int, KeySpecificDataProcessor> _dataProcessors;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataProcessors">監視対象のデータプロセッサーコレクション</param>
        /// <param name="processorExpiryThreshold">プロセッサーの期限切れしきい値（デフォルト: 10分）</param>
        /// <param name="processorExpiryScanningPeriod">プロセッサーの期限切れスキャン周期（デフォルト: 1分）</param>
        public BackgroundDataProcessorMonitor(
            Dictionary<int, KeySpecificDataProcessor> dataProcessors,
            TimeSpan? processorExpiryThreshold = null,
            TimeSpan? processorExpiryScanningPeriod = null)
        {
            _dataProcessors = dataProcessors ?? throw new ArgumentNullException(nameof(dataProcessors));
            _processorExpiryThreshold = processorExpiryThreshold ?? TimeSpan.FromMinutes(10);
            _processorExpiryScanningPeriod = processorExpiryScanningPeriod ?? TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// モニタリング情報を保持する構造体
        /// </summary>
        /// <param name="Task">モニタリングタスク</param>
        /// <param name="CancellationTokenSource">キャンセルトークンソース</param>
        public record struct MonitoringTask(Task Task, CancellationTokenSource CancellationTokenSource);

        /// <summary>
        /// モニタリングを開始する
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン</param>
        public void StartMonitoring(CancellationToken cancellationToken = default)
        {
            if (_monitoringTask != null)
            {
                return; // 既にモニタリング中
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(_processorExpiryScanningPeriod, cts.Token);
                    await ScanForExpiredProcessors(cts.Token);
                }
            }, cts.Token);

            _monitoringTask = new MonitoringTask(task, cts);
        }

        /// <summary>
        /// プロセッサーが期限切れかどうかをチェックする
        /// </summary>
        /// <param name="processorInfo">チェック対象のプロセッサー</param>
        /// <returns>期限切れの場合はtrue、それ以外はfalse</returns>
        public bool IsExpired(KeySpecificDataProcessor processorInfo)
        {
            // LastProcessingTimestampがデフォルト値の場合、まだ処理されていないため期限切れとしない
            if (processorInfo.LastProcessingTimestamp == default)
            {
                return false;
            }

            // 最後の処理から一定時間が経過したものを期限切れとする
            var timeSinceLastProcessing = DateTime.UtcNow - processorInfo.LastProcessingTimestamp;
            return timeSinceLastProcessing > _processorExpiryThreshold;
        }

        /// <summary>
        /// 期限切れプロセッサーをスキャンして処理する
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン</param>
        private async Task ScanForExpiredProcessors(CancellationToken cancellationToken)
        {
            // ロックを取得
            await _processorsLock.WaitAsync(cancellationToken);

            try
            {
                // 期限切れプロセッサーのキーを特定
                var expiredKeys = _dataProcessors
                    .Where(kv => IsExpired(kv.Value))
                    .Select(kv => kv.Key)
                    .ToList();

                // 期限切れプロセッサーを停止して削除
                foreach (var key in expiredKeys)
                {
                    if (_dataProcessors.TryGetValue(key, out var processor))
                    {
                        await processor.StopProcessing();
                        _dataProcessors.Remove(key);
                        OnProcessorExpired(processor);
                    }
                }
            }
            finally
            {
                _processorsLock.Release();
            }
        }

        /// <summary>
        /// モニタリングを停止する
        /// </summary>
        public async Task StopMonitoring()
        {
            if (_monitoringTask == null)
            {
                return;
            }

            // モニタリングタスクをキャンセル
            _monitoringTask.Value.CancellationTokenSource.Cancel();

            try
            {
                // タスクの完了を待機
                await _monitoringTask.Value.Task;
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合は例外を無視
            }
            catch (Exception ex)
            {
                // その他の例外は記録
                OnError(ex, "モニタリング停止中にエラーが発生しました");
            }
            finally
            {
                _monitoringTask.Value.CancellationTokenSource.Dispose();
                _monitoringTask = null;
            }
        }

        /// <summary>
        /// 処理開始時に呼び出される
        /// </summary>
        public virtual void OnProcessingStarted() { }

        /// <summary>
        /// 処理停止時に呼び出される
        /// </summary>
        public virtual void OnProcessingStopped() { }

        /// <summary>
        /// データ受信時に呼び出される
        /// </summary>
        public virtual void OnDataReceived(MeasurementDataWithKey data) { }

        /// <summary>
        /// データ処理完了時に呼び出される
        /// </summary>
        public virtual void OnDataProcessed(MeasurementDataWithKey data) { }

        /// <summary>
        /// 新しいプロセッサー作成時に呼び出される
        /// </summary>
        public virtual void OnProcessorCreated(KeySpecificDataProcessor processor) { }

        /// <summary>
        /// プロセッサーが期限切れになった時に呼び出される
        /// </summary>
        public virtual void OnProcessorExpired(KeySpecificDataProcessor processor) { }

        /// <summary>
        /// エラー発生時に呼び出される
        /// </summary>
        public virtual void OnError(Exception exception, string? message = null) { }

        /// <summary>
        /// エラー発生時に呼び出される（データ関連）
        /// </summary>
        public virtual void OnError(Exception exception, MeasurementDataWithKey? data) { }
    }
}

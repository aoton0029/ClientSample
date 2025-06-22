using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace InspectionApp.DataProcessors
{
    /// <summary>
    /// データ処理インターフェース
    /// </summary>
    public interface IDataProcessor
    {
        /// <summary>
        /// 測定データの処理をスケジュールする
        /// </summary>
        /// <param name="dataWithKey">処理対象のデータ</param>
        /// <returns>処理タスク</returns>
        Task ScheduleDataProcessing(MeasurementDataWithKey dataWithKey);
    }

    /// <summary>
    /// 特定のキーに関連するデータを処理するクラス
    /// </summary>
    public class KeySpecificDataProcessor : IDataProcessor
    {
        /// <summary>
        /// このプロセッサーのキー
        /// </summary>
        public int ProcessorKey { get; }

        /// <summary>
        /// 内部処理キュー
        /// </summary>
        private readonly Channel<MeasurementDataWithKey> _internalQueue = Channel.CreateUnbounded<MeasurementDataWithKey>();

        /// <summary>
        /// キャンセルトークンソース
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 処理タスク
        /// </summary>
        private Task _processingTask;

        public DateTime LastProcessingTimestamp => _processingFinishedTimestamp ?? DateTime.UtcNow;

        private DateTime? _processingFinishedTimestamp = DateTime.UtcNow;

        private bool Processing
        {
            set
            {
                if (!value)
                {
                    _processingFinishedTimestamp = DateTime.UtcNow;
                }
                else
                {
                    _processingFinishedTimestamp = null;
                }
            }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="processorKey">プロセッサーキー</param>
        public KeySpecificDataProcessor(int processorKey)
        {
            ProcessorKey = processorKey;
        }

        /// <summary>
        /// データ処理を開始する
        /// </summary>
        public void StartProcessing()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _processingTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var data = await _internalQueue.Reader.ReadAsync(_cancellationTokenSource.Token);
                        await ProcessData(data);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        // 例外処理（ログ出力など）
                        Console.WriteLine($"処理中にエラーが発生しました: {ex.Message}");
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// データを処理する
        /// </summary>
        /// <param name="data">処理対象データ</param>
        protected virtual async Task ProcessData(MeasurementDataWithKey data)
        {
            // 派生クラスで実装するための仮想メソッド
            await Task.CompletedTask;
        }

        /// <summary>
        /// 処理を停止する
        /// </summary>
        public async Task StopProcessing()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                if (_processingTask != null)
                {
                    await _processingTask;
                }
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// データ処理をスケジュールする
        /// </summary>
        /// <param name="dataWithKey">処理対象のデータ</param>
        public async Task ScheduleDataProcessing(MeasurementDataWithKey dataWithKey)
        {
            if (dataWithKey.Key == ProcessorKey)
            {
                await _internalQueue.Writer.WriteAsync(dataWithKey);
            }
        }
    }
}

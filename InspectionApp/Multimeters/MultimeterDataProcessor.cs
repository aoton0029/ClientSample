using InspectionApp.DataProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionApp.Multimeters
{
    /// <summary>
    /// マルチメーターの測定データを処理する特化プロセッサ
    /// </summary>
    public class MultimeterDataProcessor : KeySpecificDataProcessor
    {
        /// <summary>
        /// 最後に測定された値
        /// </summary>
        public double LastValue { get; private set; }

        /// <summary>
        /// 最後に測定された値のソース
        /// </summary>
        public string? LastValueSource { get; private set; }

        /// <summary>
        /// 測定値が閾値を超えた場合に発生するイベント
        /// </summary>
        public event EventHandler<MeasurementThresholdEventArgs>? ThresholdExceeded;

        /// <summary>
        /// 測定値の上限閾値
        /// </summary>
        public double? UpperThreshold { get; set; }

        /// <summary>
        /// 測定値の下限閾値
        /// </summary>
        public double? LowerThreshold { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="processorKey">プロセッサーのキー</param>
        public MultimeterDataProcessor(int processorKey) : base(processorKey)
        {
        }

        /// <summary>
        /// データ処理を行います
        /// </summary>
        /// <param name="data">処理するデータ</param>
        protected override async Task ProcessData(MeasurementDataWithKey data)
        {
            await base.ProcessData(data);

            LastValue = data.Value;
            LastValueSource = data.Source;

            // 閾値チェック
            if (UpperThreshold.HasValue && data.Value > UpperThreshold.Value)
            {
                ThresholdExceeded?.Invoke(this, new MeasurementThresholdEventArgs(
                    data, ThresholdType.Upper, "測定値が上限閾値を超えました"));
            }
            else if (LowerThreshold.HasValue && data.Value < LowerThreshold.Value)
            {
                ThresholdExceeded?.Invoke(this, new MeasurementThresholdEventArgs(
                    data, ThresholdType.Lower, "測定値が下限閾値を下回りました"));
            }
        }
    }
}

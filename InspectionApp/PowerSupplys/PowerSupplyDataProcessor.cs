using InspectionApp.DataProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionApp.PowerSupplys
{
    /// <summary>
    /// 電源装置の測定データを処理する特化プロセッサ
    /// </summary>
    public class PowerSupplyDataProcessor : KeySpecificDataProcessor
    {
        /// <summary>
        /// 最後に測定された電圧
        /// </summary>
        public double LastVoltage { get; private set; }

        /// <summary>
        /// 最後に測定された電流
        /// </summary>
        public double LastCurrent { get; private set; }

        /// <summary>
        /// 測定値が閾値を超えた場合に発生するイベント
        /// </summary>
        public event EventHandler<MeasurementThresholdEventArgs>? ThresholdExceeded;

        /// <summary>
        /// 電圧の上限閾値
        /// </summary>
        public double? VoltageUpperThreshold { get; set; }

        /// <summary>
        /// 電圧の下限閾値
        /// </summary>
        public double? VoltageLowerThreshold { get; set; }

        /// <summary>
        /// 電流の上限閾値
        /// </summary>
        public double? CurrentUpperThreshold { get; set; }

        /// <summary>
        /// 電流の下限閾値
        /// </summary>
        public double? CurrentLowerThreshold { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="processorKey">プロセッサーのキー</param>
        public PowerSupplyDataProcessor(int processorKey) : base(processorKey)
        {
        }

        /// <summary>
        /// データ処理を行います
        /// </summary>
        /// <param name="data">処理するデータ</param>
        protected override async Task ProcessData(MeasurementDataWithKey data)
        {
            await base.ProcessData(data);

            // ソースに基づいて処理を分ける
            if (data.Source.Contains("Voltage", StringComparison.OrdinalIgnoreCase))
            {
                LastVoltage = data.Value;
                CheckVoltageThresholds(data);
            }
            else if (data.Source.Contains("Current", StringComparison.OrdinalIgnoreCase))
            {
                LastCurrent = data.Value;
                CheckCurrentThresholds(data);
            }
        }

        /// <summary>
        /// 電圧の閾値チェックを行います
        /// </summary>
        /// <param name="data">測定データ</param>
        private void CheckVoltageThresholds(MeasurementDataWithKey data)
        {
            if (VoltageUpperThreshold.HasValue && data.Value > VoltageUpperThreshold.Value)
            {
                ThresholdExceeded?.Invoke(this, new MeasurementThresholdEventArgs(
                    data, ThresholdType.Upper, "電圧が上限閾値を超えました"));
            }
            else if (VoltageLowerThreshold.HasValue && data.Value < VoltageLowerThreshold.Value)
            {
                ThresholdExceeded?.Invoke(this, new MeasurementThresholdEventArgs(
                    data, ThresholdType.Lower, "電圧が下限閾値を下回りました"));
            }
        }

        /// <summary>
        /// 電流の閾値チェックを行います
        /// </summary>
        /// <param name="data">測定データ</param>
        private void CheckCurrentThresholds(MeasurementDataWithKey data)
        {
            if (CurrentUpperThreshold.HasValue && data.Value > CurrentUpperThreshold.Value)
            {
                ThresholdExceeded?.Invoke(this, new MeasurementThresholdEventArgs(
                    data, ThresholdType.Upper, "電流が上限閾値を超えました"));
            }
            else if (CurrentLowerThreshold.HasValue && data.Value < CurrentLowerThreshold.Value)
            {
                ThresholdExceeded?.Invoke(this, new MeasurementThresholdEventArgs(
                    data, ThresholdType.Lower, "電流が下限閾値を下回りました"));
            }
        }
    }
}

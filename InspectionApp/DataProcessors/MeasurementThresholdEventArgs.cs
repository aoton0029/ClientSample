using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionApp.DataProcessors
{
    /// <summary>
    /// 閾値の種類
    /// </summary>
    public enum ThresholdType
    {
        /// <summary>
        /// 上限閾値
        /// </summary>
        Upper,

        /// <summary>
        /// 下限閾値
        /// </summary>
        Lower
    }

    /// <summary>
    /// 閾値超過イベントの引数クラス
    /// </summary>
    public class MeasurementThresholdEventArgs : EventArgs
    {
        /// <summary>
        /// 測定データ
        /// </summary>
        public MeasurementDataWithKey Data { get; }

        /// <summary>
        /// 閾値タイプ
        /// </summary>
        public ThresholdType ThresholdType { get; }

        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="data">測定データ</param>
        /// <param name="thresholdType">閾値タイプ</param>
        /// <param name="message">メッセージ</param>
        public MeasurementThresholdEventArgs(MeasurementDataWithKey data, ThresholdType thresholdType, string message)
        {
            Data = data;
            ThresholdType = thresholdType;
            Message = message;
        }
    }
}

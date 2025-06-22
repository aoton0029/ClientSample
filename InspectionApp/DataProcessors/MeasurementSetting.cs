using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionApp.DataProcessors
{
    /// <summary>
    /// デバイス測定のタイプ
    /// </summary>
    public enum MeasurementType
    {
        /// <summary>
        /// 電圧測定
        /// </summary>
        Voltage,

        /// <summary>
        /// 電流測定
        /// </summary>
        Current,

        /// <summary>
        /// マルチメーター測定（機能依存）
        /// </summary>
        MultimeterValue
    }

    /// <summary>
    /// 測定設定パラメータ
    /// </summary>
    public class MeasurementSettings
    {
        /// <summary>
        /// 測定のキー（一意識別子）
        /// </summary>
        public int MeasurementKey { get; set; }

        /// <summary>
        /// 測定タイプ
        /// </summary>
        public MeasurementType MeasurementType { get; set; }

        /// <summary>
        /// 測定間隔
        /// </summary>
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 電源チャンネル（該当する場合）
        /// </summary>
        public int? PowerSupplyChannel { get; set; }

        /// <summary>
        /// マルチメーター機能（該当する場合）
        /// </summary>
        public string? MultimeterFunction { get; set; }

        /// <summary>
        /// ソース識別子
        /// </summary>
        public string SourceIdentifier { get; set; } = string.Empty;
    }
}

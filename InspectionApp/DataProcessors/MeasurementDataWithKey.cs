using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectionApp.DataProcessors
{
    /// <summary>
    /// キーを持つ測定データを表すクラス
    /// </summary>
    public class MeasurementDataWithKey
    {
        /// <summary>
        /// 測定データの一意識別子
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// 測定値
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 測定タイムスタンプ
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 測定ソース (例: "Multimeter", "PowerSupply")
        /// </summary>
        public string Source { get; set; }
    }
}

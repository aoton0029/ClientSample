using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowersupplyCtrl
{
    public enum ErrorCode
    {
        // 成功
        Success = 0,

        // 一般的なエラー (1-99)
        Unknown = 1,

        // 接続関連エラー (100-199)
        NotConnected = 100,
        ConnectionFailed = 101,
        Disconnected = 102,

        // 初期化関連エラー (200-299)
        NotInitialized = 200,
        InitializationFailed = 201,

        // 通信エラー (300-399)
        CommunicationError = 300,
        Timeout = 301,
        InvalidResponse = 302,

        // デバイスエラー (400-499)
        DeviceError = 400,
        CommandError = 401,

        // 測定エラー (500-599)
        MeasurementError = 500,
        RangeError = 501,
        OverRange = 502
    }
}

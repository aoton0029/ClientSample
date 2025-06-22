using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowersupplyCtrl2
{
    public class DeviceErrorException : Exception
    {
        public ErrorCode ErrorCode { get; }

        public DeviceErrorException(string message, ErrorCode errorCode = ErrorCode.Unknown, Exception? innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// 通信エラーの例外
    /// </summary>
    public class CommunicationException : DeviceErrorException
    {
        public CommunicationException(string message, ErrorCode errorCode = ErrorCode.CommunicationError, Exception? innerException = null)
            : base(message, errorCode, innerException)
        {
        }
    }

    /// <summary>
    /// タイムアウトによる例外
    /// </summary>
    public class TimeoutException : DeviceErrorException
    {
        public TimeoutException(string message, Exception? innerException = null)
            : base(message, ErrorCode.Timeout, innerException)
        {
        }
    }
}

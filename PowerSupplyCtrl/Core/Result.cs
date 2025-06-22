using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowersupplyCtrl
{
    /// <summary>
    /// マルチメーター操作の結果を表すクラス
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 操作が成功したかどうか
        /// </summary>
        public bool IsSuccess => ErrorCode == ErrorCode.Success;

        /// <summary>
        /// エラーコード（成功時は Success）
        /// </summary>
        public ErrorCode ErrorCode { get; }

        /// <summary>
        /// エラーメッセージ（成功時は空文字）
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// 処理中に発生した例外（存在する場合）
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// 成功結果を作成
        /// </summary>
        public static Result Success() => new Result(ErrorCode.Success, string.Empty);

        /// <summary>
        /// エラー結果を作成
        /// </summary>
        public static Result Error(ErrorCode code, string message) => new Result(code, message);

        /// <summary>
        /// 例外からエラー結果を作成
        /// </summary>
        public static Result FromException(Exception ex)
        {
            if (ex is MultimeterException mmEx)
            {
                return new Result(mmEx.ErrorCode, mmEx.Message, mmEx);
            }

            return new Result(ErrorCode.Unknown, ex.Message, ex);
        }

        protected Result(ErrorCode errorCode, string errorMessage, Exception? exception = null)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Exception = exception;
        }
    }

    /// <summary>
    /// 値を持つ操作結果を表すジェネリッククラス
    /// </summary>
    /// <typeparam name="T">結果の値の型</typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// 操作結果の値
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// 値を持つ成功結果を作成
        /// </summary>
        public static Result<T> Success(T value) => new Result<T>(ErrorCode.Success, string.Empty, value);

        /// <summary>
        /// エラー結果を作成
        /// </summary>
        public static new Result<T> Error(ErrorCode code, string message) => new Result<T>(code, message);

        /// <summary>
        /// 例外からエラー結果を作成
        /// </summary>
        public static new Result<T> FromException(Exception ex)
        {
            if (ex is MultimeterException mmEx)
            {
                return new Result<T>(mmEx.ErrorCode, mmEx.Message, default, mmEx);
            }

            return new Result<T>(ErrorCode.Unknown, ex.Message, default, ex);
        }

        private Result(ErrorCode errorCode, string errorMessage, T? value = default, Exception? exception = null)
            : base(errorCode, errorMessage, exception)
        {
            Value = value;
        }
    }
}

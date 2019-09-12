using System;

namespace IUBH.TOR.Domain
{
    /// <summary>
    /// Reflects the result of an operation that is either successful or
    /// erroneous. In the latter case an exception or an error message is
    /// being provided.
    /// </summary>
    public class Result
    {
        protected bool IsError { get; set; }

        public string ErrorMessage { get; protected set; }
        public Exception Exception { get; protected set; }

        public bool IsSuccessful => !IsError && ErrorMessage == null && Exception == null;

        public static readonly Result Success = new Result();

        public static Result<T> WithSuccess<T>()
        {
            return new Result<T>();
        }

        public static Result<T> WithSuccess<T>(T val)
        {
            return new Result<T>
            {
                Value = val
            };
        }

        public static Result WithError(string errorMessage)
        {
            return new Result
            {
                IsError = true,
                ErrorMessage = errorMessage
            };
        }

        public static Result<T> WithError<T>(string errorMessage)
        {
            return new Result<T>
            {
                IsError = true,
                ErrorMessage = errorMessage
            };
        }

        public static Result<T> WithError<T>(Result result)
        {
            return new Result<T>
            {
                IsError = true,
                ErrorMessage = result.ErrorMessage,
                Exception = result.Exception
            };
        }

        public static Result WithException(Exception exception)
        {
            return new Result
            {
                IsError = true,
                Exception = exception,
                ErrorMessage = exception.Message
            };
        }

        public static Result<T> WithException<T>(Exception exception)
        {
            return new Result<T>
            {
                IsError = true,
                Exception = exception,
                ErrorMessage = exception.Message
            };
        }

        public static Result WithException(Exception exception, string errorMessage)
        {
            return new Result
            {
                IsError = true,
                Exception = exception,
                ErrorMessage = errorMessage
            };
        }

        public static Result<T> WithException<T>(Exception exception, string errorMessage)
        {
            return new Result<T>
            {
                IsError = true,
                Exception = exception,
                ErrorMessage = errorMessage
            };
        }
        
        /// <summary>
        /// Converts this result to a result of the given type, keeping
        /// exception and error message but not the value. 
        /// </summary>
        public Result<T2> ToResult<T2>()
        {
            return new Result<T2>
            {
                IsError = IsError,
                Exception = Exception,
                ErrorMessage = ErrorMessage
            };
        }
    }

    /// <summary>
    /// Reflects the result of an operation that is either successful or
    /// erroneous. In the latter case an exception or an error message is
    /// being provided. If the result is successful the value will contain
    /// the actual operation's result of the given type.
    /// </summary>
    public class Result<T> : Result
    {
        public T Value { get; set; }
    }
}

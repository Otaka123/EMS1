using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Common
{
   public record Result
    {
        public bool Succeeded { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }

        public static Result Success() => new() { Succeeded = true };
        public static Result Failure(string errorMessage, Exception? ex = null) => new()
        {
            Succeeded = false,
            ErrorMessage = errorMessage,
            Exception = ex
        };
    }

    public record Result<T> : Result
    {
        public T? Data { get; init; }

        public static Result<T> Success(T data) => new()
        {
            Succeeded = true,
            Data = data
        };

        public static new Result<T> Failure(string errorMessage, Exception? ex = null) => new()
        {
            Succeeded = false,
            ErrorMessage = errorMessage,
            Exception = ex
        };
    }

}

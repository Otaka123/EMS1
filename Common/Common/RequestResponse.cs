

using Common.Application.Common;

public class RequestResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public ResponseError? ErrorR { get; set; }
    public List<string>? Errors { get; set; }
    public Dictionary<string, object>? Extensions { get; set; }
    public RequestResponse() // ✅ هذا ما كان ناقصًا
    {
    }
    // Success Responses
    public static RequestResponse Success(string? message = null, Dictionary<string, object>? extensions = null)
        => new() { IsSuccess = true, Message = message, Extensions = extensions };

    public static RequestResponse Ok(string? message = null)
        => Success(message);

    // Error Responses
    public static RequestResponse Error(ResponseError error, string? message = null,
        List<string>? errors = null, Dictionary<string, object>? extensions = null)
        => new()
        {
            IsSuccess = false,
            ErrorR = error,
            Message = message,
            Errors = errors,
            Extensions = extensions
        };

    protected static RequestResponse Fail(ResponseError error, string? message = null,
        List<string>? errors = null, Dictionary<string, object>? extensions = null)
        => new()
        {
            IsSuccess = false,
            ErrorR = error,
            Message = message,
            Errors = errors,
            Extensions = extensions
        };

    public static RequestResponse Fail(string? message = null, List<string>? errors = null)
       => Fail(ResponseError.BadRequest, message ?? "Invalid request", errors);

    public static RequestResponse BadRequest(string? message = null, List<string>? errors = null)
        => Fail(ResponseError.BadRequest, message ?? "Invalid request", errors);

    public static RequestResponse Unauthorized(string? message = null)
        => Fail(ResponseError.Unauthorized, message ?? "Unauthorized access");

    public static RequestResponse Forbidden(string? message = null)
        => Fail(ResponseError.Forbidden, message ?? "Forbidden");

    public static RequestResponse NotFound(string? message = null)
        => Fail(ResponseError.NotFound, message ?? "Resource not found");

    public static RequestResponse Locked(string? message = null)
        => Fail(ResponseError.Locked, message ?? "Account locked");

    public static RequestResponse Conflict(string? message = null)
        => Fail(ResponseError.Conflict, message ?? "Conflict occurred");

    public static RequestResponse Unprocessable(string? message = null, List<string>? errors = null)
        => Fail(ResponseError.UnprocessableEntity, message ?? "Validation failed", errors);

    public static RequestResponse TooManyRequests(string? message = null)
        => Fail(ResponseError.TooManyRequests, message ?? "Too many requests");

    public static RequestResponse InternalServerError(string? message = null)
        => Fail(ResponseError.InternalServerError, message ?? "Internal server error");

    // Custom error with extensions
    public static RequestResponse CustomError(ResponseError error, string? message = null,
        List<string>? errors = null, Dictionary<string, object>? extensions = null)
        => Fail(error, message, errors, extensions);
}

public class RequestResponse<T> : RequestResponse
{
    public T? Data { get; set; }

    // Success with Data
    public RequestResponse() // ✅ هذا ما كان ناقصًا
    {
    }

    public static RequestResponse<T> Success(T data, string? message = null,
        Dictionary<string, object>? extensions = null)
        => new()
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            Extensions = extensions
        };
   
    public static RequestResponse<T> Ok(T data, string? message = null)
        => Success(data, message);

    // Error Responses
    public new static RequestResponse<T> Fail(string? message = null, List<string>? errors = null)
       => Error(ResponseError.BadRequest, message ?? "Invalid request", errors);

    public new static RequestResponse<T> Error(ResponseError error, string? message = null,
        List<string>? errors = null, Dictionary<string, object>? extensions = null)
        => new()
        {
            IsSuccess = false,
            ErrorR = error,
            Message = message,
            Errors = errors,
            Extensions = extensions
        };

    public new static RequestResponse<T> BadRequest(string? message = null, List<string>? errors = null)
        => Error(ResponseError.BadRequest, message ?? "Invalid request", errors);

    public new static RequestResponse<T> Unauthorized(string? message = null)
        => Error(ResponseError.Unauthorized, message ?? "Unauthorized access");

    public new static RequestResponse<T> Forbidden(string? message = null)
        => Error(ResponseError.Forbidden, message ?? "Forbidden");

    public new static RequestResponse<T> NotFound(string? message = null)
        => Error(ResponseError.NotFound, message ?? "Resource not found");

    public new static RequestResponse<T> Locked(string? message = null)
        => Error(ResponseError.Locked, message ?? "Account locked");

    public new static RequestResponse<T> Conflict(string? message = null)
        => Error(ResponseError.Conflict, message ?? "Conflict occurred");

    public new static RequestResponse<T> Unprocessable(string? message = null, List<string>? errors = null)
        => Error(ResponseError.UnprocessableEntity, message ?? "Validation failed", errors);

    public new static RequestResponse<T> TooManyRequests(string? message = null)
        => Error(ResponseError.TooManyRequests, message ?? "Too many requests");

    public new static RequestResponse<T> InternalServerError(string? message = null)
        => Error(ResponseError.InternalServerError, message ?? "Internal server error");

    // Custom error with extensions
    public new static RequestResponse<T> CustomError(ResponseError error, string? message = null,
        List<string>? errors = null, Dictionary<string, object>? extensions = null)
        => Error(error, message, errors, extensions);
}
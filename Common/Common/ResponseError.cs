//namespace Common.Application.Common
//{
//    //public sealed record ResponseError
//    //{
//    //    public int StatusCode { get;  }
//    //    public string Title { get; }
//    //    public string Type { get; }
//    //    public IEnumerable<string>? details = null;
//    //    private ResponseError(int statusCode, string title, string type)
//    //    {
//    //        StatusCode = statusCode;
//    //        Title = title;
//    //        Type = type;
//    //    }

//    //    // Common Errors (RFC 7231, RFC 6585)
//    //    public static readonly ResponseError BadRequest = new(400, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1");
//    //    public static readonly ResponseError Unauthorized = new(401, "Unauthorized", "https://tools.ietf.org/html/rfc7235#section-3.1");
//    //    public static readonly ResponseError Forbidden = new(403, "Forbidden", "https://tools.ietf.org/html/rfc7231#section-6.5.3");
//    //    public static readonly ResponseError NotFound = new(404, "Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4");
//    //    public static readonly ResponseError Locked = new(423, "Locked", "https://tools.ietf.org/html/rfc4918#section-11.3");
//    //    public static readonly ResponseError Unprocessable = new(422, "Unprocessable Entity", "https://tools.ietf.org/html/rfc4918#section-11.2");
//    //    public static readonly ResponseError InternalServerError = new(500, "Internal Server Error", "https://tools.ietf.org/html/rfc7231#section-6.6.1");
//    //}
//    public sealed record ResponseError
//    {
//        public int StatusCode { get; }
//        public string Title { get; }
//        public string Type { get; }
//        public IEnumerable<string>? Details { get; init; } // جعله init-only مع دعم null
//        public string TraceId { get; set; } = Guid.NewGuid().ToString();
//        public DateTime Timestamp { get; set; } = DateTime.UtcNow;


//        private ResponseError(int statusCode, string title, string type)
//        {
//            StatusCode = statusCode;
//            Title = title;
//            Type = type;
//        }

//        // طريقة إنشاء مرنة مع التفاصيل
//        public static ResponseError Create(ResponseError template, IEnumerable<string>? details = null)
//        {
//            return template with { Details = details };
//        }

//        // Common Errors (RFC 7231, RFC 6585, RFC 4918)
//        public static readonly ResponseError BadRequest = new(400, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1");
//        public static readonly ResponseError Unauthorized = new(401, "Unauthorized", "https://tools.ietf.org/html/rfc7235#section-3.1");
//        public static readonly ResponseError Forbidden = new(403, "Forbidden", "https://tools.ietf.org/html/rfc7231#section-6.5.3");
//        public static readonly ResponseError NotFound = new(404, "Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4");
//        public static readonly ResponseError MethodNotAllowed = new(405, "Method Not Allowed", "https://tools.ietf.org/html/rfc7231#section-6.5.5");
//        public static readonly ResponseError Conflict = new(409, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.8");
//        public static readonly ResponseError Gone = new(410, "Gone", "https://tools.ietf.org/html/rfc7231#section-6.5.9");
//        public static readonly ResponseError UnprocessableEntity = new(422, "Unprocessable Entity", "https://tools.ietf.org/html/rfc4918#section-11.2");
//        public static readonly ResponseError Locked = new(423, "Locked", "https://tools.ietf.org/html/rfc4918#section-11.3");
//        public static readonly ResponseError TooManyRequests = new(429, "Too Many Requests", "https://tools.ietf.org/html/rfc6585#section-4");
//        public static readonly ResponseError InternalServerError = new(500, "Internal Server Error", "https://tools.ietf.org/html/rfc7231#section-6.6.1");
//        public static readonly ResponseError NotImplemented = new(501, "Not Implemented", "https://tools.ietf.org/html/rfc7231#section-6.6.2");
//        public static readonly ResponseError BadGateway = new(502, "Bad Gateway", "https://tools.ietf.org/html/rfc7231#section-6.6.3");
//        public static readonly ResponseError ServiceUnavailable = new(503, "Service Unavailable", "https://tools.ietf.org/html/rfc7231#section-6.6.4");
//        public static readonly ResponseError GatewayTimeout = new(504, "Gateway Timeout", "https://tools.ietf.org/html/rfc7231#section-6.6.5");

//        // أخطاء مخصصة (يمكن إضافة المزيد حسب الحاجة)
//        public static readonly ResponseError ValidationError = new(400, "Validation Error", "https://tools.ietf.org/html/rfc7231#section-6.5.1");
//        public static readonly ResponseError EmailNotConfirmed = new(403, "Email Not Confirmed", "https://example.com/errors/email-not-confirmed");
//        public static readonly ResponseError AccountSuspended = new(403, "Account Suspended", "https://example.com/errors/account-suspended");
//        public static readonly ResponseError PaymentRequired = new(402, "Payment Required", "https://tools.ietf.org/html/rfc7231#section-6.5.2");

//        // Client Closed Request (غير معتمد رسميًا ولكن شائع الاستخدام)
//        public static readonly ResponseError RequestCancelled = new(
//     498,
//     "Request Cancelled",
//     "https://httpstatuses.com/498");

//        public static readonly ResponseError ClientClosedRequest = new(
//            499,
//            "Client Closed Request",
//            "https://nginx.org/en/docs/http/ngx_http_log_module.html#log_format");
//    }
//}
namespace Common.Application.Common
{
    /// <summary>
    /// سجل يمثل خطأ في الاستجابة مع تفاصيل متكاملة حسب معايير HTTP
    /// </summary>
    /// <remarks>
    /// يتضمن معلومات تتبع وتوقيت زمني وأكواد أخطاء قياسية
    /// </remarks>
    public sealed record ResponseError
    {
        public int StatusCode { get; private set; }
        public string Title { get; private set; }
        public string Type { get; private set; }
        public IEnumerable<string>? Details { get; init; }
        public string TraceId { get; init; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string? Instance { get; init; }

        private ResponseError(int statusCode, string title, string type)
        {
            StatusCode = statusCode;
            Title = title;
            Type = type;
        }
        public static ResponseError Create(string type, string title, IDictionary<string, string[]> errors)=> new ResponseError(400, title, type)
        {
            Details = errors.SelectMany(kvp => kvp.Value).ToArray()
        };

        /// <summary>
        /// إنشاء نسخة مخصصة من خطأ موجود مع تفاصيل إضافية
        /// </summary>
        public ResponseError WithDetails(params string[] details)
            => this with { Details = details };

        /// <summary>
        /// إنشاء نسخة مخصصة من خطأ موجود مع معرف تتبع محدد
        /// </summary>
        public ResponseError WithTraceId(string traceId)
            => this with { TraceId = traceId };

        /// <summary>
        /// إنشاء نسخة مخصصة من خطأ موجود مع مسار المثيل
        /// </summary>
        public ResponseError WithInstance(string instancePath)
            => this with { Instance = instancePath };

        // أخطاء HTTP القياسية (RFC 7231, RFC 6585, RFC 4918)
        public static readonly ResponseError BadRequest = new(400, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1");
        public static readonly ResponseError Unauthorized = new(401, "Unauthorized", "https://tools.ietf.org/html/rfc7235#section-3.1");
        public static readonly ResponseError PaymentRequired = new(402, "Payment Required", "https://tools.ietf.org/html/rfc7231#section-6.5.2");
        public static readonly ResponseError Forbidden = new(403, "Forbidden", "https://tools.ietf.org/html/rfc7231#section-6.5.3");
        public static readonly ResponseError NotFound = new(404, "Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4");
        public static readonly ResponseError MethodNotAllowed = new(405, "Method Not Allowed", "https://tools.ietf.org/html/rfc7231#section-6.5.5");
        public static readonly ResponseError Conflict = new(409, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.8");
        public static readonly ResponseError Gone = new(410, "Gone", "https://tools.ietf.org/html/rfc7231#section-6.5.9");
        public static readonly ResponseError UnprocessableEntity = new(422, "Unprocessable Entity", "https://tools.ietf.org/html/rfc4918#section-11.2");
        public static readonly ResponseError Locked = new(423, "Locked", "https://tools.ietf.org/html/rfc4918#section-11.3");
        public static readonly ResponseError TooManyRequests = new(429, "Too Many Requests", "https://tools.ietf.org/html/rfc6585#section-4");
        public static readonly ResponseError RequestCancelled = new(498, "Request Cancelled", "https://httpstatuses.com/498");
        public static readonly ResponseError ClientClosedRequest = new(499, "Client Closed Request", "https://nginx.org/en/docs/http/ngx_http_log_module.html#log_format");
        public static readonly ResponseError InternalServerError = new(500, "Internal Server Error", "https://tools.ietf.org/html/rfc7231#section-6.6.1");
        public static readonly ResponseError NotImplemented = new(501, "Not Implemented", "https://tools.ietf.org/html/rfc7231#section-6.6.2");
        public static readonly ResponseError BadGateway = new(502, "Bad Gateway", "https://tools.ietf.org/html/rfc7231#section-6.6.3");
        public static readonly ResponseError ServiceUnavailable = new(503, "Service Unavailable", "https://tools.ietf.org/html/rfc7231#section-6.6.4");
        public static readonly ResponseError GatewayTimeout = new(504, "Gateway Timeout", "https://tools.ietf.org/html/rfc7231#section-6.6.5");

        // أخطاء مخصصة للتطبيق
        public static readonly ResponseError ValidationError = BadRequest with
        {
            Title = "Validation Error",
            Type = "https://example.com/errors/validation"
        };

        public static readonly ResponseError EmailNotConfirmed = Forbidden with
        {
            Title = "Email Not Confirmed",
            Type = "https://example.com/errors/email-not-confirmed"
        };

        public static readonly ResponseError AccountSuspended = Forbidden with
        {
            Title = "Account Suspended",
            Type = "https://example.com/errors/account-suspended"
        };
    }
}
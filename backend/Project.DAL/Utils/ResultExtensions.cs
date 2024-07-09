using Microsoft.AspNetCore.Http;

namespace Project.DAL.Utils
{
    public static class ResultExtensions
    {
        public static IResult ToProblemDetails(this Result result)
        {
            if (result.IsSuccess) throw new InvalidOperationException();

            return Results.Problem(
                statusCode: GetStatusCode(result.Error.Type),
                title: GetTitle(result.Error.Type),
                type: GetType(result.Error.Type),
                extensions: new Dictionary<string, object?>
                {
                    {"errors", new[]{result.Error} }
                });

        }

        static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

        static string GetTitle(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Conflict => "Conflict",
                ErrorType.Validation => "Bad Request",
                ErrorType.NotFound => "Not Found",
                _ => "Internal server error"
            };

        static string GetType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

        public static IResult ToProblemDetails<T>(this Result<T> result)
        {
            if (result.IsSuccess) throw new InvalidOperationException();

            return Results.Problem(
                statusCode: GetStatusCode(result.Error.Type),
                title: GetTitle(result.Error.Type),
                type: GetType(result.Error.Type),
                extensions: new Dictionary<string, object?>
                {
                {"errors", new[]{result.Error} }
                });
        }

        public static IResult ToHttpResult<T>(this Result<T> result)
        {
            return result.IsSuccess
                ? Results.Ok(result.Payload)
                : result.ToProblemDetails();
        }

        public static IResult ToHttpResult(this Result result)
        {
            return result.IsSuccess
                ? Results.Ok()
                : result.ToProblemDetails();
        }
    }
}
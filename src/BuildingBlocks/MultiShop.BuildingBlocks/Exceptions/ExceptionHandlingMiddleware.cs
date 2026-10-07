using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MultiShop.BuildingBlocks.Exceptions;

/// <summary>
/// Yakalanmayan exception'ları RFC 7807 ProblemDetails yanıtına çevirir.
/// 4xx hatalar Warning, 5xx hatalar stack trace ile Error olarak loglanır.
/// 500'de iç ayrıntı yalnızca Development ortamında gösterilir.
/// </summary>
/// <remarks>
/// .NET 8'in <c>UseExceptionHandler</c> + <c>IExceptionHandler</c> ikilisi her exception'ı
/// (404/400 olanları da) Error seviyesinde logladığı için kendi middleware'imizi kullanıyoruz.
/// </remarks>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IProblemDetailsService problemDetailsService,
    IOptions<ExceptionMappingOptions> options,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // İstemci bağlantıyı kapattı; yazılacak yanıt yok.
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var statusCode = options.Value.GetStatusCode(exception);
        var isServerError = statusCode >= StatusCodes.Status500InternalServerError;

        if (isServerError)
            logger.LogError(exception, "İşlenmeyen hata: {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning("İstek {StatusCode} ile reddedildi: {Method} {Path} – {Message}",
                statusCode, context.Request.Method, context.Request.Path, exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Detail = isServerError && !environment.IsDevelopment()
                ? "Beklenmeyen bir hata oluştu."
                : exception.Message,
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails,
            Exception = exception,
        });
    }
}

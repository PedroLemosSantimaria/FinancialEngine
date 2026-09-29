using FinancialEngine.Api.Exceptions;
using System.Data;
using System.Net;
using System.Text.Json;

namespace FinancialEngine.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            DuplicateEventException => (HttpStatusCode.Conflict, "Evento duplicado"),
            InsufficientBalanceException => (HttpStatusCode.UnprocessableEntity, "Saldo insuficiente"),
            AccountNotFoundException => (HttpStatusCode.NotFound, "Conta não encontrada"),
            ArgumentOutOfRangeException => (HttpStatusCode.BadRequest, "Requisição inválida"),
            DBConcurrencyException => (HttpStatusCode.Conflict, "Conflito de concorrência, tente novamente"),
            _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Erro não tratado ao processar {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning("{ExceptionType}: {Message}", ex.GetType().Name, ex.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new
        {
            title,
            status = (int)statusCode,
            detail = ex.Message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
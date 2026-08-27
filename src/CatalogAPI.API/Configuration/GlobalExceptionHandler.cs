using System.Data.Common;
using CatalogAPI.Domain.Contexts.Games.Exceptions;
using CatalogAPI.Domain.Contexts.Libraries.Exceptions;
using CatalogAPI.Domain.Shared;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UnauthorizedAccessException = CatalogAPI.Domain.Contexts.Libraries.Exceptions.UnauthorizedAccessException;

namespace CatalogAPI.API.Configuration;

/// <summary>
/// Traduz exceções de domínio (e falhas de infraestrutura) para respostas HTTP (ProblemDetails),
/// centralizando o mapeamento num único lugar. Os handlers da camada de Application apenas
/// <b>lançam</b> a exceção adequada; a decisão do status HTTP vive aqui.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = Map(exception);

        // Erros esperados (regra de domínio) não poluem o log como erro; 5xx sim.
        if (statusCode >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Erro não tratado ao processar {Path}", httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            // Mensagem de domínio é segura para expor; em 5xx evitamos vazar detalhes internos.
            Detail = statusCode >= StatusCodes.Status500InternalServerError
                ? "Ocorreu um erro inesperado ao processar a requisição."
                : exception.Message,
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title) Map(Exception exception) => exception switch
    {
        // Exceções de domínio específicas — checadas antes da base DomainException.
        GameNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
        GameAlreadyOwnedException => (StatusCodes.Status409Conflict, "Conflito"),
        InsufficientGameManagementPermissionException => (StatusCodes.Status403Forbidden, "Acesso negado"),
        UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Acesso negado"),

        // Demais violações de invariante/validação de domínio.
        DomainException => (StatusCodes.Status400BadRequest, "Requisição inválida"),

        // Falhas de infraestrutura.
        DbException => (StatusCodes.Status500InternalServerError, "Erro ao acessar o banco de dados"),
        _ => (StatusCodes.Status500InternalServerError, "Erro interno"),
    };
}

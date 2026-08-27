using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.API.Endpoints;

public static class LibraryEndpoints
{
    public static void MapLibrary(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/library")
            .WithTags("Library");

        group.MapPost("/add", AddGameToLibrary)
            .WithName("AddGameToLibrary")
            .RequireAuthorization();

        group.MapGet("/", GetLibrary)
            .WithName("GetLibrary")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetLibrary(
        [AsParameters] Application.Contexts.Libraries.UseCases.GetLibrary.DTOs.PaginationParametersRequest parameters,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userIdClaim = user.FindFirst("sub")?.Value 
                          ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var request = new Application.Contexts.Libraries.UseCases.GetLibrary.Request(
            userId, parameters.Page, parameters.PageSize);
        var response = await sender.Send(request, cancellationToken);

        if (!response.IsSuccess)
            return Results.Json(response, statusCode: response.StatusCode);

        return Results.Ok(response);
    }

    private static async Task<IResult> AddGameToLibrary(
        [FromBody] Application.Contexts.Libraries.UseCases.InitiateGamePurchase.DTOs.PurchaseRequestData body,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken
        )
    {
        var userIdClaim = user.FindFirst("sub")?.Value 
                          ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var request = new Application.Contexts.Libraries.UseCases.InitiateGamePurchase.Request(userId, body.GameId);
        var response = await sender.Send(request, cancellationToken);
        
        if (!response.IsSuccess)
            return Results.Json(response, statusCode: response.StatusCode);
        
        return Results.Accepted();
    }
}

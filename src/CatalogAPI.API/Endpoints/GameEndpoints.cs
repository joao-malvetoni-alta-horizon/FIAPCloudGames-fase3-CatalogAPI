using CatalogAPI.Application.Contexts.Games.UseCases.Create;
using CatalogAPI.Application.Contexts.Games.UseCases.Update.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.API.Endpoints;

public static class GameEndpoints
{
    public static void MapGame(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/games")
            .WithTags("Games");
        
        group.MapPost("/", CreateAsync)
            .WithName("CreateGame");

        group.MapGet("/", GetAllAsync)
            .WithName("GetAllGames");

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetById");

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteGame");

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateGame");
    }

    private static async Task<IResult> CreateAsync(
        Request request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(request, cancellationToken);

        if (!response.IsSuccess)
            return Results.Json(response, statusCode: response.StatusCode);
        
        return Results.Created($"/api/v1/games/{response.Game?.Id}", response);
    }

    private static async Task<IResult> GetAllAsync(
        [AsParameters] Application.Contexts.Games.UseCases.GetAll.Request request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(request, cancellationToken);

        if (!response.IsSuccess)
            return Results.Json(response, statusCode: response.StatusCode);
        
        return Results.Ok(response);
    }

    private static async Task<IResult> GetByIdAsync(
        [FromRoute] Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var request = new Application.Contexts.Games.UseCases.GetById.Request(id);
        var response = await sender.Send(request, cancellationToken);

        if (!response.IsSuccess)
            return Results.Json(response, statusCode: response.StatusCode);
        
        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteAsync(
        [FromRoute] Guid id,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var request = new Application.Contexts.Games.UseCases.Delete.Request(id);
        var response = await sender.Send(request, cancellationToken);

        if (!response.IsSuccess)
            return Results.Json(response, statusCode: response.StatusCode);
        
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateGameDTO body,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var request = new Application.Contexts.Games.UseCases.Update.Request(
            id,
            body.Title,
            body.Description,
            body.Price,
            body.Genre,
            body.Status,
            body.ReleaseDate);
        var response = await sender.Send(request, cancellationToken);

        if (!response.IsSuccess)
            return Results.Json(response, statusCode: response.StatusCode);
        
        return Results.NoContent();
    }
}

using BookInformationService.BookInformation.Facade.Delete;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace BookInformationService.BookInformation.Delete;

public static class DeleteEndpoint
{
    public static void MapDeleteBookInformationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/BookInformations/{id:int}", handleAsync)
            .MapToApiVersion(1)
            .MapToApiVersion(2)
            .Produces<DeleteResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");
    }

    public static async Task<IResult> handleAsync(HttpContext httpContext, int id, IDeleteBookInformationBL deleteBookInformationBL, IDistributedCache cache, CancellationToken ct)
    {
        var apiVersion = httpContext.GetRequestedApiVersion();

        DeleteResponse response = await deleteBookInformationBL.DeleteBookInformation(apiVersion!.ToString(), id, ct);

        if (response.ErrorResult != Results.Ok())
        {
            return response.ErrorResult;
        }

        // Invalidate cache by removing cached data
        var cacheKeyOfGet = $"GetBookInformation_{apiVersion}_{id}"; // Cache key based on API version
        await cache.RemoveAsync(cacheKeyOfGet, ct); // Remove the cached data

        var cacheKeyOfList = $"ListBookInformation_{apiVersion}"; // Cache key based on API version
        await cache.RemoveAsync(cacheKeyOfList, ct); // Remove the cached data

        return Results.Ok(response);
    }
}


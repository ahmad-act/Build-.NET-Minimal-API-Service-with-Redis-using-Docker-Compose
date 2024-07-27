using Microsoft.AspNetCore.Mvc;
using BookInformationService.BookInformation.Facade.Get;
using BookInformationService.BookInformation.List;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace BookInformationService.BookInformation.Get;

public static class GetEndpoint
{
    public static void MapGetBookInformationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/BookInformations/{id:int}", handleAsync)
            .MapToApiVersion(1)
            .MapToApiVersion(2)
            .Produces<GetResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");

    }

    public static async Task<IResult> handleAsync(HttpContext httpContext, int id, IGetBookInformationBL getBookInformationBL, IDistributedCache cache, CancellationToken ct)
    {
        var apiVersion = httpContext.GetRequestedApiVersion();
        var cacheKey = $"GetBookInformation_{apiVersion}_{id}"; // Unique cache key based on API version

        // Try to get data from cache
        var cachedData = await cache.GetStringAsync(cacheKey, ct);

        if (!string.IsNullOrEmpty(cachedData))
        {
            var responseFromCache = JsonConvert.DeserializeObject<ListResponse>(cachedData);
            return Results.Ok(responseFromCache);
        }

        // Fetch data from business logic layer if not cached
        GetResponse response = await getBookInformationBL.GetBookInformation(apiVersion!.ToString(), id, ct);

        if (response.ErrorResult != Results.Ok())
        {
            return response.ErrorResult;
        }

        // Store data in cache
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // Cache expiration time
        };

        var serializedData = JsonConvert.SerializeObject(response);
        await cache.SetStringAsync(cacheKey, serializedData, cacheOptions, ct);

        return Results.Ok(response);
    }
}


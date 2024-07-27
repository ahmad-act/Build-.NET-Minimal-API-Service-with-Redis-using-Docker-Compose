﻿using BookInformationService.BookInformation.Facade.List;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace BookInformationService.BookInformation.List;

public static class ListEndpoint
{
    public static void MapListBookInformationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/BookInformations", handleAsync)
            .MapToApiVersion(1)
            .MapToApiVersion(2)
            .Produces<ListResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json");
    }

    public static async Task<IResult> handleAsync(HttpContext httpContext, IListBookInformationBL listBookInformationBL, IDistributedCache cache, CancellationToken ct)
    {
        var apiVersion = httpContext.GetRequestedApiVersion();
        var cacheKey = $"ListBookInformation_{apiVersion}"; // Unique cache key based on API version

        // Try to get data from cache
        var cachedData = await cache.GetStringAsync(cacheKey, ct);

        if (!string.IsNullOrEmpty(cachedData))
        {
            var responseFromCache = JsonConvert.DeserializeObject<ListResponse>(cachedData);
            return Results.Ok(responseFromCache);
        }

        // Fetch data from business logic layer if not cached
        ListResponse response = await listBookInformationBL.ListBookInformation(apiVersion!.ToString(), ct);

        if (response.ErrorResult != Results.Ok())
        {
            return response.ErrorResult;
        }

        // Store data in cache
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) // Cache expiration time
        };

        var serializedData = JsonConvert.SerializeObject(response);
        await cache.SetStringAsync(cacheKey, serializedData, cacheOptions, ct);

        return Results.Ok(response);
    }
}


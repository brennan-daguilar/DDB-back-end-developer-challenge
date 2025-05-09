﻿using System.Net;
using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Hitpoints.Models;
using CharacterHitpointService.Util;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CharacterHitpointService.Api.AddTemporaryHitpoints;

public class AddTemporaryHitpointsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/character/{characterId}/addTemporaryHitpoints", HandleAsync)
            .WithTags("Temporary Hit Points")
            .WithSummary("Add temporary hit points");
    }

    private async Task<Results<
        Ok<AddTemporaryHitpointsResult>,
        ValidationProblem,
        ProblemHttpResult
    >> HandleAsync(string characterId, AddTemporaryHitpointsRequest request,
        IValidator<AddTemporaryHitpointsRequest> validator,
        HitpointService hitpointService)
    {
        request.CharacterId = characterId;
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var result = await hitpointService.AddTemporaryHitpointsAsync(request.CharacterId, request.Amount);

        if (!result.IsSuccess)
            return TypedResults.Problem(detail: result.Error, statusCode: (int)HttpStatusCode.BadRequest);

        return TypedResults.Ok(result.Value);
    }
}
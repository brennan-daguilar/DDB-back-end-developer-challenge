﻿using System.Net;
using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Hitpoints.Models;
using CharacterHitpointService.Shared.Models;
using CharacterHitpointService.Util;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CharacterHitpointService.Api.Damage;

public class DamageCharacterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/character/{characterId}/damage", HandleAsync)
            .WithTags("Damage")
            .WithSummary("Damage a character's hit points");
    }

    private async Task<Results<
        Ok<DamageCharacterResult>,
        ValidationProblem,
        ProblemHttpResult
    >> HandleAsync(string characterId, DamageCharacterRequest request, IValidator<DamageCharacterRequest> validator,
        HitpointService hitpointService)
    {
        request.CharacterId = characterId;
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var damageType = Enum.Parse<DamageType>(request.Type, ignoreCase: true);
        var result = await hitpointService.DamageCharacterAsync(request.CharacterId, request.Amount, damageType);

        if (!result.IsSuccess)
            return TypedResults.Problem(detail: result.Error, statusCode: (int)HttpStatusCode.BadRequest);

        return TypedResults.Ok(result.Value);
    }
}
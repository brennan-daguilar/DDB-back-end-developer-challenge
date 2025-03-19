using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Models;
using CharacterHitpointService.Util;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CharacterHitpointService.Api.AddTemporaryHitpoints;

public class AddTemporaryHitpointsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/character/{characterId}/addTemporaryHitpoints",
            HandleAsync);
    }

    private async Task<Results<
        Ok<AddTemporaryHitpointsResponse>,
        ValidationProblem,
        ProblemHttpResult
    >> HandleAsync(string characterId, AddTemporaryHitpointsRequest request, IValidator<AddTemporaryHitpointsRequest> validator,
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
            return TypedResults.Problem(detail: result.Error);

        return TypedResults.Ok(result.Value);
    }
}
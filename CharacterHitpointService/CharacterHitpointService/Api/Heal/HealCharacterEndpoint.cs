using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Hitpoints.Models;
using CharacterHitpointService.Util;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CharacterHitpointService.Api.Heal;

public class HealCharacterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/character/{characterId}/heal", HandleAsync)
            .WithTags("Heal")
            .WithSummary("Heal a character's hit points");
    }

    private async Task<Results<
        Ok<HealCharacterResult>,
        ValidationProblem,
        ProblemHttpResult
    >> HandleAsync(string characterId, HealCharacterRequest request, IValidator<HealCharacterRequest> validator,
        HitpointService hitpointService)
    {
        request.CharacterId = characterId;
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var result = await hitpointService.HealCharacterAsync(request.CharacterId, request.Amount);

        if (!result.IsSuccess)
            return TypedResults.Problem(detail: result.Error);

        return TypedResults.Ok(result.Value);
    }
}
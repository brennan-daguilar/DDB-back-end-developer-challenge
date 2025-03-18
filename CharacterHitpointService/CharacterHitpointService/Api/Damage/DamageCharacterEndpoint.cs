using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Models;
using CharacterHitpointService.Util;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CharacterHitpointService.Api.Damage;

public class DamageCharacterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/damage", HandleAsync);
    }

    private async Task<Results<
        Ok<DamageCharacterResponse>,
        ValidationProblem,
        ProblemHttpResult
    >> HandleAsync(DamageCharacterRequest request, IValidator<DamageCharacterRequest> validator,
        HitpointService hitpointService)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var damageType = Enum.Parse<DamageType>(request.Type, ignoreCase: true);
        var result = await hitpointService.DamageCharacterAsync(request.CharacterId, request.Damage, damageType);

        if (!result.IsSuccess)
            return TypedResults.Problem(detail: result.Error);

        return TypedResults.Ok(result.Value);
    }
}
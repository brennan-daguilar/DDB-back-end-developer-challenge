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
        ValidationProblem
    >> HandleAsync(DamageCharacterRequest request, IValidator<DamageCharacterRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var damageType = Enum.Parse<DamageType>(request.Type, ignoreCase: true);
        
        // TODO: Damage character and generate response

        return TypedResults.Ok(new DamageCharacterResponse()
        {
            CharacterId = request.CharacterId,
            Before = new CombinedHitpoints(10,5),
            After = new CombinedHitpoints(5,0),
            TotalDamage = 10,
            ResistanceEffect = DamageResistanceEffect.Resisted
        });
    }
}
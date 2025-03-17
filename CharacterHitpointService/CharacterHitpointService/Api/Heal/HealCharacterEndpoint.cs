using CharacterHitpointService.Models;
using CharacterHitpointService.Util;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CharacterHitpointService.Api.Heal;

public class HealCharacterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("character/{characterId}/heal",
            HandleAsync
        );
    }

    private async Task<Results<
        Ok<HealCharacterResponse>,
        ValidationProblem
    >> HandleAsync(HealCharacterRequest request, IValidator<HealCharacterRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // TODO: Heal character and generate response

        return TypedResults.Ok(new HealCharacterResponse()
        {
            CharacterId = request.CharacterId,
            Before = new CombinedHitpoints(10, 0),
            After = new CombinedHitpoints(15, 0),
            ActualHealed = 5
        });
    }
}
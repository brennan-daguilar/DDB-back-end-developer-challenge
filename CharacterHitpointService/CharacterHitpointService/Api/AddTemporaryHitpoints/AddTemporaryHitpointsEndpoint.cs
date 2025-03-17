using CharacterHitpointService.Models;
using CharacterHitpointService.Util;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CharacterHitpointService.Api.AddTemporaryHitpoints;

public class AddTemporaryHitpointsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("character/{characterId}/damage",
            HandleAsync);
    }

    private async Task<Results<
        Ok<AddTemporaryHitpointsResponse>,
        ValidationProblem
    >> HandleAsync(AddTemporaryHitpointsRequest request, IValidator<AddTemporaryHitpointsRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        // TODO: Add temporary hitpoints and generate result

        return TypedResults.Ok(new AddTemporaryHitpointsResponse()
        {
            CharacterId = request.CharacterId,
            Before = new CombinedHitpoints(10, 5),
            After = new CombinedHitpoints(10, 10),
            Gained = 5
        });
    }
}
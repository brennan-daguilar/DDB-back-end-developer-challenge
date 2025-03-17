using FluentValidation;

namespace CharacterHitpointService.Api.AddTemporaryHitpoints;

public class AddTemporaryHitpointsRequest
{
    public string CharacterId { get; set; }
    public int Value { get; set; }

    public sealed class Validator : AbstractValidator<AddTemporaryHitpointsRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CharacterId).NotEmpty();
            RuleFor(x => x.Value).GreaterThan(0);
        }
    }
}
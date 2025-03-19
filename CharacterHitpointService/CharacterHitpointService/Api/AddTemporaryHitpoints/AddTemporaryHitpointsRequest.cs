using FluentValidation;

namespace CharacterHitpointService.Api.AddTemporaryHitpoints;

public class AddTemporaryHitpointsRequest
{
    public string CharacterId { get; set; }
    public int Amount { get; set; }

    public sealed class Validator : AbstractValidator<AddTemporaryHitpointsRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CharacterId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }
}
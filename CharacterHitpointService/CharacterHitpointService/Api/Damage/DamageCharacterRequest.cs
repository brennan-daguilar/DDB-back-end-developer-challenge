using CharacterHitpointService.Models;
using FluentValidation;

namespace CharacterHitpointService.Api.Damage;

public class DamageCharacterRequest
{
    public string CharacterId { get; set; }
    public int Damage { get; set; }
    public string Type { get; set; }

    public sealed class Validator : AbstractValidator<DamageCharacterRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CharacterId).NotEmpty();
            RuleFor(x => x.Damage).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Type).NotNull().IsEnumName(typeof(DamageType), caseSensitive: false);
        }
    }
}
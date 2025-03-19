using System.Text.Json.Serialization;
using CharacterHitpointService.Shared.Models;
using FluentValidation;

namespace CharacterHitpointService.Api.Damage;

public class DamageCharacterRequest
{
    public string CharacterId { get; set; }
    [JsonRequired] public int Amount { get; set; }
    [JsonRequired] public string Type { get; set; }

    public sealed class Validator : AbstractValidator<DamageCharacterRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CharacterId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Type).NotNull().IsEnumName(typeof(DamageType), caseSensitive: false);
        }
    }
}
using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace EducationContentService.Domain.ModuleItem;

public record Position
{
    public const decimal InitialStep = 1000m;

    private Position(ItemType itemType, decimal value)
    {
        Value = value;
        Type = itemType;
    }

    public decimal Value { get; }

    public ItemType Type { get; }

    public static Position First(ItemType itemType)
        => new (ItemType.LESSON, InitialStep);

    public static Result<Position, Error> Between(Position before, Position after)
    {
        if (before.Type != after.Type)
        {
            return GeneralErrors.ValueIsInvalid("позиция");
        }

        if (before.Value >= after.Value)
        {
            return GeneralErrors.ValueIsInvalid("позиция");
        }

        return new Position(before.Type, (before.Value + after.Value) / 2);
    }

    public static Position After(Position previous)
        => new (previous.Type, previous.Value + InitialStep);
}
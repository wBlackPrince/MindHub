using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace EducationContentService.Domain.ValueObjects;

public partial record Description
{
    public const int MAX_LENGTH = 2000;
    public string Value { get; }

    private Description(string value)
    {
        Value = value;
    }

    public static Result<Description, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsInvalid("description");
        }

        string normalized = SpaceRemoveRegex().Replace(value.Trim(), " ");

        if (normalized.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("description");
        }

        return new Description(value);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRemoveRegex();
}
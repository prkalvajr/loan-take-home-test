namespace Loans.Domain.Common;

/// <summary>Shared checks for value objects, so each rule is written once.</summary>
internal static class Guard
{
    /// <summary>Returns the trimmed value, or throws when it is empty or longer than <paramref name="maxLength"/>.</summary>
    public static string Required(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException($"{field} is required.");
        if (trimmed.Length > maxLength)
            throw new DomainException($"{field} must be {maxLength} characters or fewer.");

        return trimmed;
    }
}

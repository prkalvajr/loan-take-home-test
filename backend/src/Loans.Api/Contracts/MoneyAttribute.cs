using System.ComponentModel.DataAnnotations;

namespace Loans.Api.Contracts;

/// <summary>An amount greater than 0 with at most 2 decimal places.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MoneyAttribute() : ValidationAttribute("Enter an amount greater than $0 with at most 2 decimals.")
{
    // Null is left to [Required].
    public override bool IsValid(object? value) =>
        value is null || (value is decimal amount && amount > 0 && decimal.Round(amount, 2) == amount);
}

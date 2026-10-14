using System.ComponentModel.DataAnnotations;

namespace Loans.Api.Contracts;

public sealed record AddressRequest
{
    [Required, MaxLength(200)]
    public string? Street { get; init; }

    [Required, MaxLength(200)]
    public string? City { get; init; }

    [Required, UsState]
    public string? State { get; init; }

    [Required, RegularExpression("^[0-9]{5}(-[0-9]{4})?$", ErrorMessage = "Enter a 5-digit ZIP, like 73301.")]
    public string? ZipCode { get; init; }
}

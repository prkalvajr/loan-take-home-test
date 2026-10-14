using System.ComponentModel.DataAnnotations;

namespace Loans.Api.Contracts;

public sealed record AddressRequest
{
    [Required(ErrorMessage = "Enter the street address."), MaxLength(200)]
    public string? Street { get; init; }

    [Required(ErrorMessage = "Enter the city."), MaxLength(200)]
    public string? City { get; init; }

    [Required(ErrorMessage = "Select a state."), UsState]
    public string? State { get; init; }

    [Required(ErrorMessage = "Enter the ZIP code."), RegularExpression("^[0-9]{5}(-[0-9]{4})?$", ErrorMessage = "Enter a 5-digit ZIP, like 73301.")]
    public string? ZipCode { get; init; }
}

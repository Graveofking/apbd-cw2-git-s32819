using EquipmentRentalService.Domain;

namespace EquipmentRentalService.Services;

public sealed class OperationResult
{
    private OperationResult(bool isSuccess, string? errorMessage = null, Rental? rental = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Rental = rental;
    }

    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public Rental? Rental { get; }

    public static OperationResult Success(Rental? rental = null) => new(true, rental: rental);

    public static OperationResult Failure(string message) => new(false, errorMessage: message);
}

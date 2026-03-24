using EquipmentRentalService.Domain;

namespace EquipmentRentalService.Services;

public interface IUniversityRentalService
{
    User AddUser(User user);
    Equipment AddEquipment(Equipment equipment);

    IReadOnlyList<User> GetAllUsers();
    IReadOnlyList<Equipment> GetAllEquipment();
    IReadOnlyList<Equipment> GetAvailableEquipment();

    OperationResult RentEquipment(int userId, int equipmentId, DateTime rentalDate, int durationDays);
    OperationResult ReturnEquipment(int rentalId, DateTime returnDate);
    OperationResult MarkEquipmentUnavailable(int equipmentId, string reason);

    IReadOnlyList<Rental> GetActiveRentalsForUser(int userId);
    IReadOnlyList<Rental> GetOverdueRentals(DateTime now);
    IReadOnlyList<Rental> GetAllRentals();

    string GenerateSummaryReport(DateTime now);
}

using EquipmentRentalService.Domain;

namespace EquipmentRentalService.Services;

public sealed class UniversityRentalService : IUniversityRentalService
{
    private readonly RentalPolicy _policy;
    private readonly List<User> _users = [];
    private readonly List<Equipment> _equipment = [];
    private readonly List<Rental> _rentals = [];

    public UniversityRentalService(RentalPolicy policy)
    {
        _policy = policy;
    }

    public User AddUser(User user)
    {
        _users.Add(user);
        return user;
    }

    public Equipment AddEquipment(Equipment equipment)
    {
        _equipment.Add(equipment);
        return equipment;
    }

    public IReadOnlyList<User> GetAllUsers() => _users.AsReadOnly();

    public IReadOnlyList<Equipment> GetAllEquipment() => _equipment.AsReadOnly();

    public IReadOnlyList<Equipment> GetAvailableEquipment()
    {
        return _equipment
            .Where(e => e.Status == EquipmentStatus.Available)
            .ToList()
            .AsReadOnly();
    }

    public OperationResult RentEquipment(int userId, int equipmentId, DateTime rentalDate, int durationDays)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user is null)
        {
            return OperationResult.Failure("User not found.");
        }

        var equipment = _equipment.FirstOrDefault(e => e.Id == equipmentId);
        if (equipment is null)
        {
            return OperationResult.Failure("Equipment not found.");
        }

        if (durationDays <= 0)
        {
            return OperationResult.Failure("Rental duration must be greater than 0.");
        }

        if (equipment.Status != EquipmentStatus.Available)
        {
            return OperationResult.Failure("Equipment is not available for rental.");
        }

        var userActiveRentals = _rentals.Count(r => r.User.Id == userId && r.IsActive);
        var maxRentals = _policy.GetUserLimit(user.UserType);
        if (userActiveRentals >= maxRentals)
        {
            return OperationResult.Failure($"User rental limit exceeded. Limit for {user.UserType}: {maxRentals}.");
        }

        var rental = new Rental(user, equipment, rentalDate, durationDays);
        _rentals.Add(rental);
        equipment.SetRented();
        return OperationResult.Success(rental);
    }

    public OperationResult ReturnEquipment(int rentalId, DateTime returnDate)
    {
        var rental = _rentals.FirstOrDefault(r => r.Id == rentalId);
        if (rental is null)
        {
            return OperationResult.Failure("Rental not found.");
        }

        if (!rental.IsActive)
        {
            return OperationResult.Failure("Rental has already been returned.");
        }

        var penalty = _policy.CalculatePenalty(rental.DueDate, returnDate);
        rental.Close(returnDate, penalty);
        rental.Equipment.SetAvailable();
        return OperationResult.Success(rental);
    }

    public OperationResult MarkEquipmentUnavailable(int equipmentId, string reason)
    {
        var equipment = _equipment.FirstOrDefault(e => e.Id == equipmentId);
        if (equipment is null)
        {
            return OperationResult.Failure("Equipment not found.");
        }

        if (equipment.Status == EquipmentStatus.Rented)
        {
            return OperationResult.Failure("Cannot mark rented equipment as unavailable.");
        }

        equipment.SetUnavailable(reason);
        return OperationResult.Success();
    }

    public IReadOnlyList<Rental> GetActiveRentalsForUser(int userId)
    {
        return _rentals
            .Where(r => r.User.Id == userId && r.IsActive)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<Rental> GetOverdueRentals(DateTime now)
    {
        return _rentals
            .Where(r => r.IsOverdue(now))
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<Rental> GetAllRentals() => _rentals.AsReadOnly();

    public string GenerateSummaryReport(DateTime now)
    {
        var totalEquipment = _equipment.Count;
        var availableEquipment = _equipment.Count(e => e.Status == EquipmentStatus.Available);
        var unavailableEquipment = _equipment.Count(e => e.Status == EquipmentStatus.Unavailable);
        var rentedEquipment = _equipment.Count(e => e.Status == EquipmentStatus.Rented);

        var totalUsers = _users.Count;
        var activeRentals = _rentals.Count(r => r.IsActive);
        var overdueRentals = _rentals.Count(r => r.IsOverdue(now));
        var closedRentals = _rentals.Count(r => !r.IsActive);
        var totalPenalties = _rentals.Sum(r => r.Penalty);

        return
            $"Users: {totalUsers}\n" +
            $"Equipment total: {totalEquipment} (Available: {availableEquipment}, Rented: {rentedEquipment}, Unavailable: {unavailableEquipment})\n" +
            $"Rentals active: {activeRentals}, closed: {closedRentals}, overdue: {overdueRentals}\n" +
            $"Total penalties collected: {totalPenalties:C}";
    }
    
    internal void ReplaceStateFromPersistence(
        IReadOnlyList<User> users,
        IReadOnlyList<Equipment> equipment,
        IReadOnlyList<Rental> rentals)
    {
        _users.Clear();
        _users.AddRange(users);
        _equipment.Clear();
        _equipment.AddRange(equipment);
        _rentals.Clear();
        _rentals.AddRange(rentals);
    }

    internal IReadOnlyList<User> UsersForPersistence => _users;
    internal IReadOnlyList<Equipment> EquipmentForPersistence => _equipment;
    internal IReadOnlyList<Rental> RentalsForPersistence => _rentals;
}

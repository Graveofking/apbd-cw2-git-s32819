namespace EquipmentRentalService.Domain;

public sealed class Rental
{
    private static int _idCounter = 1;

    public Rental(User user, Equipment equipment, DateTime rentalDate, int durationDays)
    {
        Id = _idCounter++;
        User = user;
        Equipment = equipment;
        RentalDate = rentalDate;
        DurationDays = durationDays;
        DueDate = rentalDate.AddDays(durationDays);
    }

    public int Id { get; }
    public User User { get; }
    public Equipment Equipment { get; }
    public DateTime RentalDate { get; }
    public DateTime DueDate { get; }
    public int DurationDays { get; }
    public DateTime? ActualReturnDate { get; private set; }
    public decimal Penalty { get; private set; }
    public bool IsActive => ActualReturnDate is null;
    public bool IsOverdue(DateTime now) => IsActive && DueDate.Date < now.Date;

    public void Close(DateTime returnDate, decimal penalty)
    {
        ActualReturnDate = returnDate;
        Penalty = penalty;
    }
}

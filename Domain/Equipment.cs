namespace EquipmentRentalService.Domain;

public enum EquipmentStatus
{
    Available,
    Rented,
    Unavailable
}

public abstract class Equipment
{
    private static int _idCounter = 1;

    protected Equipment(string name)
    {
        Id = _idCounter++;
        Name = name;
        Status = EquipmentStatus.Available;
    }

    public int Id { get; }
    public string Name { get; }
    public EquipmentStatus Status { get; private set; }
    public string? UnavailableReason { get; private set; }

    public void SetRented()
    {
        Status = EquipmentStatus.Rented;
        UnavailableReason = null;
    }

    public void SetAvailable()
    {
        Status = EquipmentStatus.Available;
        UnavailableReason = null;
    }

    public void SetUnavailable(string reason)
    {
        Status = EquipmentStatus.Unavailable;
        UnavailableReason = reason;
    }
}

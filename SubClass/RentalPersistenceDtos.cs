using EquipmentRentalService.Domain;

namespace EquipmentRentalService.Persistence;

public sealed class RentalDataFile
{
    public int Version { get; set; } = 1;
    public List<UserRecord> Users { get; set; } = [];
    public List<EquipmentRecord> Equipment { get; set; } = [];
    public List<RentalRecord> Rentals { get; set; } = [];
}

public sealed class UserRecord
{
    public int Id { get; set; }
    public string Kind { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}

public sealed class EquipmentRecord
{
    public string Kind { get; set; } = "";
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public EquipmentStatus Status { get; set; }
    public string? UnavailableReason { get; set; }
    public string? SerialNumber { get; set; }
    public int? RamGb { get; set; }
    public int? StorageGb { get; set; }
    public int? Lumens { get; set; }
    public bool? HasHdmi { get; set; }
    public string? CameraType { get; set; }
    public bool? HasVideoRecording { get; set; }
}

public sealed class RentalRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EquipmentId { get; set; }
    public DateTime RentalDate { get; set; }
    public int DurationDays { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public decimal Penalty { get; set; }
}

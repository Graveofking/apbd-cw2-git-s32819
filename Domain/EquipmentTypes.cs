namespace EquipmentRentalService.Domain;

public sealed class Laptop : Equipment
{
    public Laptop(string name, string serialNumber, int ramGb, int storageGb) : base(name)
    {
        SerialNumber = serialNumber;
        RamGb = ramGb;
        StorageGb = storageGb;
    }

    internal Laptop(
        int id,
        string name,
        string serialNumber,
        int ramGb,
        int storageGb,
        EquipmentStatus status,
        string? unavailableReason)
        : base(id, name, status, unavailableReason)
    {
        SerialNumber = serialNumber;
        RamGb = ramGb;
        StorageGb = storageGb;
    }

    public string SerialNumber { get; }
    public int RamGb { get; }
    public int StorageGb { get; }
}

public sealed class Projector : Equipment
{
    public Projector(string name, string serialNumber, int lumens, bool hasHdmi) : base(name)
    {
        SerialNumber = serialNumber;
        Lumens = lumens;
        HasHdmi = hasHdmi;
    }

    internal Projector(
        int id,
        string name,
        string serialNumber,
        int lumens,
        bool hasHdmi,
        EquipmentStatus status,
        string? unavailableReason)
        : base(id, name, status, unavailableReason)
    {
        SerialNumber = serialNumber;
        Lumens = lumens;
        HasHdmi = hasHdmi;
    }

    public string SerialNumber { get; }
    public int Lumens { get; }
    public bool HasHdmi { get; }
}

public sealed class Camera : Equipment
{
    public Camera(string name, string serialNumber, string cameraType, bool hasVideoRecording) : base(name)
    {
        SerialNumber = serialNumber;
        CameraType = cameraType;
        HasVideoRecording = hasVideoRecording;
    }

    internal Camera(
        int id,
        string name,
        string serialNumber,
        string cameraType,
        bool hasVideoRecording,
        EquipmentStatus status,
        string? unavailableReason)
        : base(id, name, status, unavailableReason)
    {
        SerialNumber = serialNumber;
        CameraType = cameraType;
        HasVideoRecording = hasVideoRecording;
    }

    public string SerialNumber { get; }
    public string CameraType { get; }
    public bool HasVideoRecording { get; }
}

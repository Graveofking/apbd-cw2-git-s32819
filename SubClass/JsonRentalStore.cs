using System.Text.Json;
using System.Text.Json.Serialization;
using EquipmentRentalService.Domain;
using EquipmentRentalService.Services;

namespace EquipmentRentalService.Persistence;

public sealed class JsonRentalStore : IRentalDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public void Save(UniversityRentalService service, string path)
    {
        var file = new RentalDataFile
        {
            Users = service.UsersForPersistence.Select(ToUserRecord).ToList(),
            Equipment = service.EquipmentForPersistence.Select(ToEquipmentRecord).ToList(),
            Rentals = service.RentalsForPersistence.Select(ToRentalRecord).ToList()
        };

        var json = JsonSerializer.Serialize(file, JsonOptions);
        File.WriteAllText(path, json);
    }

    public void Load(UniversityRentalService service, string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Data file not found.", path);
        }

        var json = File.ReadAllText(path);
        var file = JsonSerializer.Deserialize<RentalDataFile>(json, JsonOptions)
                   ?? throw new InvalidDataException("Invalid JSON.");

        var users = file.Users.Select(FromUserRecord).ToList();
        var equipment = file.Equipment.Select(FromEquipmentRecord).ToList();

        var userById = users.ToDictionary(u => u.Id);
        var equipmentById = equipment.ToDictionary(e => e.Id);

        var rentals = new List<Rental>();
        foreach (var r in file.Rentals)
        {
            if (!userById.TryGetValue(r.UserId, out var user))
            {
                throw new InvalidDataException($"Rental {r.Id}: user {r.UserId} not found.");
            }

            if (!equipmentById.TryGetValue(r.EquipmentId, out var eq))
            {
                throw new InvalidDataException($"Rental {r.Id}: equipment {r.EquipmentId} not found.");
            }

            rentals.Add(new Rental(
                r.Id,
                user,
                eq,
                r.RentalDate,
                r.DurationDays,
                r.DueDate,
                r.ActualReturnDate,
                r.Penalty));
        }

        var maxUser = users.Count == 0 ? 0 : users.Max(u => u.Id);
        var maxEq = equipment.Count == 0 ? 0 : equipment.Max(e => e.Id);
        var maxR = rentals.Count == 0 ? 0 : rentals.Max(r => r.Id);

        User.SynchronizeIdSequenceAfterLoad(maxUser);
        Equipment.SynchronizeIdSequenceAfterLoad(maxEq);
        Rental.SynchronizeIdSequenceAfterLoad(maxR);

        service.ReplaceStateFromPersistence(users, equipment, rentals);
    }

    private static UserRecord ToUserRecord(User u)
    {
        var kind = u switch
        {
            Student => "Student",
            Employee => "Employee",
            _ => throw new InvalidOperationException($"Unknown user type: {u.GetType().Name}")
        };

        return new UserRecord
        {
            Id = u.Id,
            Kind = kind,
            FirstName = u.FirstName,
            LastName = u.LastName
        };
    }

    private static EquipmentRecord ToEquipmentRecord(Equipment e)
    {
        var rec = new EquipmentRecord
        {
            Kind = e.GetType().Name,
            Id = e.Id,
            Name = e.Name,
            Status = e.Status,
            UnavailableReason = e.UnavailableReason
        };

        switch (e)
        {
            case Laptop l:
                rec.SerialNumber = l.SerialNumber;
                rec.RamGb = l.RamGb;
                rec.StorageGb = l.StorageGb;
                break;
            case Projector p:
                rec.SerialNumber = p.SerialNumber;
                rec.Lumens = p.Lumens;
                rec.HasHdmi = p.HasHdmi;
                break;
            case Camera c:
                rec.SerialNumber = c.SerialNumber;
                rec.CameraType = c.CameraType;
                rec.HasVideoRecording = c.HasVideoRecording;
                break;
        }

        return rec;
    }

    private static RentalRecord ToRentalRecord(Rental r)
    {
        return new RentalRecord
        {
            Id = r.Id,
            UserId = r.User.Id,
            EquipmentId = r.Equipment.Id,
            RentalDate = r.RentalDate,
            DurationDays = r.DurationDays,
            DueDate = r.DueDate,
            ActualReturnDate = r.ActualReturnDate,
            Penalty = r.Penalty
        };
    }

    private static User FromUserRecord(UserRecord r)
    {
        return r.Kind switch
        {
            "Student" => new Student(r.Id, r.FirstName, r.LastName),
            "Employee" => new Employee(r.Id, r.FirstName, r.LastName),
            _ => throw new InvalidDataException($"Unknown user kind: {r.Kind}")
        };
    }

    private static Equipment FromEquipmentRecord(EquipmentRecord r)
    {
        return r.Kind switch
        {
            nameof(Laptop) => new Laptop(
                r.Id,
                r.Name,
                r.SerialNumber ?? "",
                r.RamGb ?? 0,
                r.StorageGb ?? 0,
                r.Status,
                r.UnavailableReason),
            nameof(Projector) => new Projector(
                r.Id,
                r.Name,
                r.SerialNumber ?? "",
                r.Lumens ?? 0,
                r.HasHdmi ?? false,
                r.Status,
                r.UnavailableReason),
            nameof(Camera) => new Camera(
                r.Id,
                r.Name,
                r.SerialNumber ?? "",
                r.CameraType ?? "",
                r.HasVideoRecording ?? false,
                r.Status,
                r.UnavailableReason),
            _ => throw new InvalidDataException($"Unknown equipment kind: {r.Kind}")
        };
    }
}

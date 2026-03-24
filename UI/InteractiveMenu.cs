using EquipmentRentalService.Domain;
using EquipmentRentalService.Persistence;
using EquipmentRentalService.Services;

namespace EquipmentRentalService.Ui;


public sealed class InteractiveMenu
{
    private readonly IUniversityRentalService _rentals;
    private readonly UniversityRentalService _concreteService;
    private readonly IRentalDataStore _dataStore;
    private readonly string _defaultDataPath;

    public InteractiveMenu(
        IUniversityRentalService rentals,
        UniversityRentalService concreteService,
        IRentalDataStore dataStore,
        string defaultDataPath)
    {
        _rentals = rentals;
        _concreteService = concreteService;
        _dataStore = dataStore;
        _defaultDataPath = defaultDataPath;
    }

    public void Run()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== University Equipment Rental (menu) ===");
            Console.WriteLine(" 1. Add user");
            Console.WriteLine(" 2. Add equipment");
            Console.WriteLine(" 3. List all users");
            Console.WriteLine(" 4. List all equipment");
            Console.WriteLine(" 5. List available equipment");
            Console.WriteLine(" 6. Rent equipment");
            Console.WriteLine(" 7. Return equipment");
            Console.WriteLine(" 8. Mark equipment unavailable");
            Console.WriteLine(" 9. Active rentals for user");
            Console.WriteLine("10. Overdue rentals");
            Console.WriteLine("11. Summary report");
            Console.WriteLine("12. Save data to JSON");
            Console.WriteLine("13. Load data from JSON");
            Console.WriteLine("14. Filtered reports");
            Console.WriteLine("15. Run assignment demo (adds sample data to current state)");
            Console.WriteLine(" 0. Exit");
            Console.Write("Choice: ");

            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line == "0")
            {
                return;
            }

            try
            {
                HandleChoice(line.Trim());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private void HandleChoice(string choice)
    {
        var now = DateTime.Today;

        switch (choice)
        {
            case "1":
                AddUserFlow();
                break;
            case "2":
                AddEquipmentFlow();
                break;
            case "3":
                foreach (var u in _rentals.GetAllUsers())
                {
                    Console.WriteLine($"#{u.Id} | {u.FirstName} {u.LastName} | {u.UserType}");
                }

                break;
            case "4":
                Console.Write(ReportingHelper.FormatEquipmentLines(_rentals.GetAllEquipment()));
                break;
            case "5":
                Console.Write(ReportingHelper.FormatEquipmentLines(_rentals.GetAvailableEquipment()));
                break;
            case "6":
                RentFlow(now);
                break;
            case "7":
                ReturnFlow(now);
                break;
            case "8":
                UnavailableFlow();
                break;
            case "9":
                ActiveRentalsFlow();
                break;
            case "10":
                Console.Write(ReportingHelper.FormatRentalLines(_rentals.GetOverdueRentals(now)));
                break;
            case "11":
                Console.WriteLine(_rentals.GenerateSummaryReport(now));
                break;
            case "12":
                SaveFlow();
                break;
            case "13":
                LoadFlow();
                break;
            case "14":
                FilteredReportsFlow(now);
                break;
            case "15":
                DemoScenario.Run(_rentals);
                break;
            default:
                Console.WriteLine("Unknown option.");
                break;
        }
    }

    private void AddUserFlow()
    {
        Console.Write("First name: ");
        var first = Console.ReadLine() ?? "";
        Console.Write("Last name: ");
        var last = Console.ReadLine() ?? "";
        Console.Write("Type (1=Student, 2=Employee): ");
        var t = Console.ReadLine();
        User u = t == "2" ? new Employee(first, last) : new Student(first, last);
        _rentals.AddUser(u);
        Console.WriteLine($"Added user #{u.Id} ({u.UserType}).");
    }

    private void AddEquipmentFlow()
    {
        Console.WriteLine("Equipment type: 1=Laptop 2=Projector 3=Camera");
        var type = Console.ReadLine();
        Console.Write("Name: ");
        var name = Console.ReadLine() ?? "";
        Console.Write("Serial number: ");
        var serial = Console.ReadLine() ?? "";

        Equipment eq = type switch
        {
            "1" => new Laptop(
                name,
                serial,
                ReadInt("RAM (GB): "),
                ReadInt("Storage (GB): ")),
            "2" => new Projector(
                name,
                serial,
                ReadInt("Lumens: "),
                ReadBool("Has HDMI (y/n): ")),
            "3" => new Camera(
                name,
                serial,
                ReadLine("Camera type (e.g. DSLR): "),
                ReadBool("Video recording (y/n): ")),
            _ => throw new InvalidOperationException("Invalid type.")
        };

        _rentals.AddEquipment(eq);
        Console.WriteLine($"Added equipment #{eq.Id} ({eq.GetType().Name}).");
    }

    private void RentFlow(DateTime defaultDate)
    {
        var userId = ReadInt("User ID: ");
        var eqId = ReadInt("Equipment ID: ");
        Console.Write($"Rental date (yyyy-MM-dd) [Enter=today {defaultDate:yyyy-MM-dd}]: ");
        var ds = Console.ReadLine();
        var rentalDate = string.IsNullOrWhiteSpace(ds) ? defaultDate : DateTime.Parse(ds!);
        var days = ReadInt("Duration (days): ");
        var r = _rentals.RentEquipment(userId, eqId, rentalDate, days);
        Console.WriteLine(r.IsSuccess ? $"Rental #{r.Rental!.Id} created." : r.ErrorMessage);
    }

    private void ReturnFlow(DateTime defaultDate)
    {
        var rentalId = ReadInt("Rental ID: ");
        Console.Write($"Return date (yyyy-MM-dd) [Enter=today {defaultDate:yyyy-MM-dd}]: ");
        var ds = Console.ReadLine();
        var returnDate = string.IsNullOrWhiteSpace(ds) ? defaultDate : DateTime.Parse(ds!);
        var r = _rentals.ReturnEquipment(rentalId, returnDate);
        Console.WriteLine(
            r.IsSuccess
                ? $"Returned. Penalty: {r.Rental!.Penalty:C}"
                : r.ErrorMessage);
    }

    private void UnavailableFlow()
    {
        var id = ReadInt("Equipment ID: ");
        Console.Write("Reason: ");
        var reason = Console.ReadLine() ?? "Unavailable";
        var r = _rentals.MarkEquipmentUnavailable(id, reason);
        Console.WriteLine(r.IsSuccess ? "Marked unavailable." : r.ErrorMessage);
    }

    private void ActiveRentalsFlow()
    {
        var userId = ReadInt("User ID: ");
        Console.Write(ReportingHelper.FormatRentalLines(_rentals.GetActiveRentalsForUser(userId)));
    }

    private void SaveFlow()
    {
        var path = ReadPathOrDefault();
        _dataStore.Save(_concreteService, path);
        Console.WriteLine($"Saved to {path}");
    }

    private void LoadFlow()
    {
        var path = ReadPathOrDefault();
        _dataStore.Load(_concreteService, path);
        Console.WriteLine($"Loaded from {path}");
    }

    private string ReadPathOrDefault()
    {
        Console.Write($"File path [Enter='{_defaultDataPath}']: ");
        var p = Console.ReadLine();
        return string.IsNullOrWhiteSpace(p) ? _defaultDataPath : p.Trim();
    }

    private void FilteredReportsFlow(DateTime now)
    {
        Console.WriteLine("Equipment: 0=All 1=Available 2=Rented 3=Unavailable");
        var efRaw = ReadInt("Equipment filter: ");
        var ef = Enum.IsDefined(typeof(EquipmentListFilter), efRaw)
            ? (EquipmentListFilter)efRaw
            : EquipmentListFilter.All;

        Console.WriteLine("Rentals: 0=All 1=Active 2=Overdue 3=Closed");
        var rfRaw = ReadInt("Rental filter: ");
        var rf = Enum.IsDefined(typeof(RentalListFilter), rfRaw)
            ? (RentalListFilter)rfRaw
            : RentalListFilter.All;

        Console.Write("Filter rentals by user ID? (Enter = no, or type ID): ");
        var uline = Console.ReadLine();
        int? userId = int.TryParse(uline, out var uid) ? uid : null;

        Console.WriteLine();
        Console.Write(
            ReportingHelper.FormatFilteredSummary(_rentals, ef, rf, now, userId));
    }

    private static int ReadInt(string prompt)
    {
        Console.Write(prompt);
        return int.Parse(Console.ReadLine() ?? "0");
    }

    private static string ReadLine(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }

    private static bool ReadBool(string prompt)
    {
        Console.Write(prompt);
        var s = (Console.ReadLine() ?? "").Trim();
        return s.Equals("y", StringComparison.OrdinalIgnoreCase)
               || s.Equals("yes", StringComparison.OrdinalIgnoreCase)
               || s == "1"
               || s.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}

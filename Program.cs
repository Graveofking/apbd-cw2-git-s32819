using EquipmentRentalService.Domain;
using EquipmentRentalService.Services;

var rentalPolicy = new RentalPolicy(
    studentLimit: 2,
    employeeLimit: 5,
    penaltyPerLateDay: 15m
);

var rentalService = new UniversityRentalService(rentalPolicy);

Console.WriteLine("=== University Equipment Rental Demo ===");
var laptop1 = rentalService.AddEquipment(new Laptop("Lenovo ThinkPad T14", "SN-LT-1001", 16, 512));
var laptop2 = rentalService.AddEquipment(new Laptop("Dell XPS 13", "SN-LT-1002", 16, 256));
var projector1 = rentalService.AddEquipment(new Projector("Epson EB-X49", "SN-PJ-2001", 3600, true));
var camera1 = rentalService.AddEquipment(new Camera("Canon EOS 250D", "SN-CM-3001", "DSLR", true));
var camera2 = rentalService.AddEquipment(new Camera("Sony Alpha 6400", "SN-CM-3002", "Mirrorless", true));

var student = rentalService.AddUser(new Student("Anna", "Nowak"));
var employee = rentalService.AddUser(new Employee("Jan", "Kowalski"));
var student2 = rentalService.AddUser(new Student("Marta", "Wiśniewska"));

Console.WriteLine("\nAll equipment:");
PrintEquipment(rentalService.GetAllEquipment());

Console.WriteLine("\nAvailable equipment:");
PrintEquipment(rentalService.GetAvailableEquipment());


var rentResult1 = rentalService.RentEquipment(student.Id, laptop1.Id, DateTime.Today, 7);
PrintResult("Rent student -> laptop1", rentResult1);

var rentResult2 = rentalService.RentEquipment(student.Id, projector1.Id, DateTime.Today, 3);
PrintResult("Rent student -> projector1", rentResult2);


var rentResult3 = rentalService.RentEquipment(student.Id, camera1.Id, DateTime.Today, 5);
PrintResult("Rent student -> camera1 (should fail, limit exceeded)", rentResult3);

var unavailableResult = rentalService.MarkEquipmentUnavailable(camera2.Id, "Maintenance");
PrintResult("Mark camera2 unavailable", unavailableResult);
var rentUnavailable = rentalService.RentEquipment(employee.Id, camera2.Id, DateTime.Today, 2);
PrintResult("Rent employee -> camera2 (should fail, unavailable)", rentUnavailable);

var activeStudentRental = rentalService.GetActiveRentalsForUser(student.Id).First();
var returnOnTime = rentalService.ReturnEquipment(activeStudentRental.Id, DateTime.Today.AddDays(6));
PrintResult("Return on time", returnOnTime);

var employeeRent = rentalService.RentEquipment(employee.Id, camera1.Id, DateTime.Today, 2);
PrintResult("Rent employee -> camera1", employeeRent);
if (employeeRent.IsSuccess && employeeRent.Rental is not null)
{
    var lateReturn = rentalService.ReturnEquipment(employeeRent.Rental.Id, DateTime.Today.AddDays(5));
    PrintResult("Return late (penalty expected)", lateReturn);
}

var overdueRental = rentalService.RentEquipment(student2.Id, laptop2.Id, DateTime.Today.AddDays(-10), 3);
PrintResult("Create overdue rental for reporting", overdueRental);

Console.WriteLine("\nActive rentals for Anna Nowak:");
PrintRentals(rentalService.GetActiveRentalsForUser(student.Id));

Console.WriteLine("\nOverdue rentals:");
PrintRentals(rentalService.GetOverdueRentals(DateTime.Today));

Console.WriteLine("\n=== Final Summary Report ===");
Console.WriteLine(rentalService.GenerateSummaryReport(DateTime.Today));

static void PrintEquipment(IEnumerable<Equipment> equipmentList)
{
    foreach (var eq in equipmentList)
    {
        Console.WriteLine($"#{eq.Id} | {eq.Name} | {eq.GetType().Name} | Status: {eq.Status}");
    }
}

static void PrintRentals(IEnumerable<Rental> rentals)
{
    foreach (var rental in rentals)
    {
        var returnInfo = rental.ActualReturnDate is null
            ? "not returned"
            : $"returned: {rental.ActualReturnDate:yyyy-MM-dd}, penalty: {rental.Penalty:C}";
        Console.WriteLine(
            $"Rental #{rental.Id} | User #{rental.User.Id} | Equipment #{rental.Equipment.Id} | " +
            $"from {rental.RentalDate:yyyy-MM-dd} to {rental.DueDate:yyyy-MM-dd} | {returnInfo}"
        );
    }
}

static void PrintResult(string operation, OperationResult result)
{
    if (result.IsSuccess)
    {
        Console.WriteLine($"[OK] {operation}");
        return;
    }

    Console.WriteLine($"[FAIL] {operation} -> {result.ErrorMessage}");
}

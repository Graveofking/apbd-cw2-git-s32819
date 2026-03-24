using EquipmentRentalService.Domain;
using EquipmentRentalService.Services;

namespace EquipmentRentalService.Ui;

public static class DemoScenario
{
    public static void Run(IUniversityRentalService rentalService)
    {
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
        Console.Write(ReportingHelper.FormatEquipmentLines(rentalService.GetAllEquipment()));

        Console.WriteLine("\nAvailable equipment:");
        Console.Write(ReportingHelper.FormatEquipmentLines(rentalService.GetAvailableEquipment()));

        PrintResult("Rent student -> laptop1", rentalService.RentEquipment(student.Id, laptop1.Id, DateTime.Today, 7));
        PrintResult("Rent student -> projector1", rentalService.RentEquipment(student.Id, projector1.Id, DateTime.Today, 3));
        PrintResult(
            "Rent student -> camera1 (should fail, limit exceeded)",
            rentalService.RentEquipment(student.Id, camera1.Id, DateTime.Today, 5));

        PrintResult("Mark camera2 unavailable", rentalService.MarkEquipmentUnavailable(camera2.Id, "Maintenance"));
        PrintResult(
            "Rent employee -> camera2 (should fail, unavailable)",
            rentalService.RentEquipment(employee.Id, camera2.Id, DateTime.Today, 2));

        var activeStudentRental = rentalService.GetActiveRentalsForUser(student.Id).First();
        PrintResult("Return on time", rentalService.ReturnEquipment(activeStudentRental.Id, DateTime.Today.AddDays(6)));

        var employeeRent = rentalService.RentEquipment(employee.Id, camera1.Id, DateTime.Today, 2);
        PrintResult("Rent employee -> camera1", employeeRent);
        if (employeeRent.IsSuccess && employeeRent.Rental is not null)
        {
            PrintResult(
                "Return late (penalty expected)",
                rentalService.ReturnEquipment(employeeRent.Rental.Id, DateTime.Today.AddDays(5)));
        }

        PrintResult(
            "Create overdue rental for reporting",
            rentalService.RentEquipment(student2.Id, laptop2.Id, DateTime.Today.AddDays(-10), 3));

        Console.WriteLine("\nActive rentals for Anna Nowak:");
        Console.Write(ReportingHelper.FormatRentalLines(rentalService.GetActiveRentalsForUser(student.Id)));

        Console.WriteLine("\nOverdue rentals:");
        Console.Write(ReportingHelper.FormatRentalLines(rentalService.GetOverdueRentals(DateTime.Today)));

        Console.WriteLine("\n=== Final Summary Report ===");
        Console.WriteLine(rentalService.GenerateSummaryReport(DateTime.Today));
    }

    private static void PrintResult(string operation, OperationResult result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine($"[OK] {operation}");
            return;
        }

        Console.WriteLine($"[FAIL] {operation} -> {result.ErrorMessage}");
    }
}

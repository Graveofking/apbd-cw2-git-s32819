using EquipmentRentalService.Persistence;
using EquipmentRentalService.Services;
using EquipmentRentalService.Ui;

var rentalPolicy = new RentalPolicy(
    studentLimit: 2,
    employeeLimit: 5,
    penaltyPerLateDay: 15m);

var service = new UniversityRentalService(rentalPolicy);
IUniversityRentalService rentalApi = service;
IRentalDataStore dataStore = new JsonRentalStore();

var defaultDataPath = Path.Combine(Environment.CurrentDirectory, "rental-data.json");

if (args.Length > 0 && args[0].Equals("demo", StringComparison.OrdinalIgnoreCase))
{
    DemoScenario.Run(rentalApi);
    return;
}

Console.WriteLine("Interactive menu (default). Run with argument 'demo' for assignment scenario only.");
new InteractiveMenu(rentalApi, service, dataStore, defaultDataPath).Run();

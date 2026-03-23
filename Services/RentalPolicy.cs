using EquipmentRentalService.Domain;

namespace EquipmentRentalService.Services;

public sealed class RentalPolicy
{
    public RentalPolicy(int studentLimit, int employeeLimit, decimal penaltyPerLateDay)
    {
        StudentLimit = studentLimit;
        EmployeeLimit = employeeLimit;
        PenaltyPerLateDay = penaltyPerLateDay;
    }

    public int StudentLimit { get; }
    public int EmployeeLimit { get; }
    public decimal PenaltyPerLateDay { get; }

    public int GetUserLimit(UserType userType)
    {
        return userType switch
        {
            UserType.Student => StudentLimit,
            UserType.Employee => EmployeeLimit,
            _ => throw new ArgumentOutOfRangeException(nameof(userType), userType, "Unsupported user type.")
        };
    }

    public decimal CalculatePenalty(DateTime dueDate, DateTime returnDate)
    {
        var lateDays = (returnDate.Date - dueDate.Date).Days;
        if (lateDays <= 0)
        {
            return 0m;
        }

        return lateDays * PenaltyPerLateDay;
    }
}

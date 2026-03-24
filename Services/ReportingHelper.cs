using System.Text;
using EquipmentRentalService.Domain;

namespace EquipmentRentalService.Services;

public enum EquipmentListFilter
{
    All,
    Available,
    Rented,
    Unavailable
}

public enum RentalListFilter
{
    All,
    Active,
    Overdue,
    Closed
}

public static class ReportingHelper
{
    public static IReadOnlyList<Equipment> FilterEquipment(
        IUniversityRentalService service,
        EquipmentListFilter filter)
    {
        var all = service.GetAllEquipment();
        return filter switch
        {
            EquipmentListFilter.All => all.ToList(),
            EquipmentListFilter.Available => all.Where(e => e.Status == EquipmentStatus.Available).ToList(),
            EquipmentListFilter.Rented => all.Where(e => e.Status == EquipmentStatus.Rented).ToList(),
            EquipmentListFilter.Unavailable => all.Where(e => e.Status == EquipmentStatus.Unavailable).ToList(),
            _ => all.ToList()
        };
    }

    public static IReadOnlyList<Rental> FilterRentals(
        IUniversityRentalService service,
        RentalListFilter filter,
        DateTime now,
        int? userId = null)
    {
        IEnumerable<Rental> query = service.GetAllRentals();

        if (userId.HasValue)
        {
            query = query.Where(r => r.User.Id == userId.Value);
        }

        query = filter switch
        {
            RentalListFilter.All => query,
            RentalListFilter.Active => query.Where(r => r.IsActive),
            RentalListFilter.Overdue => query.Where(r => r.IsOverdue(now)),
            RentalListFilter.Closed => query.Where(r => !r.IsActive),
            _ => query
        };

        return query.ToList();
    }

    public static string FormatEquipmentLines(IEnumerable<Equipment> items)
    {
        var sb = new StringBuilder();
        foreach (var eq in items)
        {
            var reason = string.IsNullOrEmpty(eq.UnavailableReason) ? "" : $" ({eq.UnavailableReason})";
            sb.AppendLine($"#{eq.Id} | {eq.Name} | {eq.GetType().Name} | {eq.Status}{reason}");
        }

        return sb.Length == 0 ? "(none)\n" : sb.ToString();
    }

    public static string FormatRentalLines(IEnumerable<Rental> items)
    {
        var sb = new StringBuilder();
        foreach (var rental in items)
        {
            var returnInfo = rental.ActualReturnDate is null
                ? "not returned"
                : $"returned: {rental.ActualReturnDate:yyyy-MM-dd}, penalty: {rental.Penalty:C}";
            sb.AppendLine(
                $"#{rental.Id} | User #{rental.User.Id} {rental.User.FirstName} {rental.User.LastName} | " +
                $"Eq #{rental.Equipment.Id} {rental.Equipment.Name} | " +
                $"{rental.RentalDate:yyyy-MM-dd} .. {rental.DueDate:yyyy-MM-dd} | {returnInfo}");
        }

        return sb.Length == 0 ? "(none)\n" : sb.ToString();
    }

    public static string FormatFilteredSummary(
        IUniversityRentalService service,
        EquipmentListFilter equipmentFilter,
        RentalListFilter rentalFilter,
        DateTime now,
        int? userId = null)
    {
        var eq = FilterEquipment(service, equipmentFilter);
        var rent = FilterRentals(service, rentalFilter, now, userId);
        var sb = new StringBuilder();
        sb.AppendLine($"Equipment filter: {equipmentFilter} ({eq.Count} items)");
        sb.Append(FormatEquipmentLines(eq));
        sb.AppendLine();
        sb.AppendLine(
            $"Rental filter: {rentalFilter}" +
            (userId.HasValue ? $" (user #{userId})" : "") +
            $" ({rent.Count} items)");
        sb.Append(FormatRentalLines(rent));
        return sb.ToString();
    }
}

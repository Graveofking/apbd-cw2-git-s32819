using EquipmentRentalService.Services;

namespace EquipmentRentalService.Persistence;

public interface IRentalDataStore
{
    void Save(UniversityRentalService service, string path);
    void Load(UniversityRentalService service, string path);
}

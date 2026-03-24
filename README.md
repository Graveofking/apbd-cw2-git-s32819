//Doing it at night, was playing too much with C# didnt know it could be this fun.

University Equipment Rental Service
Console application in C# for handling university equipment rentals (registration, rental, return, availability control, and reports).

Had some problems with running it like usual so decided to do it by hand and console
How to run
From the repository root: with bash
dotnet run --project EquipmentRentalService/EquipmentRentalService.csproj


Interactive menu (default): starts the text menu; default save path is `rental-data.json` in the current working directory (you can override when saving/loading).
Assignment demo only (no menu):
dotnet run --project EquipmentRentalService/EquipmentRentalService.csproj -- demo


Implemented scope

- Common equipment model with specialized types:
  - `Laptop`
  - `Projector`
  - `Camera`
- Users:
  - `Student`
  - `Employee`
- Rental model with:
  - rental date
  - due date
  - actual return date
  - calculated penalty
- Business service:
  - `IUniversityRentalService` — application API (DIP: UI and demos depend on this)
  - `UniversityRentalService` — implementation
  - `RentalPolicy` — editable limits and penalty rule
- Interactive menu — `Ui/InteractiveMenu` (single class, loops and calls the service)
- JSON persistence — `Persistence/JsonRentalStore` + `IRentalDataStore`; IDs are preserved on load via domain rehydration constructors and `SynchronizeIdSequenceAfterLoad`
- Report filters — `ReportingHelper` with `EquipmentListFilter` and `RentalListFilter` (also used from menu item 14)

Functional requirements covered
1. Add user  
2. Add equipment  
3. List all equipment with status  
4. List available equipment  
5. Rent equipment  
6. Return equipment + late penalty  
7. Mark equipment unavailable  
8. Show active rentals for selected user  
9. Show overdue rentals  
10. Generate summary report  

Demonstration scenario (items 11–17 from the assignment) runs via:

- menu 15, or  
- CLI argument `demo`(see above).

Design decisions (cohesion, coupling, responsibilities)

Domain (`Domain/`): entities and simple state changes; rehydration constructors support JSON load without duplicating ID logic everywhere.
Services (`Services/`): orchestration (`UniversityRentalService`), rules (`RentalPolicy`), explicit failures (`OperationResult`), filtered/formatted reporting (`ReportingHelper`).
SubClass(`SubClass/`): JSON file format + mapping only; no business rules here.
UI** (`Ui/`): console menu and demo scenario; depends on `IUniversityRentalService`, not on domain internals.
- DIP:
  - `Program.cs` wires `IUniversityRentalService` and `IRentalDataStore` to concrete implementations.
  - `JsonRentalStore` needs `UniversityRentalService` for `Save`/`Load` because state replacement is `internal` on the concrete service (persistence stays a thin adapter).
- Failure handling: renting/returning/unavailable still use `OperationResult`; SubClass throws on missing/invalid file (clear failure).

Class organization rationale

Layers are split by reason to change: domain concepts, rental rules and workflows, file I/O, and console interaction. That keeps each class focused and avoids putting all logic in `Program.cs` or one “god” application class, without adding unnecessary extra types.

# Domain Layer

This layer contains the core entities, value objects, and domain rules for the application.

## Rules
- **Dependencies**: This layer depends ONLY on the `Shared` layer.
- **No External Libraries**: Absolutely NO references to Entity Framework Core, ASP.NET Core, or other infrastructural frameworks.
- **Organization**: Entities must be grouped by Module/Domain (e.g., `Products`, `Orders`).

## Usage
- Entities inherit from `BaseEntity`.
- Domain rules and validation that don't depend on external systems should be implemented inside the entities.

# Application Layer

This layer contains the business logic, use cases, and interfaces.

## Rules
- **Dependencies**: Depends ONLY on `Domain` and `Shared` layers.
- **CQRS & Vertical Slices**: Features are implemented using MediatR and organized into Vertical Slices (`Features/{Domain}/Commands` and `Features/{Domain}/Queries`).
- **Data Access**: Use `IApplicationDbContext` directly. NO Repository pattern is allowed.
- **Validation**: `ValidationBehavior` intercepts requests and uses FluentValidation automatically. Handlers do NOT perform explicit manual validation.

## Usage
- Each feature folder contains its Request, Handler, Validator, and Response DTO.
- Example: `Features/Products/Commands/CreateProduct/`

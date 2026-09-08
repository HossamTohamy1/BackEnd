# loxxking-backend-clean

This solution is a complete .NET 10 Web API built from the ground up utilizing **Strict Clean Architecture** and **CQRS with Vertical Slices**. 

## Architecture Overview
The project is strictly separated into the following layers, adhering strictly to the Dependency Inversion Principle. Dependencies flow inwards:

`Api -> Infrastructure -> Application -> Domain -> Shared`

1. **Shared**: The innermost zero-dependency layer. Contains generic building blocks like the `Result` pattern, the unified `ApiResponse` wrapper, and `Localization` resources.
2. **Domain**: The core business entities and rules. It depends only on `Shared`. No EF Core or ASP.NET references are allowed here.
3. **Application**: The business logic orchestration layer using MediatR. Features are organized by **Vertical Slices** (Requests, Handlers, Validators, Responses grouped by Feature, not by technical type). All validation is handled automatically via MediatR Pipeline `ValidationBehavior`. Data access is performed directly via `IApplicationDbContext` (no Repository pattern).
4. **Infrastructure**: Handles all external concerns: PostgreSQL database (EF Core), Identity, JWT + Cookie Smart Authentication Scheme, and SignalR. Depends on `Application` and `Shared`.
5. **Api**: The presentation layer. Contains extremely thin controllers that only dispatch commands/queries to MediatR and return standard `ApiResponse<T>` objects via `ResultExtensions`. No business logic allowed here.

## Mandatory Architectural Rules
1. **Result Pattern**: Expected failures (e.g., validation, not found) must return `Result.Failure(Error)`. Exceptions are reserved for truly unexpected errors.
2. **No Repository Pattern**: CQRS Handlers depend directly on `IApplicationDbContext`.
3. **Automatic Validation**: You never manually validate inputs inside Handlers. Define a FluentValidator for the Request, and the MediatR `ValidationBehavior` will intercept it and throw a `ValidationException` (caught globally by `ValidationExceptionHandler`) if it fails.
4. **Vertical Slices**: All files for a single feature (Command, Handler, Validator) must live together in a single folder under `Application/Features/{Domain}/`.
5. **No Code Comments**: Self-documenting code is mandatory. Use descriptive names instead of `//` or `///`.
6. **No Includes by Default**: EF Core `Include()` is strictly forbidden unless absolutely necessary. Data must be projected directly into DTOs using `.Select()` to prevent N+1 issues and over-fetching.
7. **Localization**: Error messages and responses must use `SharedResource` (English and Arabic) injected into validators.

## Project Setup
- Built on **.NET 10**.
- Database: **PostgreSQL**.
- Authentication: **JWT + Cookie Smart Scheme**.

See the `README.md` inside each specific project for layer-specific rules.

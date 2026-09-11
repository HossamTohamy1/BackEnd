`Api -> Infrastructure -> Application -> Domain -> Shared`
1. **Shared**: The innermost zero-dependency layer. Contains generic building blocks like the `Result` pattern, the unified `ApiResponse` wrapper, and `Localization` resources.
2. **Domain**: The core business entities and rules. It depends only on `Shared`. No EF Core or ASP.NET references are allowed here.
3. **Application**: The business logic orchestration layer using MediatR. Features are organized by **Vertical Slices** (Requests, Handlers, Validators, Responses grouped by Feature, not by technical type). All validation is handled automatically via MediatR Pipeline `ValidationBehavior`. Data access is performed directly via `IApplicationDbContext` (no Repository pattern).
4. **Infrastructure**: Handles all external concerns: Microsoft SQL Server database (EF Core), ASP.NET Identity, JWT + Cookie Smart Authentication Scheme, SignalR Hubs, File Storage, and Background Sync Services for Legacy CRM (Orders & Support Chat). Depends on `Application` and `Shared`.
5. **Api**: The presentation layer. Contains thin controllers that dispatch commands/queries to MediatR and return standard `ApiResponse<T>` objects via `ResultExtensions`. Handles Rate Limiting, CORS, and incoming webhooks/sync endpoints from CRM.
## Mandatory Architectural Rules
1. **Result Pattern**: Expected failures (e.g., validation, not found) must return `Result.Failure(Error)`. Exceptions are reserved for truly unexpected errors.
2. **No Repository Pattern**: CQRS Handlers depend directly on `IApplicationDbContext`.
3. **Automatic Validation**: You never manually validate inputs inside Handlers. Define a FluentValidator for the Request, and the MediatR `ValidationBehavior` will intercept it and throw a `ValidationException` (caught globally by `ValidationExceptionHandler`) if it fails.
4. **Vertical Slices**: All files for a single feature (Command, Handler, Validator) must live together in a single folder under `Application/Features/{Domain}/`.
5. **No Code Comments**: Self-documenting code is mandatory. Use descriptive names instead of `//` or `///`.
6. **No Includes by Default**: EF Core `Include()` is strictly forbidden unless absolutely necessary. Data must be projected directly into DTOs using `.Select()` to prevent N+1 issues and over-fetching.
7. **Localization**: Error messages and responses must use `SharedResource` (English and Arabic) injected into validators.
## Core Integrations & Features
- **Database**: **Microsoft SQL Server** (EF Core 10).
- **Authentication**: **SmartScheme** dynamically routing between **JWT Bearer** (Authorization header) and **Cookie Authentication** (`.Loxxking.Session`).
- **SSO (Single Sign-On)**: Seamless authentication with Luxira CRM via RSA public key signature verification and memory-cached JTI replay attack prevention.
- **Real-time Support Chat & SignalR**:
  - Live WebSocket chat at `/chatHub` supporting visitors, guests, authenticated customers, and CRM staff replies.
  - Multi-tenant guest session tracking via `X-Guest-Id` and `userTag`.
  - Media & voice messaging support (`chat/audio`, `chat/images`).
- **Legacy CRM Integration (Luxira CRM)**:
  - `OrderSyncBackgroundService`: Asynchronously pushes store orders to CRM.
  - `VisitorChatSyncBackgroundService`: Asynchronously syncs store visitor messages to CRM.
  - `IncomingFromCrm` Endpoint: Receives replies from CRM employees and broadcasts them to the storefront visitor in real time.
- **Invoicing & Export**: Dynamic invoice generation via `QuestPDF`.
## Project Setup & Running
- **SDK**: .NET 10.
- **Build**:
  ```bash
  dotnet build
  ```
- **Run**:
  ```bash
  cd loxxking-backend-clean.Api
  dotnet run
  ```
- **Configuration**: Ensure `ConnectionStrings:DefaultConnection` and `LegacyCrm` settings are configured in `appsettings.json`.
See the `README.md` inside each specific project for layer-specific rules.

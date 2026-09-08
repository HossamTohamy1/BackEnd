# Shared Layer

This layer contains shared building blocks and contracts that are used across all other layers in the solution. 

## Rules
- **Zero Dependencies**: This layer MUST NOT depend on any other layer in the solution (`Domain`, `Application`, `Infrastructure`, `Api`).
- **No Business Logic**: It should only contain generic structures (e.g., Result Pattern, Error responses) and base elements (e.g., Localization resources).
- **No External Libraries (generally)**: Keep it pure C# as much as possible, except for basic abstractions (like Localization abstractions).

## Usage
- `Result` and `Error`: Used to implement the Result pattern across the Application layer instead of throwing exceptions for expected validations and business rules.
- `ApiResponse<T>`: Used to wrap API responses uniformly.
- `SharedResource`: Central place for multilingual string definitions.

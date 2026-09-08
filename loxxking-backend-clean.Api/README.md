# Api Layer

This layer is responsible for receiving HTTP requests, converting them into MediatR Commands/Queries, and formatting the response.

## Rules
- **Dependencies**: Depends ONLY on `Infrastructure` and `Shared`.
- **Very Thin Controllers**: Controllers must NOT contain any business logic or Entity Framework code. They only send requests to MediatR and return the result using `ToApiResponse()`.
- **Response Format**: All responses must use the `ApiResponse<T>` unified wrapper.
- **Error Handling**: Do not use `try/catch` in controllers. Validation exceptions are handled globally via `ValidationExceptionHandler`. Expected failures are returned via the Result pattern.

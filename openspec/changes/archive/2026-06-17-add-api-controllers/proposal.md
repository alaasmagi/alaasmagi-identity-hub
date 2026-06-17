## Why

The authentication service has Application-layer behavior for auth, consent, two-factor, external auth, and admin operations, but it still needs the Web API surface that exposes those services consistently. This change defines the controller layer, Swagger documentation, and authentication scheme wiring needed for API clients and admin API endpoints.

## What Changes

- Add thin ASP.NET Core Web API controllers for `/api/auth/*` authentication flows.
- Add thin controllers for two-factor authentication, consent, external authentication, and admin management endpoints.
- Add a shared `ApiControllerBase` result mapper so `Result<T>` failures produce consistent HTTP status codes and error payloads.
- Configure Swagger/OpenAPI with XML comments, JWT bearer authorization support, and the "Auth Service API" v1 document.
- Configure side-by-side JWT bearer, admin cookie, Google, and Microsoft authentication schemes in the Web project.
- Ensure API controllers extract authenticated user/admin ids, IP address, and user agent from the HTTP context instead of trusting request bodies.
- Preserve the existing Application-layer business logic boundary: controllers only dispatch to injected services and map responses.

## Capabilities

### New Capabilities
- `identity-api-controllers`: API controller routes, HTTP response mapping, authentication attributes, external challenge/callback behavior, Swagger documentation, and Web authentication wiring for the centralized identity service.

### Modified Capabilities

## Impact

- Affected project: `Web`.
- New files under `Web/Controllers` and, where needed, `Web/Contracts/Requests` and `Web/Contracts/Responses`.
- Updates to `Web/Program.cs` for authentication, authorization, Swagger, and Application service DI wiring.
- Updates to `Web/Web.csproj` to generate XML documentation consumed by Swagger.
- API surface added for auth, 2FA, consent, external auth, and admin operations; no repository, Identity manager, infrastructure, Razor Page, or business-logic implementation changes.

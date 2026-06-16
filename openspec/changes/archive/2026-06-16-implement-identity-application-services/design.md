## Context

The solution is a Clean Architecture .NET identity service with Domain entities, repository contracts, and ASP.NET Identity infrastructure in place. The Application project is currently the correct boundary for coordinating business workflows, but it has no service contracts, DTOs, validators, result model, or DI registration for the required authentication flows.

The implementation must remain Application-layer only: no controllers, Razor Pages, repository CRUD implementations, migrations, or infrastructure provider code. The services must use existing repositories for entity operations and ASP.NET Identity managers for identity-specific operations such as password verification, password reset, authenticator tokens, recovery codes, external login info, and user token storage.

## Goals / Non-Goals

**Goals:**

- Add plain scoped services with interfaces for Auth, Consent, TwoFactor, ExternalAuth, and Admin topics.
- Add request/response DTOs, FluentValidation validators, `Result<T>`, `Unit`, and shared helpers required by those services.
- Enforce client-scoped authentication and access by resolving `Client` records by `Client.ClientId`, not entity `Id`.
- Store refresh tokens, temporary 2FA tokens, and consent tokens through `UserManager.SetAuthenticationTokenAsync`.
- Build roles dynamically for token issuance by filtering Identity roles to `AppRole.ClientId == requesting client.ClientId`.
- Log security events through `ISecurityEventService` at the end of authentication-related flows.
- Register all Application services and validators through DI.

**Non-Goals:**

- Do not implement repository classes or alter their persistence model.
- Do not implement Web controllers, Razor Pages, external provider challenge endpoints, or admin UI pages.
- Do not persist claims through `AspNetUserClaims` or `AspNetRoleClaims`.
- Do not introduce MediatR, CQRS handlers, or presentation-layer business logic.
- Do not change Domain entities unless compilation proves a small compatibility adjustment is required.

## Decisions

1. Use topic services instead of handlers.

   Each capability maps to a small interface and implementation class: `IAuthService`, `IConsentService`, `ITwoFactorService`, `IExternalAuthService`, and `IAdminService`. This matches the requested Application-layer style and keeps controllers/Razor Pages able to call business operations directly.

   Alternative considered: MediatR commands and queries. Rejected because the requested shape explicitly requires plain service classes with interfaces.

2. Keep DTOs local to service folders and shared models under `Application/Common`.

   Each service folder owns its request, response, and validator files. Shared primitives such as `Result<T>`, `Unit`, `PagedResponse<T>`, and token provider constants live under `Application/Common`.

   Alternative considered: put DTOs in the existing `DTO` project. Rejected because these are Application service contracts rather than persistence or Web transport mappings.

3. Validate inline at service boundaries.

   Service constructors receive the relevant `IValidator<T>` instances, and each service method validates its request before doing business work. Validation failures return `Result<T>.Failure(...)` rather than throwing.

   Alternative considered: validation decorators. Rejected for this change because no existing decorator pipeline is wired.

4. Centralize repeated post-authentication behavior.

   Auth, 2FA, consent, and external callback flows all need the same client access checks, consent-token generation, active/pending/revoked handling, client-filtered role lookup, and token issuance. The implementation should use private helper methods or small internal shared services inside the Application layer to avoid divergent behavior while keeping public services topic-focused.

   Alternative considered: duplicate the checks in each public method. Rejected because subtle differences in consent and role filtering would be security-sensitive.

5. Treat repository query gaps as implementation constraints.

   The repository interfaces currently expose generic CRUD only. The implementation should first use available base repository query methods if exposed by referenced packages; if those are insufficient, add narrow query methods to repository interfaces only where necessary, leaving repository implementations for a separate layer/task if they are not already supported.

   Alternative considered: reference `AppDbContext` from Application. Rejected because it would violate the Application/DataAccess boundary.

6. Return claims as Application response data for external MVC services.

   `IExternalAuthService.ExchangeAuthCodeAsync` should validate client secret and auth-code payload, then return claim name/value data sufficient for an external MVC application to build its own cookie. The auth service must not share its admin cookie with external services.

   Alternative considered: issue an auth-service cookie during exchange. Rejected because external MVC services must own their own local cookie.

## Risks / Trade-offs

- Repository base contracts may not expose filtering by email, client id, or user-client pairs -> verify available methods during implementation and add narrowly scoped repository contract methods only if unavoidable.
- `ITokenService.ValidateAuthCode` validates but may not consume auth codes -> if no consume operation exists, document or add an Application-side one-time-use guard only if supported by available token storage.
- Multiple refresh tokens per user/client in `AspNetUserTokens` need deterministic token names -> define constants and naming conventions early to support rotation and revocation.
- Assigning a "default role" is underspecified -> use a conservative lookup by normalized role name such as `Default` scoped to the client only if it exists; otherwise continue without role assignment.
- `ResponseType = "cookie"` still returns Application-layer data only -> presentation code remains responsible for actual cookie sign-in because this change excludes Web implementation.
- Security-event logging can fail independently of business success -> keep logging through the provided service and return business results; do not use exceptions for business failures.

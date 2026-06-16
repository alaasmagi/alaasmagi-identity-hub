## Why

The identity hub has the domain model, repositories, and infrastructure contracts needed for authentication, but it lacks the Application-layer business services that coordinate ASP.NET Identity, client consent, 2FA, external login, token issuance, and admin management. Implementing this layer creates the central contract used by Web/API entry points without mixing business logic into controllers, Razor Pages, or infrastructure.

## What Changes

- Add focused Application services and interfaces for authentication, consent, two-factor authentication, external MVC federation, and admin management.
- Add request/response DTOs, FluentValidation validators, and a shared `Result<T>` / `Unit` result model under the Application project.
- Coordinate `UserManager<AppUser>`, `RoleManager<AppRole>`, `SignInManager<AppUser>`, repositories, `ITokenService`, `IEmailService`, and `ISecurityEventService` from plain scoped services.
- Implement client-scoped login, refresh token rotation, logout, email confirmation, password reset, password change, consent granting/revocation, 2FA setup/login/recovery, external auth-code exchange, and admin user/client/security-event operations.
- Preserve the rule that claims are built dynamically at token generation or auth-code exchange time and are never persisted through ASP.NET Identity claim tables.
- Register each service as scoped through interface and implementation without introducing MediatR or presentation-layer concerns.

## Capabilities

### New Capabilities

- `identity-application-services`: Application-layer authentication, consent, two-factor, external federation, and admin-management business services for the centralized ASP.NET Identity provider.

### Modified Capabilities

- None.

## Impact

- Affected code: `Application` project service folders, shared Application common types, DI registration, request/response DTOs, and validators.
- Dependencies: uses existing Domain entities/enums, repository interfaces in `Contracts`, ASP.NET Core Identity managers, FluentValidation, and existing infrastructure interfaces.
- API/Web impact: controllers and Razor Pages can call the new services, but this change does not implement controllers, Razor Pages, repository CRUD, or infrastructure providers.
- Security impact: enforces client-scoped access, dynamic runtime claims, client-secret validation against `ClientSecretHash`, refresh/temp/consent token storage in `AspNetUserTokens`, and security-event logging for authentication-related actions.

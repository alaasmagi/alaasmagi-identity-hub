## Context

The Web project currently hosts the ASP.NET Core shell, Identity setup, MVC routing, and Razor Page support, but it does not expose the centralized authentication service through API controllers. The Application project already contains the business-facing service interfaces and implementations for authentication, consent, two-factor authentication, external authentication, and admin workflows, plus `Result<T>` for expected business failures.

This change adds the API presentation layer only. Controllers will live in `Web`, call Application services through constructor-injected interfaces, and map service results to HTTP responses. Admin Razor Pages and external login UI pages remain separate concerns.

## Goals / Non-Goals

**Goals:**

- Add `/api/auth/*` and `/api/admin/*` Web API endpoints matching the requested route surface.
- Keep controllers thin: bind route/query/body values, read authenticated claims and HTTP metadata, call Application services, and return HTTP responses.
- Centralize `Result<T>` to HTTP mapping in `ApiControllerBase`.
- Configure JWT bearer authentication for protected API endpoints and cookie authentication with Admin role enforcement for admin endpoints.
- Configure Swagger/OpenAPI with XML comments, response metadata, tags, and bearer-token support.
- Generate XML documentation for the Web project and document any Web-specific request or response contracts.

**Non-Goals:**

- No Application service, repository, Identity manager, infrastructure service, or database behavior changes.
- No Razor Pages, views, admin UI, login UI, or consent UI implementation.
- No direct repository, `UserManager`, `RoleManager`, or `SignInManager` access from controllers, except external challenge/callback integration where ASP.NET Core external authentication primitives are required.
- No logging of client secrets or echoing secrets in API responses.

## Decisions

1. Use `ApiControllerBase.HandleResult<T>` for all standard service responses.

   Rationale: `Result<T>` already represents expected business outcomes. A shared protected helper keeps controllers consistent and prevents each action from duplicating error-to-status-code mapping.

   Alternative considered: action-specific `switch` blocks in each controller. This was rejected because it increases drift and makes future error mapping changes harder.

2. Reference Application DTOs directly when their contract matches the API payload; introduce Web request DTOs only when the API shape differs.

   Rationale: The existing Application request records are identical for many public payloads. For authenticated operations, the API body intentionally excludes `UserId` or `AdminUserId`; Web-specific DTOs should be created for those bodies and mapped to Application requests after extracting ids from claims.

   Alternative considered: duplicate every request and response contract in `Web/Contracts`. This was rejected because it creates avoidable mapping code and duplicate XML documentation for identical contracts.

3. Extract user and admin ids exclusively from `ClaimTypes.NameIdentifier`.

   Rationale: Authenticated API and admin operations must not trust caller-supplied ids for the acting principal. Controllers will return unauthorized when the claim is absent or cannot parse as a `Guid`.

   Alternative considered: accepting acting ids in request bodies. This was rejected for security reasons and conflicts with the controller rules.

4. Use JWT and cookie authentication side by side with explicit controller/action schemes.

   Rationale: API clients authenticate with JWT bearer tokens while the internal admin surface uses the auth service cookie and Admin role. Explicit `[Authorize(AuthenticationSchemes = ...)]` attributes avoid accidental use of the default scheme.

   Alternative considered: one default scheme for all protected endpoints. This was rejected because API and admin surfaces have different trust and session models.

5. Use the existing `Application.DependencyInjection.AddApplication()` extension for service registration.

   Rationale: The Application project already owns its service and validator registrations. Calling the extension from `Program.cs` keeps Web startup focused on presentation concerns while still registering `IAuthService`, `IConsentService`, `ITwoFactorService`, `IExternalAuthService`, and `IAdminService` for controller injection.

   Alternative considered: registering each Application service manually in `Program.cs`. This is acceptable but duplicates existing composition-root knowledge.

6. Implement external provider challenge/callback using ASP.NET Core authentication primitives at the controller boundary.

   Rationale: A browser redirect to Google or Microsoft must return a `ChallengeResult`; the callback must obtain external login information from the ASP.NET Core authentication stack before dispatching to `IExternalAuthService`. The controller remains a presentation adapter and delegates business validation and auth-code generation to the Application service.

   Alternative considered: moving `ChallengeResult` construction into Application. This was rejected because it would couple Application to MVC result types.

7. Document API actions and schemas through XML comments and Swashbuckle attributes.

   Rationale: Swagger is part of the API contract for this service. XML comments on actions, parameters, and Web-specific DTOs make the generated schema usable, while `[ProducesResponseType]` attributes declare expected status codes.

   Alternative considered: relying on inferred OpenAPI metadata only. This was rejected because inferred docs will miss business failure status codes and schema descriptions.

## Risks / Trade-offs

- [Risk] Existing Application record DTOs do not have XML comments, but the Swagger rules require schema comments for DTOs. -> Mitigation: add XML comments to Application DTOs that are reused directly, and add XML comments to any new Web-specific DTOs.
- [Risk] The Web project currently references DataAccess only. -> Mitigation: add a Web-to-Application project reference and any required authentication/Swagger package references during implementation.
- [Risk] JWT token validation parameters depend on configuration keys that may not exist yet. -> Mitigation: wire validation from configuration with clear option names and fail fast only for required runtime values.
- [Risk] External callback behavior depends on the current external authentication setup and available login info APIs. -> Mitigation: keep provider challenge/callback code isolated in `ExternalController` and delegate all business validation to `IExternalAuthService`.
- [Risk] `Result<Unit>` success responses do not need a payload. -> Mitigation: the base helper should return `Ok()` when the result value is `Unit` or otherwise intentionally empty, while preserving `Ok(value)` for data responses.

## Context

The service already has Domain entities, Application services, API controllers, Identity infrastructure, and a single `AppDbContext`. The missing layer is the Razor Pages UI that lets browser users complete the same auth workflows as the API and lets auth-service administrators manage users, clients, roles, and security events.

There are two separate page groups. Federated account pages under `Web/Areas/Identity/Pages/Account` are entry points from external MVC client services and must preserve `clientId`, `redirectUri`, and temporary flow tokens across redirects and form posts. Admin pages under `Web/Areas/Admin/Pages` are internal management pages protected by the auth-service cookie and `Admin` role, and must not carry federated login state.

The implementation should start from ASP.NET Core Identity and Razor Pages scaffolding where available, then adapt PageModels to call `IAuthService`, `IConsentService`, `ITwoFactorService`, `IExternalAuthService`, and `IAdminService` directly. The default Bootstrap scaffolded visual style remains the UI baseline.

## Goals / Non-Goals

**Goals:**
- Implement account Razor Pages for local login, registration, 2FA, consent, external login, password reset, email confirmation, password change, and 2FA management.
- Implement admin Razor Pages for users, user-client approvals, clients, roles, and security events.
- Preserve federated login context through URLs and hidden fields at every form boundary.
- Redirect final successful federated flows to the originating `redirectUri` with an auth code.
- Use Application services for business workflows and use direct EF Core only for scaffolded CRUD where no matching Application service method exists.
- Keep markup and validation conventions aligned with default ASP.NET Core Identity Bootstrap scaffolding.

**Non-Goals:**
- Do not redesign the Application service layer, API controllers, token service, or domain model as part of the UI work.
- Do not make external MVC services exchange auth codes inside these Razor Pages.
- Do not introduce a custom UI framework or custom design system.
- Do not store federated flow state only in TempData or session.
- Do not expose `ClientSecretHash`, password hashes, raw flow tokens, or generated secrets except in explicit one-time reveal screens.

## Decisions

1. **Use scaffolded pages as the baseline.**
   - Decision: Generate standard Identity pages and Razor Pages CRUD scaffolds first, then adapt them.
   - Rationale: The scaffolded pages already handle the standard mechanics, validation shape, Bootstrap markup, and expected file structure.
   - Alternative considered: Hand-write all pages. This would increase risk of missing Identity UI conventions and duplicating framework behavior.

2. **Keep PageModels thin and service-driven.**
   - Decision: Account PageModels construct Application request DTOs and map `Result<T>` outcomes into page validation errors, internal `RedirectToPage`, or final external `Redirect`.
   - Rationale: Business rules already live in Application services and API controllers follow the same pattern.
   - Alternative considered: Calling API controllers or making HTTP calls back into the same app. That adds unnecessary serialization, routing, and authentication complexity.

3. **Treat federated login state as explicit form/query state.**
   - Decision: Bind `clientId`, `redirectUri`, `tempToken`, `consentToken`, and `returnUrl` with `SupportsGet = true` where needed and render them as hidden fields on every posting form.
   - Rationale: The flow must survive refreshes, back-button usage, and reopening login pages without relying only on server-side session state.
   - Alternative considered: TempData/session-only flow state. That is fragile for multi-tab login and contradicts the external redirect contract.

4. **Use a shared redirect/result mapping helper for account flows.**
   - Decision: Implement page-local or shared Web helpers that map login-like results to `LoginWith2fa`, `Consent`, validation errors, waiting pages, or final external redirect.
   - Rationale: Login, 2FA, recovery-code, consent, and external callback pages all branch on the same concepts. A focused helper reduces duplicated branching without moving business logic into Web.
   - Alternative considered: Duplicate branching in each PageModel. That is simpler initially but increases the chance of inconsistent handling.

5. **Protect admin pages through a shared base or area convention.**
   - Decision: Apply cookie authentication and `Admin` role authorization once for the admin area, preferably through a shared `AdminPageModel` base class or Razor Pages authorization convention.
   - Rationale: This avoids repeated attributes and keeps admin access rules consistent.
   - Alternative considered: Repeat `[Authorize]` on every PageModel. That is easy to drift as pages are added.

6. **Hash and reveal client secrets only at creation/regeneration boundaries.**
   - Decision: Client create and regenerate-secret flows generate a plaintext secret, persist only `ClientSecretHash`, and render the plaintext secret exactly once.
   - Rationale: Admin usability requires the one-time value, while the persisted application state must not expose recoverable secrets.
   - Alternative considered: Showing or editing `ClientSecretHash`. That leaks implementation details and creates security risk.

7. **Resolve the auth-code response contract before implementing final redirects.**
   - Decision: PageModels must redirect with an auth code returned by the Application workflow for cookie/federated responses; they must not convert JWT access tokens into auth codes themselves.
   - Rationale: Auth-code generation and validation are part of the existing token workflow and should remain in Application/Infrastructure.
   - Alternative considered: Generating codes in PageModels through lower-level services. That would move auth business logic into the UI layer.

## Risks / Trade-offs

- **Current response DTO mismatch** -> The existing `LoginResponse` and `ConsentResponse` currently expose access/refresh tokens, while final browser redirects require an auth code. During implementation, confirm whether cookie response support exists elsewhere; if not, add or expose the existing auth-code output through the Application service contract before wiring redirects.
- **Scaffolding may generate direct Identity manager usage** -> Adapt generated PageModels immediately so business decisions go through Application services.
- **Admin CRUD may need limited direct `AppDbContext` access** -> Restrict direct EF Core use to generated Client, Role, and read-only SecurityEvent CRUD pages where the Application service has no matching command.
- **External redirect misuse** -> Validate that `redirectUri` is non-empty before final redirects and rely on Application services/token exchange to validate allowed origins.
- **Secret leakage in generated CRUD views** -> Remove `ClientSecretHash` from forms, tables, validation output, and display templates; use explicit one-time secret view models.
- **UI drift from scaffolded Identity style** -> Keep Bootstrap `form-floating`, validation summaries, `btn-primary`, and container/row/column conventions as acceptance criteria.

## Migration Plan

1. Generate Identity and admin CRUD scaffolding into the existing `Web` project.
2. Adapt generated account PageModels to Application services and explicit federated state threading.
3. Add the new Consent page and admin-only pages that scaffolding does not provide.
4. Add admin layout/navigation and shared authorization.
5. Build and run focused page/handler tests where feasible, plus a full solution build.
6. Rollback strategy: remove the new/changed Razor Pages files and route conventions; existing API controllers and Application services remain intact.

## Open Questions

- Which concrete property or response type currently carries the auth code for `IAuthService.LoginAsync(... ResponseType = "cookie")`, `ITwoFactorService` login continuation, and `IConsentService.GrantConsentAsync`? The visible DTOs expose tokens today, so implementation must reconcile this before final redirect handlers are completed.
- Is there an existing lightweight client lookup service/DTO for displaying the client name on Login/Register, or should the UI use a minimal Application/Admin method rather than direct `AppDbContext` access?

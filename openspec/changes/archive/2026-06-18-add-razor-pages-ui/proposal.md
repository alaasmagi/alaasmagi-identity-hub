## Why

The authentication service has implemented Application services and API controllers, but it still lacks the Razor Pages user interface needed for browser-based external MVC login flows and internal administrator workflows. This change adds the page layer so users can complete login, 2FA, consent, registration, password, external-provider, and account-management flows in a standard ASP.NET Core Identity UI while administrators can manage users, clients, roles, and security events inside the auth service.

## What Changes

- Add scaffolded-and-adapted ASP.NET Core Identity Razor Pages under `Web/Areas/Identity/Pages/Account`.
- Thread `clientId`, `redirectUri`, and flow tokens through federated login pages via query-bound properties and hidden form fields.
- Route local login, registration, 2FA, consent, password, email-confirmation, change-password, and external-login pages through existing Application service interfaces.
- Add a consent page that renders client context and redirects back to external MVC services with either an auth code or access-denied error.
- Add admin Razor Pages under `Web/Areas/Admin/Pages` for users, user-client access approvals, client CRUD, roles CRUD, and security-event review.
- Protect admin pages with cookie authentication and the `Admin` role, without mixing admin workflows with federated `clientId`/`redirectUri` state.
- Keep the UI visually aligned with default ASP.NET Core Identity Bootstrap scaffolded pages.

## Capabilities

### New Capabilities
- `razor-pages-ui`: Razor Pages account and admin UI behavior for federated MVC authentication and internal auth-service administration.

### Modified Capabilities
- None.

## Impact

- Affected code: `Web/Areas/Identity/Pages/Account/**`, `Web/Areas/Identity/Pages/Account/Manage/**`, `Web/Areas/Admin/Pages/**`, and Web startup/page routing as needed.
- Dependencies: ASP.NET Core Identity scaffolded Razor Pages and Razor Pages CRUD scaffolding are used as the baseline where available.
- Existing APIs and Application service contracts remain unchanged.
- Security impact: auth-code redirects, client context preservation, one-time secret display, admin cookie authorization, and avoidance of secret/hash/token exposure in rendered HTML.

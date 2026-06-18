## 1. Scaffolding and Flow Foundation

- [x] 1.1 Confirm or expose the Application response field used by cookie/federated flows to return an auth code from login, 2FA, recovery-code, and consent success paths.
- [x] 1.2 Run Identity scaffold generation for the required `Areas/Identity/Pages/Account` and `Manage` pages against `AppDbContext`.
- [x] 1.3 Run Razor Pages CRUD scaffold generation for `Client`, `AppRole`, and `SecurityEvent` against `AppDbContext` into `Areas/Admin/Pages`.
- [x] 1.4 Add shared account-flow helpers or PageModel utilities for preserving `clientId`, `redirectUri`, flow tokens, mapping login-style results, appending query parameters, and handling missing final `redirectUri`.
- [x] 1.5 Add any required Razor Pages route conventions, area services, imports, and validation partial wiring while keeping existing Web project structure.

## 2. Federated Account Pages

- [x] 2.1 Adapt `Login.cshtml` and `Login.cshtml.cs` to bind and post `clientId` and `redirectUri`, call `IAuthService.LoginAsync` with response type `cookie`, render client context, show provider buttons, and branch to 2FA, consent, inline errors, or final auth-code redirect.
- [x] 2.2 Adapt `LoginWith2fa.cshtml` and `LoginWith2fa.cshtml.cs` to bind and post `clientId`, `redirectUri`, and `tempToken`, call `ITwoFactorService.LoginWithTwoFactorAsync`, and preserve the recovery-code link context.
- [x] 2.3 Adapt `LoginWithRecoveryCode.cshtml` and `LoginWithRecoveryCode.cshtml.cs` to bind and post `clientId`, `redirectUri`, and `tempToken`, call `ITwoFactorService.LoginWithRecoveryCodeAsync`, and use the shared login-result branching.
- [x] 2.4 Add `Consent.cshtml` and `Consent.cshtml.cs` to load consent info, render client/registration context, approve through `IConsentService.GrantConsentAsync`, render pending/not-invited states, and deny with `error=access_denied`.
- [x] 2.5 Adapt `Register.cshtml` and `Register.cshtml.cs` to collect `FullName`, call `IAuthService.RegisterAsync`, preserve `clientId` and `redirectUri`, show external provider buttons, and link confirmation back to Login with the same context.
- [x] 2.6 Adapt `ForgotPassword.cshtml`, `ForgotPasswordConfirmation.cshtml`, `ResetPassword.cshtml`, `ResetPasswordConfirmation.cshtml`, and `ConfirmEmail.cshtml` to call the corresponding `IAuthService` methods and keep scaffolded validation behavior.
- [x] 2.7 Adapt `ExternalLogin.cshtml` and callback handlers to preserve provider context, call `IExternalAuthService.HandleExternalCallbackAsync`, branch to consent when needed, and redirect to `redirectUri` with an auth code on success.

## 3. Account Manage Pages

- [x] 3.1 Adapt `Manage/ChangePassword.cshtml` and PageModel to require an authenticated cookie principal, read the user id from claims, and call `IAuthService.ChangePasswordAsync`.
- [x] 3.2 Adapt `Manage/TwoFactorAuthentication.cshtml` to call `ITwoFactorService.GetStatusAsync` and render the scaffolded status/actions.
- [x] 3.3 Adapt `Manage/EnableAuthenticator.cshtml` to call `SetupAuthenticatorAsync` on GET and `EnableTwoFactorAsync` on POST, then render one-time recovery codes.
- [x] 3.4 Adapt `Manage/GenerateRecoveryCodes.cshtml` to call `RegenerateRecoveryCodesAsync` and render the one-time recovery-code reveal.
- [x] 3.5 Adapt `Manage/Disable2fa.cshtml` to call `DisableTwoFactorAsync` and preserve scaffolded confirmation/validation behavior.

## 4. Admin Area Structure and Authorization

- [x] 4.1 Add shared admin authorization through an admin PageModel base class or Razor Pages convention using cookie authentication and the `Admin` role.
- [x] 4.2 Add `/Areas/Admin/Pages/_ViewImports.cshtml`, `_ViewStart.cshtml` as needed, and `_Layout.cshtml` with Bootstrap navigation for Users, Clients, Roles, and Security Events.
- [x] 4.3 Ensure admin pages do not carry federated `clientId`, `redirectUri`, `tempToken`, or `consentToken` fields or query state.

## 5. Admin User and Security Event Pages

- [x] 5.1 Implement `/Areas/Admin/Pages/Users/Index` to call `IAdminService.GetUsersAsync` with paging/search and render FullName, Email, IsActive, IsBanned, CreatedAt when available, and row actions.
- [x] 5.2 Implement `/Areas/Admin/Pages/Users/Clients` to call `IAdminService.GetUserClientsAsync`, render user-client rows, and approve pending access through `IAdminService.ApproveUserClientAsync`.
- [x] 5.3 Implement `/Areas/Admin/Pages/Users/Ban` to collect a reason for ban, support unban confirmation, and call `BanUserAsync` or `UnbanUserAsync` with the admin user id from claims.
- [x] 5.4 Adapt `/Areas/Admin/Pages/SecurityEvents/Index` to call `IAdminService.GetSecurityEventsAsync` with optional user/client filters and paging.
- [x] 5.5 Keep SecurityEvent admin pages read-only by removing or not exposing Create, Edit, and Delete pages.

## 6. Admin Client and Role CRUD

- [x] 6.1 Adapt Client Index, Details, Create, Edit, and Delete pages to remove all display and binding of `ClientSecretHash`.
- [x] 6.2 Implement Client Create server-side generation of `ClientId`, plaintext client secret, stored `ClientSecretHash`, and one-time secret confirmation display.
- [x] 6.3 Implement Client Edit fields for name, active state, `RegistrationType` dropdown, and comma-separated `AllowedOrigins`, without secret hash editing.
- [x] 6.4 Add Client Regenerate Secret confirmation/action that creates a new plaintext secret, overwrites only the stored hash, and renders the new secret exactly once.
- [x] 6.5 Adapt Role Index, Details, Create, Edit, and Delete pages for `AppRole`, including a Client dropdown populated from `Client.Name` and `Client.ClientId`.

## 7. UI Conventions and Security Review

- [x] 7.1 Verify account pages retain scaffolded Bootstrap Identity markup conventions including `form-floating`, tag helpers, validation summaries, `btn-primary`, and `_ValidationScriptsPartial`.
- [x] 7.2 Verify no rendered page displays password hashes, `ClientSecretHash`, raw flow tokens, or generated secrets outside the explicit one-time secret/recovery-code reveal screens.
- [x] 7.3 Verify all internal page-to-page transitions use `RedirectToPage` and only final external handoffs use `Redirect`.
- [x] 7.4 Verify PageModels are public, use nullable-reference-safe properties, and avoid direct `AppDbContext` injection except for allowed scaffolded CRUD gaps.

## 8. Verification

- [x] 8.1 Build the solution and fix Razor compilation, nullable, DI, and namespace errors.
- [x] 8.2 Run existing tests, if present, and add focused tests for shared account-flow result mapping if the test project structure supports it.
- [ ] 8.3 Manually exercise the main federated browser flow: Login with active access, Login requiring 2FA, Login requiring consent, Consent pending, and Deny consent.
- [ ] 8.4 Manually exercise admin authorization and primary admin actions: user listing, ban/unban, user-client approval, client create/edit/regenerate secret, role create/edit, and security-event filtering.

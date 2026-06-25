# razor-pages-ui Specification

## Purpose
The Web Razor Pages UI provides scaffolded Bootstrap Identity account pages and protected admin pages that call the centralized identity Application services directly.
## Requirements
### Requirement: Federated account pages preserve client flow state
The Identity account Razor Pages used by external MVC services SHALL accept federated flow parameters from the query string and SHALL preserve them across every internal redirect and form post until the final redirect back to the calling service.

#### Scenario: Login page receives external context
- **WHEN** a browser opens `/Identity/Account/Login` with `clientId` and `redirectUri` query parameters
- **THEN** the Login PageModel binds those values for GET and POST and the Login form emits them as hidden fields

#### Scenario: Two-factor page receives temporary token context
- **WHEN** Login redirects to `LoginWith2fa` because two-factor authentication is required
- **THEN** the redirect includes `clientId`, `redirectUri`, and `tempToken`, and the 2FA form emits those values as hidden fields

#### Scenario: Recovery-code page receives temporary token context
- **WHEN** the user follows the recovery-code link from `LoginWith2fa`
- **THEN** the link and recovery-code form preserve `clientId`, `redirectUri`, and `tempToken`

#### Scenario: Consent page receives consent context
- **WHEN** Login, 2FA, recovery-code login, or external login requires consent
- **THEN** the redirect to Consent includes `clientId`, `redirectUri`, and `consentToken`, and the Consent form emits those values as hidden fields

### Requirement: Account pages dispatch authentication workflows to Application services
Identity account PageModels SHALL call Application service interfaces directly for authentication workflows and SHALL NOT call API controllers or make HTTP requests back into the same application.

#### Scenario: Password login is submitted
- **WHEN** the Login form is posted with email, password, and client context
- **THEN** the PageModel calls `IAuthService.LoginAsync` with a `LoginRequest` using response type `cookie`

#### Scenario: Registration is submitted
- **WHEN** the Register form is posted with email, password, and full name
- **THEN** the PageModel calls `IAuthService.RegisterAsync` with a `RegisterRequest` and preserves `clientId` and `redirectUri` through the registration confirmation path

#### Scenario: Two-factor code is submitted
- **WHEN** the 2FA form is posted with a temp token and authenticator code
- **THEN** the PageModel calls `ITwoFactorService.LoginWithTwoFactorAsync`

#### Scenario: Recovery code is submitted
- **WHEN** the recovery-code form is posted with a temp token and recovery code
- **THEN** the PageModel calls `ITwoFactorService.LoginWithRecoveryCodeAsync`

#### Scenario: Password reset is requested
- **WHEN** the Forgot Password form is posted with an email address
- **THEN** the PageModel calls `IAuthService.RequestPasswordResetAsync` and redirects to confirmation without revealing whether the email exists

#### Scenario: Password reset is confirmed
- **WHEN** the Reset Password form is posted with user id, token, and new password
- **THEN** the PageModel calls `IAuthService.ResetPasswordAsync`

#### Scenario: Email is confirmed
- **WHEN** the Confirm Email page receives user id and token
- **THEN** the PageModel calls `IAuthService.ConfirmEmailAsync`

#### Scenario: Password is changed
- **WHEN** an authenticated user posts the Change Password form
- **THEN** the PageModel reads the user id from the cookie principal and calls `IAuthService.ChangePasswordAsync`

### Requirement: Account pages map Application results to browser flow outcomes
Account PageModels SHALL map Application `Result<T>` outcomes into scaffolded validation errors, internal Razor Page redirects, waiting messages, access-denied messages, or final external redirects.

#### Scenario: Login requires two factor
- **WHEN** `IAuthService.LoginAsync` returns a successful response requiring two-factor authentication
- **THEN** Login redirects internally to `LoginWith2fa` with `clientId`, `redirectUri`, and `tempToken`

#### Scenario: Login requires consent
- **WHEN** login-style Application service responses require consent
- **THEN** the current page redirects internally to Consent with `clientId`, `redirectUri`, and `consentToken`

#### Scenario: Login is blocked by pending approval or revoked access
- **WHEN** a login-style Application service returns `AwaitingApproval` or `AccessRevoked`
- **THEN** the page remains in the scaffolded form and displays an inline validation error

#### Scenario: Authentication succeeds for an external MVC service
- **WHEN** a login-style Application service response completes successfully for the cookie/federated flow
- **THEN** the page validates that `redirectUri` is non-empty and redirects the browser to `redirectUri` with the returned auth code in a `code` query parameter

#### Scenario: Redirect URI is missing at final handoff
- **WHEN** a page reaches the final handoff but `redirectUri` is empty
- **THEN** the page shows a generic error result instead of redirecting to an empty or invalid target

### Requirement: Consent page renders and resolves client consent
The Consent Razor Page SHALL render client consent context from the Application consent service and SHALL let the user approve or deny the requesting client.

#### Scenario: Consent info is loaded
- **WHEN** Consent is opened with a consent token
- **THEN** the PageModel calls `IConsentService.GetConsentInfoAsync` and renders the client name and registration type context without exposing internal secrets

#### Scenario: Consent is approved
- **WHEN** the user submits Approve on Consent
- **THEN** the PageModel reads the authenticated user id from the cookie principal, passes the remote IP address to `IConsentService.GrantConsentAsync`, and handles the service response

#### Scenario: Consent approval grants active access
- **WHEN** consent approval succeeds with active access
- **THEN** Consent validates `redirectUri` and redirects to `redirectUri` with the returned auth code in a `code` query parameter

#### Scenario: Consent approval requires admin approval
- **WHEN** consent approval returns pending status
- **THEN** Consent renders a waiting-for-admin-approval message and does not redirect

#### Scenario: Consent approval is not invited
- **WHEN** consent approval fails with `NotInvited`
- **THEN** Consent renders an access-denied message and does not expose service internals

#### Scenario: Consent is denied
- **WHEN** the user submits Deny on Consent
- **THEN** Consent redirects to `redirectUri` with `error=access_denied` and does not call `IConsentService.GrantConsentAsync`

### Requirement: External provider pages preserve federated login context
External login pages SHALL list configured providers for the requested client, start provider challenges with preserved context, and route callbacks through `IExternalAuthService`.

#### Scenario: External providers are shown on Login
- **WHEN** Login or Register loads with a valid client id
- **THEN** the page calls `IExternalAuthService.GetProvidersAsync` and renders buttons for configured providers such as Google or Microsoft

#### Scenario: External challenge is started
- **WHEN** the user selects an external provider
- **THEN** the challenge preserves provider, `clientId`, `redirectUri`, and optional tenant id through authentication properties

#### Scenario: External callback succeeds
- **WHEN** the external provider callback returns login information
- **THEN** the PageModel calls `IExternalAuthService.HandleExternalCallbackAsync` and redirects to `redirectUri` with the returned auth code on success

#### Scenario: External callback requires consent
- **WHEN** the external callback workflow requires consent
- **THEN** the user is internally redirected to Consent with the preserved `clientId`, `redirectUri`, and consent token

### Requirement: Account management pages use Application services for user settings
Authenticated account management pages SHALL use Application services for password and two-factor settings instead of implementing those workflows directly in PageModels.

#### Scenario: Two-factor status page loads
- **WHEN** an authenticated user opens Two Factor Authentication
- **THEN** the PageModel reads the user id from the cookie principal and calls `ITwoFactorService.GetStatusAsync`

#### Scenario: Authenticator setup page loads
- **WHEN** an authenticated user opens Enable Authenticator
- **THEN** the PageModel calls `ITwoFactorService.SetupAuthenticatorAsync` and renders the shared key and QR-code URI

#### Scenario: Authenticator is enabled
- **WHEN** the user posts a valid authenticator code
- **THEN** the PageModel calls `ITwoFactorService.EnableTwoFactorAsync` and renders the one-time recovery codes returned by the service

#### Scenario: Two-factor is disabled
- **WHEN** the user confirms disabling 2FA with the required code
- **THEN** the PageModel calls `ITwoFactorService.DisableTwoFactorAsync`

#### Scenario: Recovery codes are regenerated
- **WHEN** the user requests new recovery codes
- **THEN** the PageModel calls `ITwoFactorService.RegenerateRecoveryCodesAsync` and renders the one-time recovery codes returned by the service

### Requirement: Admin pages are protected and isolated from federated login state
Admin Razor Pages SHALL require cookie authentication with the `Admin` role and SHALL NOT participate in the external `clientId` and `redirectUri` login flow.

#### Scenario: Non-admin opens an admin page
- **WHEN** a request without an authenticated cookie principal in the `Admin` role targets `/Admin`
- **THEN** the request is challenged or forbidden by the admin area authorization rule

#### Scenario: Admin page models are implemented
- **WHEN** admin PageModels are created
- **THEN** they inherit or receive shared authorization instead of repeating the same authorization attribute on every PageModel

#### Scenario: Admin forms post
- **WHEN** admin CRUD or action forms are rendered
- **THEN** they do not emit `clientId`, `redirectUri`, `tempToken`, or `consentToken` hidden fields

### Requirement: Admin user pages call IAdminService
Admin user-management pages SHALL call `IAdminService` for user listing, user-client access, approvals, bans, unbans, and security-event listing.

#### Scenario: Users are listed
- **WHEN** an admin opens `/Admin/Users`
- **THEN** the PageModel calls `IAdminService.GetUsersAsync` with paging and optional search and renders FullName, Email, IsActive, IsBanned, and CreatedAt when available

#### Scenario: User clients are listed
- **WHEN** an admin opens a user's Clients page
- **THEN** the PageModel calls `IAdminService.GetUserClientsAsync` and renders client name, status, granted date, and revoked date

#### Scenario: Pending user-client access is approved
- **WHEN** an admin posts Approve for a pending user-client row
- **THEN** the PageModel reads the admin user id from the cookie principal and calls `IAdminService.ApproveUserClientAsync`

#### Scenario: User is banned
- **WHEN** an admin confirms a ban with a reason
- **THEN** the PageModel reads the admin user id from the cookie principal and calls `IAdminService.BanUserAsync`

#### Scenario: User is unbanned
- **WHEN** an admin confirms unbanning a user
- **THEN** the PageModel reads the admin user id from the cookie principal and calls `IAdminService.UnbanUserAsync`

#### Scenario: Security events are listed
- **WHEN** an admin opens Security Events with optional user or client filters
- **THEN** the PageModel calls `IAdminService.GetSecurityEventsAsync` with filters and paging

### Requirement: Admin CRUD pages protect sensitive values
Admin Client and Role CRUD pages SHALL use scaffolded Razor Pages conventions while preventing sensitive values from being displayed or edited, and SHALL present allowed origins as one URL per line while persisting them as a JSON array string.

#### Scenario: Client is created
- **WHEN** an admin creates a Client
- **THEN** the server generates `ClientId`, generates a plaintext client secret, stores only `ClientSecretHash`, serializes newline-entered allowed origins to a JSON array string, and renders the plaintext client secret exactly once on a confirmation screen

#### Scenario: Client is edited
- **WHEN** an admin opens or posts the Client Edit page
- **THEN** `ClientSecretHash` is not displayed, bound from the form, or editable

#### Scenario: Client secret is regenerated
- **WHEN** an admin confirms regenerating a client secret
- **THEN** the server generates a new plaintext secret, replaces the stored hash, and renders the plaintext secret exactly once

#### Scenario: Client registration type is edited
- **WHEN** an admin creates or edits a Client
- **THEN** `RegistrationType` is rendered as an `Open`, `InviteOnly`, and `RequiresApproval` dropdown and `AllowedOrigins` is rendered as a textarea that accepts one allowed origin URL per line

#### Scenario: Allowed origins are displayed as lines
- **WHEN** an admin opens the Client Edit page
- **THEN** the stored JSON-array `AllowedOrigins` value is parsed and displayed as newline-separated URL text

#### Scenario: Allowed origins hint is shown
- **WHEN** an admin creates or edits a Client
- **THEN** the page shows a hint telling the admin to enter one allowed origin URL per line

#### Scenario: Role client is selected
- **WHEN** an admin creates or edits an AppRole
- **THEN** the page renders a Client dropdown using `Client.Name` as display text and `Client.ClientId` as the value

#### Scenario: Security events are read-only
- **WHEN** admin SecurityEvent pages are generated
- **THEN** the UI exposes Index and Details only and does not expose Create, Edit, or Delete pages for security events

### Requirement: Razor Pages follow scaffolded Bootstrap Identity conventions
The account and admin Razor Pages SHALL preserve the default ASP.NET Core Identity scaffolded Bootstrap conventions unless a small contextual addition is required.

#### Scenario: Account form is rendered
- **WHEN** Login, Register, 2FA, recovery-code, forgot-password, reset-password, or change-password forms render
- **THEN** they use scaffolded conventions including `form-floating`, `asp-for`, `asp-validation-for`, validation summaries, and `btn-primary`

#### Scenario: Client context is available
- **WHEN** account pages can resolve the requesting client display name
- **THEN** they render contextual text such as "Sign in to continue to {ClientName}" without exposing client secrets or internal database identifiers

#### Scenario: Validation scripts are needed
- **WHEN** a page renders a form requiring client-side validation
- **THEN** it reuses `_ValidationScriptsPartial.cshtml`

#### Scenario: Admin layout is rendered
- **WHEN** an admin page is displayed
- **THEN** it uses a Bootstrap layout consistent with the main site and provides navigation for Users, Clients, Roles, and Security Events

### Requirement: UI supports configured languages and cultures
The Web UI SHALL support English, Estonian, and Finnish using the cultures `en-US`, `et-EE`, and `fi-FI`, and SHALL use `en-US` as the default request culture.

#### Scenario: Default culture is applied
- **WHEN** a browser opens any MVC view or Razor Page without a culture cookie
- **THEN** the UI renders with the `en-US` culture and English resource values

#### Scenario: Supported culture is applied
- **WHEN** a browser opens any MVC view or Razor Page with a valid localization cookie for `et-EE` or `fi-FI`
- **THEN** the UI renders with the matching culture and localized Estonian or Finnish resource values

#### Scenario: Culture-specific formatting is rendered
- **WHEN** a page displays localized dates, numbers, or currency values
- **THEN** the rendered format follows the active request culture

### Requirement: UI text is resource-backed
The Web UI SHALL render user-facing text from `.resx` resources for MVC views, Admin Razor Pages, Identity Razor Pages, shared layouts, partial views, validation messages, status messages, dialogs, buttons, tooltips, table headers, empty states, error messages, and success notifications.

#### Scenario: MVC views render localized text
- **WHEN** a localized culture is active and a page under `Web/Views` is rendered
- **THEN** visible static text, page titles, navigation text, buttons, validation text, and messages come from resource entries for the active culture

#### Scenario: Admin Razor Pages render localized text
- **WHEN** a localized culture is active and an Admin page under `/Admin` is rendered
- **THEN** visible static text, navigation text, form labels, table headers, action links, empty states, validation text, and status messages come from resource entries for the active culture

#### Scenario: Identity Razor Pages render localized text
- **WHEN** a localized culture is active and an Identity account or account-management page is rendered
- **THEN** visible static text, form labels, validation text, status messages, provider buttons, and workflow messages come from resource entries for the active culture

#### Scenario: Missing translation falls back to English
- **WHEN** a resource key is missing in the active non-English resource file
- **THEN** the UI falls back to the English resource value without failing the request

### Requirement: Resource files contain supported translations
The Web project SHALL include complete shared resource files for English, Estonian, and Finnish UI text.

#### Scenario: English resources exist
- **WHEN** the Web project is built
- **THEN** `Resources.resx` is available as the default English UI resource catalog

#### Scenario: Estonian resources exist
- **WHEN** the Web project is built
- **THEN** `Resources.et.resx` is available with Estonian translations for UI resource keys

#### Scenario: Finnish resources exist
- **WHEN** the Web project is built
- **THEN** `Resources.fi.resx` is available with Finnish translations for UI resource keys

#### Scenario: Resource keys are aligned
- **WHEN** the resource files are compared
- **THEN** the Estonian and Finnish catalogs contain the same UI keys as the English catalog unless an explicitly documented fallback is intentional

### Requirement: Top navigation provides language selection
The Web UI SHALL provide a language selector in the top bar or header that lets users switch between English, Eesti, and Suomi without manually editing URLs.

#### Scenario: Language selector is rendered
- **WHEN** a user opens a page that uses the application top navigation
- **THEN** the header displays language choices for English, Eesti, and Suomi

#### Scenario: Current language is indicated
- **WHEN** the language selector is displayed
- **THEN** the currently active culture is visually indicated

#### Scenario: User switches language
- **WHEN** a user selects a different supported language
- **THEN** the application writes the standard ASP.NET Core localization cookie and redirects back to the current local page

#### Scenario: Selected language persists
- **WHEN** a user navigates to another page or returns in a future visit after selecting a language
- **THEN** the previously selected language remains active while the localization cookie is valid

#### Scenario: Invalid language is rejected
- **WHEN** a language switch request contains an unsupported culture
- **THEN** the application does not write that culture as the selected culture

#### Scenario: Return URL is constrained
- **WHEN** a language switch request includes a return URL
- **THEN** the application redirects only to a local URL or to a safe default page

### Requirement: Localization infrastructure covers validation
The Web UI SHALL apply localization to validation and presentation-layer error messages rendered by MVC views, Razor Pages, and shared validation components.

#### Scenario: Data annotation validation is localized
- **WHEN** a form field with data annotation validation fails validation
- **THEN** the validation message is rendered using the active UI culture when a localized resource value exists

#### Scenario: ModelState messages are localized
- **WHEN** a PageModel or controller adds a presentation-layer validation or error message to `ModelState`
- **THEN** the message is sourced from the shared UI resources for the active culture

#### Scenario: Status messages are localized
- **WHEN** a PageModel displays a success, warning, or error status message
- **THEN** the message is sourced from the shared UI resources for the active culture

### Requirement: Localization audit verifies no hardcoded UI strings remain
The implementation SHALL include a verification pass that checks Web UI files for remaining hardcoded user-facing strings and identifies any translation values that require human review.

#### Scenario: Hardcoded UI text is audited
- **WHEN** localization implementation is completed
- **THEN** `.cshtml` and related Web presentation `.cs` files are scanned for remaining hardcoded user-facing text

#### Scenario: Review-needed translations are reported
- **WHEN** a translation cannot be confidently translated or domain wording is ambiguous
- **THEN** the implementation summary lists the resource key and English source text for human review


# identity-application-services Specification

## Purpose
TBD - created by archiving change implement-identity-application-services. Update Purpose after archive.
## Requirements
### Requirement: Application services are exposed by topic
The Application layer SHALL expose plain service interfaces and implementation classes for authentication, consent, two-factor authentication, external authentication, and admin management.

#### Scenario: Services are resolved through dependency injection
- **WHEN** the Application dependency-registration method is called
- **THEN** `IAuthService`, `IConsentService`, `ITwoFactorService`, `IExternalAuthService`, and `IAdminService` are registered as scoped services with their concrete implementations

#### Scenario: Services avoid MediatR
- **WHEN** the Application services are implemented
- **THEN** the services use direct method calls and interfaces without MediatR requests, handlers, or pipeline behavior

### Requirement: Service requests are validated
Every request DTO accepted by an Application service method SHALL have a FluentValidation validator and SHALL be validated before business logic is executed.

#### Scenario: Request validation fails
- **WHEN** a service method receives an invalid request DTO
- **THEN** the method returns a failed `Result<T>` containing validation error information and does not perform the business operation

#### Scenario: Business failure occurs
- **WHEN** a business rule prevents a service method from completing
- **THEN** the method returns `Result<T>.Failure(...)` and does not throw an exception for the expected business failure

### Requirement: Authentication service manages local account flows
`IAuthService` SHALL implement registration, login, refresh-token rotation, logout, email confirmation, password reset, password reset confirmation, and password change using ASP.NET Identity managers and existing infrastructure services.

#### Scenario: User registers
- **WHEN** `RegisterAsync` receives a valid email, password, and full name
- **THEN** the service creates an `AppUser` with `UserManager.CreateAsync`, sends an email confirmation token through `IEmailService`, logs a security event, and returns the new user id

#### Scenario: Registration or password reset hides email existence
- **WHEN** registration or password-reset request handling encounters an email that already exists or does not exist
- **THEN** the response does not reveal whether the email exists

#### Scenario: Login validates active client
- **WHEN** `LoginAsync` is called with a `ClientId`
- **THEN** the service looks up the `Client` by `Client.ClientId`, requires it to be active, and rejects inactive or missing clients

#### Scenario: Login fails authentication
- **WHEN** password sign-in validation fails for a user login
- **THEN** the service logs a `FailedAttempt` security event where a user can be resolved and returns a failed result

#### Scenario: Login requires two factor
- **WHEN** a valid password login targets a user with two-factor authentication enabled
- **THEN** the service stores a temporary token in `AspNetUserTokens` and returns `RequiresTwoFactor` with the temp token instead of issuing access or refresh tokens

#### Scenario: Login requires consent
- **WHEN** a valid password login has no `AppUserClient` relationship for the requested client
- **THEN** the service stores a consent token in `AspNetUserTokens` that expires in 10 minutes and returns `RequiresConsent` with the consent token

#### Scenario: Login is blocked by user-client status
- **WHEN** a valid password login has a `Pending` or `Revoked` `AppUserClient` relationship
- **THEN** the service returns `AwaitingApproval` for `Pending` or `AccessRevoked` for `Revoked`

#### Scenario: Login issues tokens for active access
- **WHEN** a valid password login has an `Active` `AppUserClient` relationship
- **THEN** the service issues an access token and refresh token, stores the refresh token in `AspNetUserTokens`, and logs a `Login` security event

#### Scenario: Refresh token rotates
- **WHEN** `RefreshTokenAsync` receives a valid stored refresh token and the user-client relationship is still active
- **THEN** the service invalidates the old refresh token, stores a new refresh token, returns a new access token and refresh token, and logs `TokenRefresh`

#### Scenario: Logout revokes refresh token
- **WHEN** `LogoutAsync` receives a user id and stored refresh token
- **THEN** the service removes or invalidates that refresh token and logs `Logout`

#### Scenario: Password is changed
- **WHEN** `ChangePasswordAsync` succeeds for a user
- **THEN** the service logs `PasswordChanged`

### Requirement: Claims and roles are built dynamically
The Application layer SHALL build token and external-cookie claim data at runtime and SHALL NOT store claims through ASP.NET Identity claim tables.

#### Scenario: Token roles are client-scoped
- **WHEN** an access token or external claims response is generated for a client
- **THEN** roles are fetched through `UserManager.GetRolesAsync` and filtered to roles whose `AppRole.ClientId` matches the requesting `Client.ClientId`

#### Scenario: Persisted claims are not used
- **WHEN** Application authentication services are implemented
- **THEN** they do not call `UserManager.AddClaimAsync` or `RoleManager.AddClaimAsync`

#### Scenario: Runtime claims include client audience
- **WHEN** claims are generated for a user and client
- **THEN** the resulting claims include `sub`, `email`, `aud` containing `Client.ClientId`, and client-filtered roles

### Requirement: Consent service manages client access consent
`IConsentService` SHALL provide consent-token lookup, consent grant, and consent revocation for user-client relationships.

#### Scenario: Consent info is returned
- **WHEN** `GetConsentInfoAsync` receives a valid, unexpired consent token
- **THEN** the service returns the target client's name and registration type

#### Scenario: Open registration grants active access
- **WHEN** `GrantConsentAsync` receives a valid consent token for a client with `Open` registration
- **THEN** the service creates an `Active` `AppUserClient`, records consent metadata, assigns a client-scoped default role if one exists, issues access and refresh tokens, and logs `ConsentGiven`

#### Scenario: Invite-only registration rejects uninvited user
- **WHEN** `GrantConsentAsync` targets a client with `InviteOnly` registration and no existing invitation access
- **THEN** the service returns `NotInvited` and does not issue tokens

#### Scenario: Requires-approval registration creates pending access
- **WHEN** `GrantConsentAsync` targets a client with `RequiresApproval` registration
- **THEN** the service creates a `Pending` `AppUserClient`, records consent metadata, returns `Pending`, and logs `ConsentGiven`

#### Scenario: Consent is revoked
- **WHEN** `RevokeConsentAsync` is called for a user and client
- **THEN** the service sets the user-client status to `Revoked`, records revocation metadata, revokes refresh tokens for that user/client, and logs `ConsentRevoked`

### Requirement: Two-factor service manages authenticator and recovery flows
`ITwoFactorService` SHALL provide two-factor status, authenticator setup, enable, disable, recovery-code regeneration, temp-token two-factor login, and temp-token recovery-code login.

#### Scenario: Two-factor status is returned
- **WHEN** `GetStatusAsync` is called for a user
- **THEN** the service returns whether 2FA is enabled, whether an authenticator key exists, and how many recovery codes remain

#### Scenario: Authenticator setup is returned
- **WHEN** `SetupAuthenticatorAsync` is called for a user
- **THEN** the service returns an authenticator shared key and QR-code URI without enabling 2FA

#### Scenario: Two-factor is enabled
- **WHEN** `EnableTwoFactorAsync` receives a valid authenticator code
- **THEN** the service enables two-factor authentication, generates recovery codes, returns them, and logs `TwoFactorEnabled`

#### Scenario: Two-factor is disabled
- **WHEN** `DisableTwoFactorAsync` receives a valid authenticator code
- **THEN** the service disables two-factor authentication and logs `TwoFactorDisabled`

#### Scenario: Two-factor login continues post-authentication flow
- **WHEN** `LoginWithTwoFactorAsync` validates a temp token and two-factor code
- **THEN** the service continues the same client consent and token issuance flow as password login

#### Scenario: Recovery-code login continues post-authentication flow
- **WHEN** `LoginWithRecoveryCodeAsync` validates a temp token and recovery code
- **THEN** the service continues the same client consent and token issuance flow as password login

### Requirement: External authentication service supports federated MVC login
`IExternalAuthService` SHALL support provider discovery, external callback handling, and auth-code exchange for external MVC applications.

#### Scenario: External providers are listed for an active client
- **WHEN** `GetProvidersAsync` is called with an active client id
- **THEN** the service returns the configured external authentication provider names

#### Scenario: External callback creates or finds user
- **WHEN** `HandleExternalCallbackAsync` receives valid external login information
- **THEN** the service finds or creates an `AppUser` from external claims and treats the email as confirmed

#### Scenario: External callback validates redirect URI
- **WHEN** an external callback is ready to issue an auth code
- **THEN** the service validates `RedirectUri` against the client's `AllowedOrigins` before generating the code

#### Scenario: External callback follows consent rules
- **WHEN** the external user lacks active access to the requested client
- **THEN** the service follows the same consent, pending, and revoked access rules as local login

#### Scenario: Auth code is exchanged for claims
- **WHEN** `ExchangeAuthCodeAsync` receives a valid code, client id, and client secret matching `Client.ClientSecretHash`
- **THEN** the service validates and consumes the auth code and returns runtime claims for the external MVC service to build its own cookie

#### Scenario: Invalid client secret is rejected
- **WHEN** `ExchangeAuthCodeAsync` receives a client secret that does not match `Client.ClientSecretHash`
- **THEN** the service returns a failed result and does not return claims

### Requirement: Admin service manages users, approvals, and audit queries
`IAdminService` SHALL provide user listing, user ban/unban, user-client listing, user-client approval, and security-event listing for the auth service admin UI.

#### Scenario: Users are listed with paging
- **WHEN** `GetUsersAsync` receives page, page size, and optional search text
- **THEN** the service returns a paged response of user summaries

#### Scenario: User is banned
- **WHEN** `BanUserAsync` succeeds
- **THEN** the service sets `IsBanned`, clears active access, records the ban reason, revokes refresh tokens across all clients, and logs `AccountBanned`

#### Scenario: User is unbanned
- **WHEN** `UnbanUserAsync` succeeds
- **THEN** the service clears the ban state, reactivates the user, clears the ban reason, and logs `AccountUnbanned`

#### Scenario: User-client access is approved
- **WHEN** `ApproveUserClientAsync` is called for a pending user-client relationship
- **THEN** the service sets the relationship status to `Active`

#### Scenario: Security events are listed with filters
- **WHEN** `GetSecurityEventsAsync` receives optional user id, optional client id, page, and page size
- **THEN** the service returns a paged response of matching security events


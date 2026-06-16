## 1. Common Foundation

- [x] 1.1 Add required Application project references and NuGet dependencies for Domain, Contracts, ASP.NET Core Identity abstractions, dependency injection, authentication abstractions, and FluentValidation.
- [x] 1.2 Create `Application/Common/Result.cs` with `Result<T>` success/failure helpers and `Application/Common/Unit.cs` for void-style service results.
- [x] 1.3 Create shared response models such as `PagedResponse<T>`, `ClaimDto`, and shared token/provider constants for refresh, temp, and consent tokens stored in `AspNetUserTokens`.
- [x] 1.4 Add shared Application helpers for validation-to-result conversion, token-name construction, client lookup by `Client.ClientId`, active user-client lookup, client-filtered role lookup, token issuance, and runtime claims generation.
- [x] 1.5 Verify available `IBaseRepository<T>` query APIs and add only narrow repository contract methods needed by Application services if generic CRUD cannot support client/user lookups.
- [x] 1.6 Add an Application DI extension that registers all services and validators as scoped without MediatR.

## 2. Auth Service

- [x] 2.1 Create `Application/Auth` folder structure with `IAuthService`, `AuthService`, request DTOs, response DTOs, and validators for register, login, refresh token, logout, confirm email, password reset request, reset password, and change password.
- [x] 2.2 Implement `RegisterAsync` using `UserManager.CreateAsync`, email confirmation token generation, `IEmailService.SendEmailConfirmationAsync`, privacy-preserving errors, and security-event logging.
- [x] 2.3 Implement `LoginAsync` client validation, user/password validation, failed-attempt logging, banned/inactive checks, 2FA temp-token return, consent-token return, pending/revoked failures, active-access token issuance, refresh-token storage, and login logging.
- [x] 2.4 Implement `RefreshTokenAsync` to validate a stored refresh token, verify the user-client relationship is still active, rotate the token, issue a new access token, and log `TokenRefresh`.
- [x] 2.5 Implement `LogoutAsync` to revoke the matching stored refresh token for the user and log `Logout`.
- [x] 2.6 Implement `ConfirmEmailAsync`, `RequestPasswordResetAsync`, `ResetPasswordAsync`, and `ChangePasswordAsync`, including non-enumerating reset responses and `PasswordChanged` logging.

## 3. Consent Service

- [x] 3.1 Create `Application/Consent` folder structure with `IConsentService`, `ConsentService`, request DTOs, response DTOs, and validators.
- [x] 3.2 Implement `GetConsentInfoAsync` to validate an unexpired consent token from `AspNetUserTokens` and return client name and registration type.
- [x] 3.3 Implement `GrantConsentAsync` for `Open` registration by creating an active `AppUserClient`, recording consent metadata, assigning a client-scoped default role if present, issuing tokens, storing refresh token, and logging `ConsentGiven`.
- [x] 3.4 Implement `GrantConsentAsync` for `InviteOnly` and `RequiresApproval`, returning `NotInvited` for non-invited users and creating pending user-client records for approval-required clients.
- [x] 3.5 Implement `RevokeConsentAsync` to mark the user-client relationship revoked, record revocation metadata, revoke user/client refresh tokens, and log `ConsentRevoked`.

## 4. Two-Factor Service

- [x] 4.1 Create `Application/TwoFactor` folder structure with `ITwoFactorService`, `TwoFactorService`, request DTOs, response DTOs, and validators.
- [x] 4.2 Implement `GetStatusAsync` using Identity APIs for enabled status, authenticator-key presence, and recovery-code count.
- [x] 4.3 Implement `SetupAuthenticatorAsync` to reset or read the authenticator key and return shared key plus QR-code URI data.
- [x] 4.4 Implement `EnableTwoFactorAsync`, `DisableTwoFactorAsync`, and `RegenerateRecoveryCodesAsync` with authenticator-code verification, recovery-code generation where applicable, and security-event logging for enable/disable.
- [x] 4.5 Implement `LoginWithTwoFactorAsync` to validate temp token, verify two-factor code, clear temp token, and continue the shared post-authentication consent/token flow.
- [x] 4.6 Implement `LoginWithRecoveryCodeAsync` to validate temp token, redeem recovery code, clear temp token, and continue the shared post-authentication consent/token flow.

## 5. External Auth Service

- [x] 5.1 Create `Application/ExternalAuth` folder structure with `IExternalAuthService`, `ExternalAuthService`, request DTOs, response DTOs, and validators.
- [x] 5.2 Implement `GetProvidersAsync` for active clients using available authentication scheme/provider data.
- [x] 5.3 Implement `HandleExternalCallbackAsync` to read external login info, find or create a confirmed `AppUser`, attach login info where needed, validate client and redirect URI, apply user-client consent/status rules, generate an auth code on success, and log security events.
- [x] 5.4 Implement `ExchangeAuthCodeAsync` to validate client lookup by `Client.ClientId`, compare `ClientSecret` against `ClientSecretHash`, validate and consume auth-code payload as supported by existing token infrastructure, and return runtime claims for the external MVC app.
- [x] 5.5 Verify no external auth flow signs in with or shares the auth service admin cookie.

## 6. Admin Service

- [x] 6.1 Create `Application/Admin` folder structure with `IAdminService`, `AdminService`, request DTOs, response DTOs, and validators for paged/search/filter operations and admin commands.
- [x] 6.2 Implement `GetUsersAsync` with paging, optional search, and `UserSummary` response data.
- [x] 6.3 Implement `BanUserAsync` to set ban/inactive state, record reason, revoke refresh tokens across all clients, persist changes, and log `AccountBanned`.
- [x] 6.4 Implement `UnbanUserAsync` to clear ban state, reactivate the user, persist changes, and log `AccountUnbanned`.
- [x] 6.5 Implement `GetUserClientsAsync` and `ApproveUserClientAsync`, including lookup by `Client.ClientId` and transition of pending relationships to active.
- [x] 6.6 Implement `GetSecurityEventsAsync` with optional user/client filters and paged `SecurityEvent` results.

## 7. Verification

- [x] 7.1 Build the solution and fix compile errors caused by missing references, namespace mismatches, or repository contract gaps.
- [x] 7.2 Add focused Application unit tests or service tests for validation failures, login consent/status branches, refresh-token rotation, consent grant/revoke, 2FA continuation, external auth-code exchange, and admin ban/unban behavior if a test project exists or can be added consistently.
- [x] 7.3 Run available tests and verify no Application service calls `UserManager.AddClaimAsync` or `RoleManager.AddClaimAsync`.
- [x] 7.4 Run `openspec status --change "implement-identity-application-services"` and confirm the change is apply-ready.

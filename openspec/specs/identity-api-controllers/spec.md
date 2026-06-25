# identity-api-controllers Specification

## Purpose
The Web API layer exposes the centralized identity Application services through documented ASP.NET Core API controllers.
## Requirements
### Requirement: API controllers dispatch to Application services
The Web API layer SHALL expose controllers for authentication, two-factor authentication, consent, external authentication, and admin operations that call Application service interfaces directly and do not implement business logic.

#### Scenario: Controllers use service interfaces
- **WHEN** an API action handles a request
- **THEN** it calls the matching Application service interface method through constructor injection

#### Scenario: Controllers avoid lower-layer dependencies
- **WHEN** an API controller is implemented
- **THEN** it does not inject repositories, `UserManager`, `RoleManager`, or infrastructure services for business operations

#### Scenario: Authenticated actions use principal claims
- **WHEN** an authenticated API or admin action needs the acting user id
- **THEN** it reads `ClaimTypes.NameIdentifier` from the current principal and does not trust an acting user id from the request body

### Requirement: Result responses are mapped consistently
The Web API layer SHALL provide a shared `ApiControllerBase` with `HandleResult<T>` that maps Application `Result<T>` values to HTTP responses and documents that global API rate limiting can return HTTP 429 before action result mapping.

#### Scenario: Successful result with value
- **WHEN** `HandleResult<T>` receives a successful result with a value
- **THEN** it returns HTTP 200 with the value in the response body

#### Scenario: Successful result without response data
- **WHEN** `HandleResult<T>` receives a successful unit or empty result
- **THEN** it returns HTTP 200 without requiring a response DTO

#### Scenario: Not found failure
- **WHEN** `HandleResult<T>` receives a failed result with error `NotFound`
- **THEN** it returns HTTP 404 with an `{ error }` payload

#### Scenario: Unauthorized failure
- **WHEN** `HandleResult<T>` receives a failed result with error `Unauthorized` or `AccessRevoked`
- **THEN** it returns HTTP 401 with an `{ error }` payload

#### Scenario: Forbidden failure
- **WHEN** `HandleResult<T>` receives a failed result with error `AwaitingApproval` or `NotInvited`
- **THEN** it returns HTTP 403 with an `{ error }` payload

#### Scenario: Validation or business failure
- **WHEN** `HandleResult<T>` receives any other failed result
- **THEN** it returns HTTP 400 with an `{ error }` payload

#### Scenario: Rate limit rejection bypasses result mapping
- **WHEN** global API rate limiting rejects a request before controller action execution
- **THEN** the response is HTTP 429 with a rate-limit error payload and `Retry-After` header

### Requirement: Authentication endpoints are exposed
The Web API layer SHALL expose `AuthController` at `api/auth` for local account authentication flows.

#### Scenario: User registration
- **WHEN** `POST api/auth/register` receives a valid registration body from an anonymous caller
- **THEN** it calls `IAuthService.RegisterAsync` and returns HTTP 201 with `RegisterResponse`

#### Scenario: User login
- **WHEN** `POST api/auth/login` receives login credentials, client id, and response type from an anonymous caller
- **THEN** it calls `IAuthService.LoginAsync` and returns the mapped login result, including consent or two-factor fields when supplied by the service

#### Scenario: User logout
- **WHEN** `POST api/auth/logout` is called with JWT authentication and a refresh-token body
- **THEN** it extracts the user id from JWT claims, calls `IAuthService.LogoutAsync`, and returns the mapped result

#### Scenario: Refresh token
- **WHEN** `POST api/auth/refresh` receives a refresh token and client id from an anonymous caller
- **THEN** it calls `IAuthService.RefreshTokenAsync` and returns the mapped token response

#### Scenario: Email confirmation
- **WHEN** `POST api/auth/email/confirm` receives a user id and token from an anonymous caller
- **THEN** it calls `IAuthService.ConfirmEmailAsync` and returns the mapped result

#### Scenario: Password reset is requested
- **WHEN** `POST api/auth/password/reset/request` receives an email address from an anonymous caller
- **THEN** it calls `IAuthService.RequestPasswordResetAsync` and returns HTTP 200 according to the service result without revealing whether the email exists

#### Scenario: Password reset is confirmed
- **WHEN** `POST api/auth/password/reset/confirm` receives a user id, token, and new password from an anonymous caller
- **THEN** it calls `IAuthService.ResetPasswordAsync` and returns the mapped result

#### Scenario: Password is changed
- **WHEN** `POST api/auth/password/change` is called with JWT authentication and current and new password values
- **THEN** it extracts the user id from JWT claims, calls `IAuthService.ChangePasswordAsync`, and returns the mapped result

### Requirement: Two-factor endpoints are exposed
The Web API layer SHALL expose `TwoFactorController` at `api/auth/2fa` for authenticator and recovery-code flows.

#### Scenario: Two-factor status is returned
- **WHEN** `GET api/auth/2fa/status` is called with JWT authentication
- **THEN** it extracts the user id from JWT claims, calls `ITwoFactorService.GetStatusAsync`, and returns the mapped status response

#### Scenario: Authenticator setup is returned
- **WHEN** `GET api/auth/2fa/authenticator/setup` is called with JWT authentication
- **THEN** it extracts the user id from JWT claims, calls `ITwoFactorService.SetupAuthenticatorAsync`, and returns the mapped setup response

#### Scenario: Authenticator is enabled
- **WHEN** `POST api/auth/2fa/authenticator/enable` is called with JWT authentication and an authenticator code
- **THEN** it extracts the user id from JWT claims, calls `ITwoFactorService.EnableTwoFactorAsync`, and returns the mapped recovery-codes response

#### Scenario: Authenticator is disabled
- **WHEN** `POST api/auth/2fa/authenticator/disable` is called with JWT authentication and an authenticator code
- **THEN** it extracts the user id from JWT claims, calls `ITwoFactorService.DisableTwoFactorAsync`, and returns the mapped result

#### Scenario: Recovery codes are regenerated
- **WHEN** `POST api/auth/2fa/recovery-codes/regenerate` is called with JWT authentication and an authenticator code
- **THEN** it extracts the user id from JWT claims, calls `ITwoFactorService.RegenerateRecoveryCodesAsync`, and returns the mapped recovery-codes response

#### Scenario: Two-factor login completes
- **WHEN** `POST api/auth/2fa/login` receives a temp token and authenticator code from an anonymous caller
- **THEN** it calls `ITwoFactorService.LoginWithTwoFactorAsync` and returns the mapped login response

#### Scenario: Recovery-code login completes
- **WHEN** `POST api/auth/2fa/login/recovery` receives a temp token and recovery code from an anonymous caller
- **THEN** it calls `ITwoFactorService.LoginWithRecoveryCodeAsync` and returns the mapped login response

### Requirement: Consent endpoints are exposed
The Web API layer SHALL expose `ConsentController` at `api/auth/consent` for consent lookup, grant, and revoke operations.

#### Scenario: Consent info is returned
- **WHEN** `GET api/auth/consent` receives a `consentToken` query parameter from an anonymous caller
- **THEN** it calls `IConsentService.GetConsentInfoAsync` and returns the mapped consent info response

#### Scenario: Consent is granted
- **WHEN** `POST api/auth/consent` receives a consent-token body from an anonymous caller
- **THEN** it populates IP address from `HttpContext.Connection.RemoteIpAddress`, dispatches to `IConsentService.GrantConsentAsync`, and returns the mapped consent response

#### Scenario: Consent is revoked
- **WHEN** `DELETE api/auth/consent/{clientId}` is called with JWT authentication
- **THEN** it extracts the user id from JWT claims, calls `IConsentService.RevokeConsentAsync`, and returns the mapped result

### Requirement: External authentication endpoints are exposed
The Web API layer SHALL expose `ExternalController` at `api/auth/external` for provider discovery, external challenge, callback, and auth-code exchange.

#### Scenario: External providers are returned
- **WHEN** `GET api/auth/external/providers` receives a client id query parameter from an anonymous caller
- **THEN** it calls `IExternalAuthService.GetProvidersAsync` and returns the mapped providers response

#### Scenario: External challenge is started
- **WHEN** `POST api/auth/external/challenge` receives provider, client id, redirect URI, and optional tenant id
- **THEN** it returns a `ChallengeResult` for the requested external provider and carries the client callback context through authentication properties

#### Scenario: External callback succeeds
- **WHEN** `GET api/auth/external/callback` completes external authentication successfully for a client id and redirect URI
- **THEN** it calls `IExternalAuthService.HandleExternalCallbackAsync` and redirects to `{redirectUri}?code={code}`

#### Scenario: External callback fails
- **WHEN** `GET api/auth/external/callback` cannot complete external authentication or the Application service returns a failure
- **THEN** it redirects to `{redirectUri}?error={error}`

#### Scenario: Auth code is exchanged
- **WHEN** `POST api/auth/external/token/exchange` receives an auth code, client id, and client secret from an anonymous caller
- **THEN** it calls `IExternalAuthService.ExchangeAuthCodeAsync` and returns the mapped claims response without echoing the client secret

### Requirement: Admin API endpoints are exposed
The Web API layer SHALL expose `AdminController` at `api/admin` and protect it with cookie authentication and the Admin role.

#### Scenario: Users are listed
- **WHEN** `GET api/admin/users` receives paging and optional search query parameters from an authenticated admin
- **THEN** it calls `IAdminService.GetUsersAsync` and returns the mapped paged user response

#### Scenario: User is banned
- **WHEN** `POST api/admin/users/{userId}/ban` receives a reason body from an authenticated admin
- **THEN** it extracts the admin user id from cookie claims, calls `IAdminService.BanUserAsync`, and returns the mapped result

#### Scenario: User is unbanned
- **WHEN** `DELETE api/admin/users/{userId}/ban` is called by an authenticated admin
- **THEN** it extracts the admin user id from cookie claims, calls `IAdminService.UnbanUserAsync`, and returns the mapped result

#### Scenario: User clients are listed
- **WHEN** `GET api/admin/users/{userId}/clients` is called by an authenticated admin
- **THEN** it calls `IAdminService.GetUserClientsAsync` and returns the mapped client-access response

#### Scenario: User client is approved
- **WHEN** `POST api/admin/users/{userId}/clients/{clientId}/approve` is called by an authenticated admin
- **THEN** it extracts the admin user id from cookie claims, calls `IAdminService.ApproveUserClientAsync`, and returns the mapped result

#### Scenario: Security events are listed
- **WHEN** `GET api/admin/security-events` receives optional user id, optional client id, page, and page size query parameters from an authenticated admin
- **THEN** it calls `IAdminService.GetSecurityEventsAsync` and returns the mapped paged security-event response

### Requirement: Swagger and schema documentation are configured
The Web API layer SHALL configure Swagger/OpenAPI for the Auth Service API and provide XML documentation for controllers, actions, parameters, and DTO schemas.

#### Scenario: Swagger document is available
- **WHEN** the Web application starts
- **THEN** it exposes Swagger for document `v1` with title `Auth Service API` and version `v1`

#### Scenario: JWT authorization is documented
- **WHEN** Swagger UI is opened
- **THEN** it includes a bearer-token security definition that allows callers to authorize JWT-protected API endpoints

#### Scenario: Controller actions are documented
- **WHEN** Swagger metadata is generated
- **THEN** every API controller action has XML summary, parameter, returns comments, explicit binding-source attributes, controller grouping, JSON production metadata, and `ProducesResponseType` entries for applicable status codes

#### Scenario: DTO schemas are documented
- **WHEN** Swagger schema metadata is generated for request or response DTOs
- **THEN** DTO classes and their public properties have XML summary comments

### Requirement: Web startup configures API dependencies
The Web project SHALL configure controllers, authentication schemes, authorization middleware, Swagger services, XML documentation generation, and Application service registrations required by the API layer.

#### Scenario: Authentication schemes are registered
- **WHEN** Web services are configured
- **THEN** JWT bearer, cookie, Google, and Microsoft authentication schemes are registered side by side

#### Scenario: Middleware order supports authentication
- **WHEN** the Web application pipeline is built
- **THEN** it calls authentication middleware before authorization middleware

#### Scenario: Application services are registered
- **WHEN** controllers are activated by dependency injection
- **THEN** the Application service interfaces required by controllers are resolvable

#### Scenario: XML documentation is generated
- **WHEN** the Web project is built
- **THEN** XML documentation is generated and included by Swagger configuration

### Requirement: Client management endpoints are exposed
The Web API layer SHALL expose `ClientManagementController` at `api/client` for client role synchronization and delegated user-role management.

#### Scenario: Controller dispatches to client role service
- **WHEN** a client management action handles a request
- **THEN** it retrieves `Client` from `HttpContext.Items["AuthenticatedClient"]` and calls `IClientRoleService` without implementing role business logic

#### Scenario: Role sync endpoint is exposed
- **WHEN** `POST api/client/roles/sync` receives a valid sync body and valid client credentials
- **THEN** it calls `IClientRoleService.SyncRolesAsync` with the authenticated client's database id and returns HTTP 200 with synced roles and default role

#### Scenario: User roles endpoint is exposed
- **WHEN** `GET api/client/users/{userId:guid}/roles` receives valid client credentials
- **THEN** it calls `IClientRoleService.GetUserRolesAsync` and returns HTTP 200 with the user's client-scoped roles

#### Scenario: Set user roles endpoint is exposed
- **WHEN** `POST api/client/users/{userId:guid}/roles` receives valid client credentials and a role replacement body
- **THEN** it calls `IClientRoleService.SetUserRolesAsync` and returns HTTP 200 on success

#### Scenario: Remove user role endpoint is exposed
- **WHEN** `DELETE api/client/users/{userId:guid}/roles/{roleName}` receives valid client credentials
- **THEN** it calls `IClientRoleService.RemoveUserRoleAsync` and returns HTTP 200 on success

#### Scenario: User not in client maps to not found
- **WHEN** a client role service result fails with `UserNotInClient`
- **THEN** the controller returns HTTP 404 with an `{ error }` payload

### Requirement: Client management endpoints use client credential filtering
The Web API layer SHALL apply `ClientAuthenticationFilter` to each client management action and SHALL NOT require JWT bearer authorization for those actions.

#### Scenario: Filter is applied per action
- **WHEN** a client management action is executed
- **THEN** `[ServiceFilter(typeof(ClientAuthenticationFilter))]` authenticates the calling client for that action

#### Scenario: No controller-level JWT authorization is required
- **WHEN** `ClientManagementController` is defined
- **THEN** it has no controller-level `[Authorize]` requirement for JWT bearer authentication

#### Scenario: Filter is registered in dependency injection
- **WHEN** Web services are configured
- **THEN** `ClientAuthenticationFilter` is registered as a scoped service

### Requirement: Client management endpoints are documented in Swagger
The Web API layer SHALL document client management endpoints and their client credential headers in Swagger/OpenAPI.

#### Scenario: Client credential security definition is registered
- **WHEN** Swagger is configured
- **THEN** it includes a `ClientCredentials` API key security definition for `X-Client-Id` with a description that also requires `X-Client-Secret`

#### Scenario: Client credential headers are added only to client management operations
- **WHEN** Swagger generates operations for `ClientManagementController`
- **THEN** an operation filter adds required `X-Client-Id` and `X-Client-Secret` header parameters to those operations only

#### Scenario: Action XML documentation is present
- **WHEN** Swagger metadata is generated for `ClientManagementController`
- **THEN** every action has XML `<summary>` documentation, `<remarks>` noting header-based client credentials instead of JWT bearer, and `ProducesResponseType` entries for each documented status code

#### Scenario: Controller produces JSON
- **WHEN** `ClientManagementController` is defined
- **THEN** it has `[Produces("application/json")]` metadata

### Requirement: API endpoints are rate limited
The Web API layer SHALL apply ASP.NET Core fixed-window rate limiting to all API endpoints by client IP address.

#### Scenario: Default API rate limit applies
- **WHEN** any API endpoint is called without a stricter endpoint policy
- **THEN** the endpoint is limited to 60 requests per 60 seconds for the caller IP address

#### Scenario: Rate limit rejection returns JSON
- **WHEN** a request exceeds an applicable rate limit
- **THEN** the system returns HTTP 429 with `{ "error": "TooManyRequests" }`

#### Scenario: Retry after header is included
- **WHEN** a request is rejected by rate limiting
- **THEN** the response includes a `Retry-After` header derived from the limiter metadata

#### Scenario: Rate limit partition uses IP address
- **WHEN** a rate limit partition is selected
- **THEN** the partition key is `HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"` and does not use user identity, JWT claims, or client id

### Requirement: Sensitive authentication endpoints use strict rate limiting
The Web API layer SHALL apply the `auth-strict` rate-limiting policy to sensitive authentication actions.

#### Scenario: Strict authentication limit applies
- **WHEN** a sensitive authentication endpoint is called
- **THEN** the endpoint is limited to 10 requests per 60 seconds for the caller IP address

#### Scenario: Strict endpoints are marked
- **WHEN** API controllers are inspected
- **THEN** register, login, password reset request, password reset confirmation, two-factor login, recovery-code login, consent grant, and external challenge actions have `[EnableRateLimiting("auth-strict")]`

#### Scenario: Strict endpoint Swagger documents 429
- **WHEN** Swagger metadata is generated for an action with `[EnableRateLimiting("auth-strict")]`
- **THEN** that action documents HTTP 429 as a possible response

### Requirement: Admin client APIs accept allowed-origin lists
The Admin API SHALL expose allowed origins as structured string lists in client create and edit request DTOs while serializing to `Client.AllowedOrigins` JSON internally.

#### Scenario: Client create serializes allowed origins
- **WHEN** `POST /api/admin/clients` receives a client create body with `AllowedOrigins` as a list of URL strings
- **THEN** the controller serializes that list to JSON before passing or saving the client data

#### Scenario: Client edit serializes allowed origins
- **WHEN** `PUT /api/admin/clients/{id}` receives a client edit body with `AllowedOrigins` as a list of URL strings
- **THEN** the controller serializes that list to JSON before passing or saving the client data

#### Scenario: Swagger describes allowed-origin list semantics
- **WHEN** Swagger metadata is generated for admin client create or edit actions
- **THEN** the action remarks document that `AllowedOrigins` is supplied as a list and stored internally as JSON


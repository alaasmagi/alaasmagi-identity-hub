## ADDED Requirements

### Requirement: Token lifetimes are configured globally
The Application and Web infrastructure SHALL use one global token lifetime configuration for all clients, with no per-client override.

#### Scenario: Access token uses configured lifetime
- **WHEN** an access token is generated
- **THEN** the token expiration is set to the current UTC time plus the configured `AccessTokenSeconds` value

#### Scenario: Default token lifetimes are available
- **WHEN** no token lifetime configuration is supplied
- **THEN** access tokens default to 900 seconds and refresh tokens default to 604800 seconds

#### Scenario: Client-specific lifetime is not used
- **WHEN** tokens are generated for any registered client
- **THEN** the same global configured lifetime values are used regardless of the client

### Requirement: Refresh token expiry is enforced
The Application layer SHALL persist refresh-token expiry together with the refresh-token value and reject expired refresh tokens before issuing new tokens.

#### Scenario: Refresh token is stored with expiry
- **WHEN** a refresh token is created and stored in `AspNetUserTokens`
- **THEN** the stored token value contains JSON with the random token value and an absolute UTC expiry timestamp

#### Scenario: Expired refresh token is rejected
- **WHEN** `RefreshTokenAsync` receives a refresh token whose stored expiry is earlier than the current UTC time
- **THEN** the service returns a failed result with `RefreshTokenExpired` before issuing new tokens

#### Scenario: Valid refresh token continues normally
- **WHEN** `RefreshTokenAsync` receives a refresh token whose stored token value matches and whose expiry is in the future
- **THEN** the service continues the existing client/user validation and token rotation flow

### Requirement: Allowed origins are parsed as JSON arrays
The Application layer SHALL interpret `Client.AllowedOrigins` as a JSON array of URL strings while preserving the `string?` property type.

#### Scenario: Allowed origins parse successfully
- **WHEN** `Client.AllowedOrigins` contains a valid JSON array of strings
- **THEN** redirect validation uses the parsed string values as the allowed redirect URL list

#### Scenario: Empty or invalid allowed origins are denied
- **WHEN** `Client.AllowedOrigins` is empty, whitespace, null, or invalid JSON
- **THEN** redirect validation treats it as an empty list

#### Scenario: Allowed origins are serialized
- **WHEN** presentation code provides a list of allowed origin URLs
- **THEN** the list is serialized to JSON before being assigned to `Client.AllowedOrigins`

## MODIFIED Requirements

### Requirement: Authentication service manages local account flows
`IAuthService` SHALL implement registration, login, refresh-token rotation with configured expiry enforcement, logout, email confirmation, password reset, password reset confirmation, and password change using ASP.NET Identity managers and existing infrastructure services.

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
- **THEN** the service issues an access token using the configured access-token lifetime, stores a refresh token with configured expiry in `AspNetUserTokens`, and logs a `Login` security event

#### Scenario: Refresh token rotates
- **WHEN** `RefreshTokenAsync` receives a valid stored refresh token that has not expired and the user-client relationship is still active
- **THEN** the service invalidates the old refresh token, stores a new refresh token with configured expiry, returns a new access token and refresh token, and logs `TokenRefresh`

#### Scenario: Expired refresh token is rejected
- **WHEN** `RefreshTokenAsync` receives a refresh token whose stored expiry is earlier than the current UTC time
- **THEN** the service returns `RefreshTokenExpired`, does not issue new tokens, and does not continue further refresh processing

#### Scenario: Logout revokes refresh token
- **WHEN** `LogoutAsync` receives a user id and stored refresh token
- **THEN** the service removes or invalidates that refresh token and logs `Logout`

#### Scenario: Password is changed
- **WHEN** `ChangePasswordAsync` succeeds for a user
- **THEN** the service logs `PasswordChanged`

### Requirement: External authentication service supports federated MVC login
`IExternalAuthService` SHALL support provider discovery, external callback handling, and auth-code exchange for external MVC applications, validating redirect URIs against JSON-array allowed origins.

#### Scenario: External providers are listed for an active client
- **WHEN** `GetProvidersAsync` is called with an active client id
- **THEN** the service returns the configured external authentication provider names

#### Scenario: External callback creates or finds user
- **WHEN** `HandleExternalCallbackAsync` receives valid external login information
- **THEN** the service finds or creates an `AppUser` from external claims and treats the email as confirmed

#### Scenario: External callback validates redirect URI
- **WHEN** an external callback is ready to issue an auth code
- **THEN** the service parses `Client.AllowedOrigins` as a JSON array and requires the requested `RedirectUri` to exactly match one configured value case-insensitively

#### Scenario: External callback rejects unlisted redirect URI
- **WHEN** an external callback requests a redirect URI that is absent from the parsed allowed-origin array
- **THEN** the service returns `RedirectUriNotAllowed` and does not generate an auth code

#### Scenario: External callback follows consent rules
- **WHEN** the external user lacks active access to the requested client
- **THEN** the service follows the same consent, pending, and revoked access rules as local login

#### Scenario: Auth code is exchanged for claims
- **WHEN** `ExchangeAuthCodeAsync` receives a valid code, client id, client secret matching `Client.ClientSecretHash`, and a redirect URI allowed by the client's parsed allowed-origin array
- **THEN** the service validates and consumes the auth code and returns runtime claims for the external MVC service to build its own cookie

#### Scenario: Invalid client secret is rejected
- **WHEN** `ExchangeAuthCodeAsync` receives a client secret that does not match `Client.ClientSecretHash`
- **THEN** the service returns a failed result and does not return claims

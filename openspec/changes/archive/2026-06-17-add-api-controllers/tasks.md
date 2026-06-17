## 1. Project and Startup Wiring

- [x] 1.1 Add Web project references and NuGet packages required for Application services, JWT bearer authentication, external providers, and Swashbuckle.
- [x] 1.2 Enable XML documentation file generation in `Web/Web.csproj` and suppress missing XML comment warnings where appropriate.
- [x] 1.3 Update `Web/Program.cs` to register controllers, Application services via the existing Application DI extension, Swagger/OpenAPI with XML comments, and JWT bearer security metadata.
- [x] 1.4 Update `Web/Program.cs` authentication configuration to register JWT bearer, admin cookie, Google, and Microsoft schemes side by side from configuration.
- [x] 1.5 Update the Web middleware pipeline so authentication runs before authorization and Swagger UI is exposed with the `Auth Service API v1` document.

## 2. Shared API Infrastructure

- [x] 2.1 Create `Web/Controllers/ApiControllerBase.cs` with `HandleResult<T>` mapping success, `NotFound`, `Unauthorized`, `AccessRevoked`, `AwaitingApproval`, `NotInvited`, and fallback failures to the required HTTP responses.
- [x] 2.2 Add shared controller helpers for parsing `ClaimTypes.NameIdentifier` as `Guid` and returning unauthorized when the claim is missing or invalid.
- [x] 2.3 Add Web-specific request DTOs under `Web/Contracts/Requests` for API bodies that differ from Application requests because user/admin ids or HTTP metadata are server-populated.
- [x] 2.4 Add XML summary comments to reused Application DTOs and all new Web DTO classes/properties so Swagger schemas are documented.

## 3. Auth Controller

- [x] 3.1 Create `AuthController` at `api/auth` with `[ApiController]`, `[Produces("application/json")]`, Auth tags, XML comments, explicit binding attributes, and response metadata.
- [x] 3.2 Implement anonymous registration, login, refresh, email confirmation, password-reset request, and password-reset confirmation actions.
- [x] 3.3 Implement JWT-protected logout and password-change actions that extract the user id from JWT claims and map body DTOs to Application requests.
- [x] 3.4 Return `CreatedAtAction` for successful registration and ensure all other actions use the shared result mapper.

## 4. Two-Factor Controller

- [x] 4.1 Create `TwoFactorController` at `api/auth/2fa` with controller metadata, XML comments, explicit binding attributes, and response metadata.
- [x] 4.2 Implement JWT-protected status, authenticator setup, enable, disable, and recovery-code regeneration actions using the user id from JWT claims.
- [x] 4.3 Implement anonymous two-factor login and recovery-code login actions and map service results through `HandleResult<T>`.

## 5. Consent Controller

- [x] 5.1 Create `ConsentController` at `api/auth/consent` with controller metadata, XML comments, explicit binding attributes, and response metadata.
- [x] 5.2 Implement anonymous consent info lookup from the `consentToken` query parameter.
- [x] 5.3 Implement anonymous consent grant using a body consent token plus server-populated IP address and validated consent context assumptions from the Application service.
- [x] 5.4 Implement JWT-protected consent revocation using `clientId` from the route and user id from JWT claims.

## 6. External Auth Controller

- [x] 6.1 Create `ExternalController` at `api/auth/external` with controller metadata, XML comments, explicit binding attributes, and response metadata.
- [x] 6.2 Implement anonymous provider discovery from the `clientId` query parameter.
- [x] 6.3 Implement anonymous external challenge action that returns a `ChallengeResult` for the requested provider and carries client id, redirect URI, and optional Microsoft tenant id in authentication properties.
- [x] 6.4 Implement anonymous external callback action that obtains external login context, calls `IExternalAuthService.HandleExternalCallbackAsync`, and redirects to the supplied redirect URI with either `code` or `error`.
- [x] 6.5 Implement anonymous auth-code exchange action that calls `IExternalAuthService.ExchangeAuthCodeAsync` and never logs or echoes the client secret.

## 7. Admin Controller

- [x] 7.1 Create `AdminController` at `api/admin` protected by cookie authentication and Admin role, with controller metadata, XML comments, explicit binding attributes, and response metadata.
- [x] 7.2 Implement user listing with page, page size, and optional search query parameters.
- [x] 7.3 Implement ban, unban, and approve-user-client actions that extract the admin user id from cookie claims.
- [x] 7.4 Implement user-client listing and security-event listing actions with route and query parameter binding.

## 8. Verification

- [x] 8.1 Build the solution and fix compile errors introduced by Web project references, authentication packages, or contract namespaces.
- [x] 8.2 Run available tests or add focused controller/unit tests if an existing test project supports the Web layer.
- [ ] 8.3 Verify Swagger generation includes the API document, JWT Authorize button, controller groups, XML comments, and expected response codes.
- [x] 8.4 Review controllers for presentation-only behavior and confirm they do not access repositories, Identity managers, or infrastructure services for business operations.

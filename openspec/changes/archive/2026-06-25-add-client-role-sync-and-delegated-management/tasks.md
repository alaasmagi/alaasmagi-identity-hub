## 1. Domain and Database

- [x] 1.1 Verify whether existing `AppRole.ClientId` and `AppUserClient.ClientId` store `Client.Id` or `Client.ClientId`, and align all new requests and repository calls to the existing schema.
- [x] 1.2 Add nullable `DefaultRoleId` and `DefaultRole` navigation to `Domain/Client.cs`.
- [x] 1.3 Add nullable `DefaultRoleId` and `DefaultRole` navigation to `DTO/DataAccess/DTO/ClientEntity.cs`.
- [x] 1.4 Update `DTO/DataAccess/Mapper/ClientEntityMapper.cs` to map `DefaultRoleId` and default role data needed by Application code.
- [x] 1.5 Configure `ClientEntity.DefaultRole` in `DataAccess/Context/AppDbContext.cs` with `OnDelete(DeleteBehavior.SetNull)`.
- [x] 1.6 Add and inspect an EF Core migration for the nullable `DefaultRoleId` column, FK, and index.

## 2. Client Credential Authentication

- [x] 2.1 Identify the existing client secret hashing/verifying implementation used by auth-code exchange or admin client secret creation.
- [x] 2.2 Implement `Web/Filters/ClientAuthenticationFilter.cs` as an `IAsyncActionFilter` that validates `X-Client-Id` and `X-Client-Secret`.
- [x] 2.3 Ensure the filter returns HTTP 401 with `{ "error": "InvalidClientCredentials" }` for all credential failures and inactive clients.
- [x] 2.4 Ensure the filter stores the resolved `Client` in `HttpContext.Items["AuthenticatedClient"]` and never logs or returns the plaintext secret.
- [x] 2.5 Register `ClientAuthenticationFilter` as scoped in `Web/Program.cs`.

## 3. Application Client Role Service

- [x] 3.1 Create `Application/ClientRoles` folder structure for interface, service, requests, responses, and validators.
- [x] 3.2 Add `IClientRoleService` with `SyncRolesAsync`, `GetUserRolesAsync`, `SetUserRolesAsync`, and `RemoveUserRoleAsync`.
- [x] 3.3 Add request records for sync roles, set user roles, and remove user role.
- [x] 3.4 Add response records for sync roles and get user roles.
- [x] 3.5 Add FluentValidation validators for sync roles, set user roles, and remove user role inputs.
- [x] 3.6 Implement `ClientRoleService.SyncRolesAsync` to create missing scoped roles, preserve existing roles, avoid deletion, set or clear `Client.DefaultRoleId`, and return synced state.
- [x] 3.7 Implement user-client active-access verification shared by user-role methods, returning `UserNotInClient` for missing or inactive access.
- [x] 3.8 Implement client-scoped current-role lookup using normalized names and `UserManager.GetRolesAsync`.
- [x] 3.9 Implement `GetUserRolesAsync` to return only roles belonging to the requesting client.
- [x] 3.10 Implement `SetUserRolesAsync` as full replacement within the requesting client's scope, preserving roles from other clients.
- [x] 3.11 Implement `RemoveUserRoleAsync` to validate scoped role existence and current membership before removing.
- [x] 3.12 Register `IClientRoleService` / `ClientRoleService` as scoped in `Application/DependencyInjection.cs` or `Web/Program.cs` consistently with existing services.

## 4. Consent Default Role Assignment

- [x] 4.1 Replace the hardcoded default role lookup in `AuthWorkflow.AssignDefaultRoleIfPresentAsync` with `Client.DefaultRoleId` / `Client.DefaultRole`.
- [x] 4.2 Ensure client loading for consent includes default role data before active consent assignment.
- [x] 4.3 Verify active consent grants assign the default role only when configured and do not assign roles for pending approval.

## 5. Client Management API

- [x] 5.1 Add DTOs for role sync, set user roles, get user roles, and sync response in the Web/API layer or reuse Application records if that is the existing controller pattern.
- [x] 5.2 Implement `Web/Controllers/ClientManagementController.cs` at route `api/client`, inheriting from `ApiControllerBase` and producing JSON.
- [x] 5.3 Add `POST api/client/roles/sync` with `[ServiceFilter(typeof(ClientAuthenticationFilter))]` and authenticated-client resolution from `HttpContext.Items`.
- [x] 5.4 Add `GET api/client/users/{userId:guid}/roles` with client credential filtering and 404 mapping for `UserNotInClient`.
- [x] 5.5 Add `POST api/client/users/{userId:guid}/roles` as a full replacement endpoint with client credential filtering.
- [x] 5.6 Add `DELETE api/client/users/{userId:guid}/roles/{roleName}` with client credential filtering.
- [x] 5.7 Add XML `<summary>`, `<remarks>`, and `ProducesResponseType` documentation for every client management action.
- [x] 5.8 Ensure `ClientManagementController` has no controller-level `[Authorize]` attribute.

## 6. Swagger Integration

- [x] 6.1 Add the `ClientCredentials` OpenAPI security definition for `X-Client-Id` in `Web/Program.cs`.
- [x] 6.2 Implement an operation filter that adds required `X-Client-Id` and `X-Client-Secret` header parameters only for `ClientManagementController` actions.
- [x] 6.3 Register the operation filter with Swagger and verify other controllers do not receive the custom headers.

## 7. Verification

- [ ] 7.1 Build the solution and fix compile errors.
- [x] 7.2 Run existing automated tests if available.
- [ ] 7.3 Manually verify role sync idempotency, default-role updates, invalid credential handling, and role replacement preserving other-client roles.
- [ ] 7.4 Inspect generated Swagger JSON/UI for client credential headers and documented response codes.

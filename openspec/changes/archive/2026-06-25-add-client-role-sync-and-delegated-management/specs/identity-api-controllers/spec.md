## ADDED Requirements

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

## Why

Client applications currently depend on manual admin UI role setup, which makes deployments fragile when a service's expected role set changes. The auth service needs a machine-to-machine path for clients to declare their scoped roles, manage user assignments inside their own scope, and receive default role behavior during consent.

## What Changes

- Add a client-scoped role synchronization endpoint authenticated with `X-Client-Id` and `X-Client-Secret` headers.
- Add delegated service-to-service endpoints for a client backend to read, replace, and remove a user's roles within that client's role scope.
- Add `Client.DefaultRoleId` and a nullable FK to `AppRole` so each client can designate one default role.
- Assign the configured default role when consent creates an active `AppUserClient`.
- Add reusable client-credential action filtering for selected machine-to-machine endpoints.
- Extend Swagger/OpenAPI documentation with client credential headers for the new client management endpoints.
- Add Application-layer client role service requests, responses, validators, and business logic using the existing `Result<T>` pattern.

## Capabilities

### New Capabilities
- `client-role-management`: Client services can synchronize scoped roles and manage user role assignments within their own client boundary using client credentials.

### Modified Capabilities
- `identity-application-services`: Consent and Application service behavior changes to support client default roles and delegated client role operations.
- `identity-api-controllers`: Web API behavior changes to expose documented client management endpoints and client credential authentication filtering.

## Impact

- Domain: `Client` gains nullable `DefaultRoleId` and `DefaultRole` navigation.
- DataAccess: `AppDbContext` relationship configuration and EF Core migration for the new FK.
- Application: new `IClientRoleService` / `ClientRoleService`, request and response models, and FluentValidation validators; consent service default role assignment update.
- Web: new `ClientAuthenticationFilter`, `ClientManagementController`, Swagger operation filter/security definition updates, and DI registrations.
- APIs: new `api/client` endpoints for role sync and delegated user-role management; no JWT bearer authentication for these endpoints.

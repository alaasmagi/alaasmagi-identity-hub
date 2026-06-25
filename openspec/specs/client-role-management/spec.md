# client-role-management Specification

## Purpose
TBD - created by archiving change add-client-role-sync-and-delegated-management. Update Purpose after archive.
## Requirements
### Requirement: Client credentials authenticate delegated role endpoints
The system SHALL authenticate client role synchronization and delegated role management requests with `X-Client-Id` and `X-Client-Secret` headers.

#### Scenario: Valid client credentials are accepted
- **WHEN** a request supplies an active client's `Client.ClientId` in `X-Client-Id` and a matching plaintext secret in `X-Client-Secret`
- **THEN** the request is allowed to continue and the resolved `Client` is stored in `HttpContext.Items["AuthenticatedClient"]`

#### Scenario: Missing or invalid client credentials are rejected
- **WHEN** either client credential header is missing, malformed, inactive, unknown, or has a secret that does not match `Client.ClientSecretHash`
- **THEN** the system returns HTTP 401 with `{ "error": "InvalidClientCredentials" }`

#### Scenario: Client secret is not exposed
- **WHEN** client credential authentication fails or succeeds
- **THEN** the plaintext client secret is not logged, persisted, or echoed in any response body

### Requirement: Client services can synchronize scoped roles
The system SHALL let an authenticated client service register its role definitions without deleting existing roles.

#### Scenario: Missing roles are created
- **WHEN** a client syncs a role name that does not already exist for that client after normalized-name comparison
- **THEN** the system creates an `AppRole` for that client with the supplied role name

#### Scenario: Existing roles are preserved
- **WHEN** a client syncs a role name that already exists for that client after normalized-name comparison
- **THEN** the system leaves the existing role unchanged

#### Scenario: Roles omitted from sync remain
- **WHEN** the database contains a client role that is absent from the sync payload
- **THEN** the system does not delete or modify that role

#### Scenario: Default role is set from sync payload
- **WHEN** one or more role definitions have `IsDefault` set to true
- **THEN** the system sets `Client.DefaultRoleId` to the role matching the first default definition

#### Scenario: Default role is cleared
- **WHEN** no role definition has `IsDefault` set to true
- **THEN** the system sets `Client.DefaultRoleId` to null

#### Scenario: Role sync is idempotent
- **WHEN** the same client repeats the same role sync payload
- **THEN** the system does not create duplicate roles and returns the same synced role names and default role name

### Requirement: Client services can read user roles within client scope
The system SHALL let an authenticated client service read a user's roles that belong to that client only.

#### Scenario: Active client user roles are returned
- **WHEN** the target user has an active `AppUserClient` relationship for the authenticated client
- **THEN** the system returns the user's roles filtered to `AppRole` records scoped to that client

#### Scenario: User outside client is not found
- **WHEN** the target user has no active `AppUserClient` relationship for the authenticated client
- **THEN** the system returns a failed result with `UserNotInClient` that the API maps to HTTP 404

### Requirement: Client services can replace user roles within client scope
The system SHALL let an authenticated client service fully replace a user's roles for that client without affecting roles from other clients.

#### Scenario: Replacement validates requested roles
- **WHEN** a requested role name does not exist as an `AppRole` scoped to the authenticated client
- **THEN** the system returns a failed result with `InvalidRole:{roleName}` and does not change the user's roles

#### Scenario: Replacement removes only omitted client roles
- **WHEN** the user currently holds client-scoped roles that are absent from the replacement list
- **THEN** the system removes only those scoped roles from the user

#### Scenario: Replacement adds only missing client roles
- **WHEN** the replacement list contains client-scoped roles the user does not currently hold
- **THEN** the system adds those roles to the user

#### Scenario: Other client roles are preserved
- **WHEN** the user holds roles scoped to other clients, including roles with names that differ only by case
- **THEN** the system does not remove or add those other clients' roles

### Requirement: Client services can remove one user role within client scope
The system SHALL let an authenticated client service remove a single role from a user only when that role belongs to the authenticated client.

#### Scenario: Existing held role is removed
- **WHEN** the requested role exists for the authenticated client and the user holds that role
- **THEN** the system removes the role from the user

#### Scenario: Unknown scoped role is rejected
- **WHEN** the requested role does not exist for the authenticated client
- **THEN** the system returns a business failure and does not change the user's roles

#### Scenario: Role not held is rejected
- **WHEN** the requested role exists for the authenticated client but the user does not hold it
- **THEN** the system returns a business failure and does not change the user's roles

### Requirement: Delegated role requests are validated
The system SHALL validate delegated role request payloads before executing role business logic.

#### Scenario: Sync roles payload is invalid
- **WHEN** a sync request has no roles, an empty role name, or more than one default role
- **THEN** the system returns a validation failure and does not create roles or update `Client.DefaultRoleId`

#### Scenario: Set user roles payload is invalid
- **WHEN** a set-user-roles request has a null roles list
- **THEN** the system returns a validation failure and does not change the user's roles

#### Scenario: Remove user role payload is invalid
- **WHEN** a remove-user-role request has an empty role name
- **THEN** the system returns a validation failure and does not change the user's roles


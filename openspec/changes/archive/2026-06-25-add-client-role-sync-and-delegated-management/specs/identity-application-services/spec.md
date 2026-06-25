## ADDED Requirements

### Requirement: Client role service manages scoped role operations
`IClientRoleService` SHALL provide role synchronization, user-role lookup, user-role replacement, and single-role removal for authenticated client services.

#### Scenario: Service is resolved through dependency injection
- **WHEN** the Application dependency-registration method is called
- **THEN** `IClientRoleService` is registered as a scoped service with `ClientRoleService`

#### Scenario: Role synchronization returns synced state
- **WHEN** `SyncRolesAsync` succeeds for a client
- **THEN** it returns `SyncRolesResponse` with the synced role names and the configured default role name when one is set

#### Scenario: User roles are returned from client scope
- **WHEN** `GetUserRolesAsync` succeeds for an active user-client relationship
- **THEN** it returns `GetUserRolesResponse` with the user id and only the role names belonging to that client

#### Scenario: User roles are fully replaced in client scope
- **WHEN** `SetUserRolesAsync` succeeds
- **THEN** it removes omitted roles and adds missing roles only within the requesting client's role scope

#### Scenario: One user role is removed from client scope
- **WHEN** `RemoveUserRoleAsync` succeeds
- **THEN** it removes only the requested client-scoped role from the user

### Requirement: Client default role is persisted
The Application and persistence layers SHALL preserve each client's nullable default role selection.

#### Scenario: Client maps default role fields
- **WHEN** a `Client` is mapped to or from its EF entity representation
- **THEN** `DefaultRoleId` and `DefaultRole` data required for default assignment are preserved

#### Scenario: Default role FK is nullable
- **WHEN** a default role is deleted
- **THEN** the related client's `DefaultRoleId` is set to null rather than deleting the client

## MODIFIED Requirements

### Requirement: Consent service manages client access consent
`IConsentService` SHALL provide consent-token lookup, consent grant, and consent revocation for user-client relationships.

#### Scenario: Consent info is returned
- **WHEN** `GetConsentInfoAsync` receives a valid, unexpired consent token
- **THEN** the service returns the target client's name and registration type

#### Scenario: Open registration grants active access
- **WHEN** `GrantConsentAsync` receives a valid consent token for a client with `Open` registration
- **THEN** the service creates an `Active` `AppUserClient`, records consent metadata, assigns `Client.DefaultRole` when `Client.DefaultRoleId` is set, issues access and refresh tokens, and logs `ConsentGiven`

#### Scenario: Invite-only registration rejects uninvited user
- **WHEN** `GrantConsentAsync` targets a client with `InviteOnly` registration and no existing invitation access
- **THEN** the service returns `NotInvited` and does not issue tokens

#### Scenario: Requires-approval registration creates pending access
- **WHEN** `GrantConsentAsync` targets a client with `RequiresApproval` registration
- **THEN** the service creates a `Pending` `AppUserClient`, records consent metadata, returns `Pending`, and logs `ConsentGiven`

#### Scenario: Consent is revoked
- **WHEN** `RevokeConsentAsync` is called for a user and client
- **THEN** the service sets the user-client status to `Revoked`, records revocation metadata, revokes refresh tokens for that user/client, and logs `ConsentRevoked`

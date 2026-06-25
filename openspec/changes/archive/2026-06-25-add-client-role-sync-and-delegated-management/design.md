## Context

The auth service already scopes roles to clients through `AppRole.ClientId` and filters runtime token claims by client. Roles are currently administered manually, and `AuthWorkflow.AssignDefaultRoleIfPresentAsync` uses a hardcoded `DEFAULT` role lookup instead of a client-owned default-role setting.

This change spans Domain, DTO/DataAccess, Application, and Web. It must preserve the existing separation: repositories handle entity persistence, Identity managers handle user/role membership, Application services contain business rules, and controllers only map HTTP to Application service calls.

## Goals / Non-Goals

**Goals:**

- Let a trusted client service synchronize its complete role list without deleting existing roles.
- Let a trusted client service read, replace, and remove roles for users only inside that client's role scope.
- Persist each client's default role and assign it when consent creates active access.
- Authenticate the new machine-to-machine endpoints with client headers instead of JWT bearer tokens.
- Document the client credential headers in Swagger only for the new client management actions.

**Non-Goals:**

- No browser-facing delegated role management UI.
- No global API authentication change and no global MVC/admin behavior change.
- No deletion of roles omitted from a sync payload.
- No changes to the existing JWT or cookie claim storage model.
- No support for managing another client's roles or cross-client role inheritance.

## Decisions

1. Store the default role on `Client` as `DefaultRoleId`.

   Rationale: the default role is client configuration, not a role naming convention. Persisting the FK removes the current hardcoded `DEFAULT` assumption and allows each client to choose any of its scoped roles as default.

   Alternatives considered: keep using a conventional role name. That avoids a migration, but it cannot represent clients that want a differently named default role and conflicts with the role sync payload's explicit `IsDefault` flag.

2. Keep delegated client authentication in a scoped `IAsyncActionFilter`.

   Rationale: the new endpoints use service-to-service credentials, while the rest of the API uses JWT/cookie authentication. A `ServiceFilter` keeps this authentication path explicit on each action and lets controllers reuse the resolved client from `HttpContext.Items["AuthenticatedClient"]`.

   Alternatives considered: global middleware or a custom authorization scheme. Global middleware would affect unrelated endpoints, and a full authentication handler is more infrastructure than needed for a small set of machine endpoints.

3. Put role sync and user role operations in `IClientRoleService`.

   Rationale: this keeps role business rules out of controllers and matches the existing Application service style. The service can use `RoleManager<AppRoleEntity>` and `UserManager<AppUserEntity>` for Identity operations while using repositories for client, role, and user-client lookups.

   Alternatives considered: implement directly in `ClientManagementController`. That would duplicate Identity and repository rules in Web and violate the existing controller contract.

4. Compare role names by normalized name for client-scoped queries.

   Rationale: ASP.NET Identity treats role names case-insensitively through `NormalizedName`. Sync, validation, and set-difference logic must use normalized names or an ordinal-ignore-case comparer so repeat calls do not create duplicates and role membership changes target the intended role.

   Alternatives considered: raw string comparison. That is simpler but can create duplicate-role attempts or invalid-role failures when casing differs.

5. Replace roles only within the requesting client's scope.

   Rationale: a user may hold roles for multiple clients, and `UserManager` role membership is global by name. The service must first calculate the user's current roles that correspond to `AppRole.ClientId == clientDbId`, then only remove/add the scoped delta.

   Alternatives considered: remove all current roles before adding requested roles. That would break permissions in other clients.

## Risks / Trade-offs

- Role names are global in ASP.NET Identity while `AppRole` is client-scoped -> use normalized-name scoped repository queries and client-role filtering before mutating memberships.
- Two clients could define the same role name -> delegated operations must validate by `ClientId` and only mutate roles returned from that client's role set. If Identity role names are globally unique in the store, implementation may need to preserve the existing schema constraints and fail clearly when a duplicate name is not allowed.
- Existing clients have no `DefaultRoleId` after migration -> default role assignment is skipped until clients sync or an admin configures one.
- Client secrets are sensitive -> the filter must never log, echo, or persist the plaintext `X-Client-Secret` header.
- Sync creates roles but never deletes them -> stale roles remain until an admin removes them intentionally, preserving existing user permissions.

## Migration Plan

1. Add `DefaultRoleId` and `DefaultRole` to the Domain `Client` model and the EF `ClientEntity`, and update `ClientEntityMapper`.
2. Configure `ClientEntity.DefaultRole` with `OnDelete(DeleteBehavior.SetNull)` in `AppDbContext`.
3. Add an EF Core migration that creates a nullable `DefaultRoleId` column and FK/index to the roles table.
4. Deploy application code before clients begin calling the sync endpoint.
5. Rollback removes the new endpoint behavior and leaves the nullable column unused; a database rollback can drop the FK and column if required.

## Open Questions

- The requested contract uses `ClientDbId` for `Client.Id`, while current user-client and role repositories commonly query by `Client.ClientId`. Implementation must verify the existing stored `AppRole.ClientId` and `AppUserClient.ClientId` semantics before wiring requests, and consistently pass the internal id or public client id expected by the current schema.

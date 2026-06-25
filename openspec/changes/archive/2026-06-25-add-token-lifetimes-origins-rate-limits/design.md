## Context

The identity service currently issues JWT access tokens and refresh tokens through existing token infrastructure, stores refresh tokens in `AspNetUserTokens`, validates redirect URIs against `Client.AllowedOrigins`, exposes admin client management through Web/API and Razor Pages, and has no explicit API rate limiting. The requested change touches security behavior across Web startup, token generation/refresh validation, redirect URI validation, API documentation, and Admin Client UI.

The implementation must keep the existing architecture stable: no `ITokenService` signature change, no `Client.AllowedOrigins` property type change, no database migration, and no per-client token lifetime override.

## Goals / Non-Goals

**Goals:**
- Configure access-token and refresh-token lifetimes once globally with sensible defaults and `.env` overrides.
- Enforce refresh-token expiration using JSON stored in the existing `AspNetUserTokens` value.
- Treat `Client.AllowedOrigins` as a JSON array while keeping the persisted C# property as `string?`.
- Replace all redirect URI checks that use `AllowedOrigins` with helper-based JSON parsing and exact URL matching.
- Make Admin Client create/edit pages ergonomic by accepting one allowed origin URL per line.
- Apply IP-based fixed-window rate limiting globally to APIs, with stricter limits for sensitive authentication endpoints.
- Document allowed-origin list semantics and 429 responses in Swagger metadata.

**Non-Goals:**
- No per-client token lifetime settings.
- No database migration or schema change for refresh-token expiry or allowed origins.
- No changes to `ITokenService` public interface.
- No user-, role-, JWT-, or client-id-based rate limit partitioning.
- No behavioral changes to claims construction, user-role scope, or consent registration type rules beyond redirect-origin parsing and token expiry validation.

## Decisions

1. **Use typed global options for lifetimes**
   - Add `TokenLifetimeOptions` with defaults of 900 access-token seconds and 604800 refresh-token seconds.
   - Bind the options from `TokenLifetime` configuration in `Program.cs`.
   - Rationale: typed options keep defaults near the consuming code while still allowing `.env` / configuration override without scattering constants.
   - Alternative considered: store lifetime values directly in `TokenService` constants. Rejected because operators need `.env` configuration.

2. **Store refresh token value and expiry as JSON in `AspNetUserTokens`**
   - Store `{ "token": "...", "expiresAt": "..." }` as the token value.
   - On refresh validation, deserialize the stored value, compare `expiresAt` to `DateTime.UtcNow`, and return `RefreshTokenExpired` before normal token matching if expired.
   - Rationale: this satisfies expiry enforcement without a new table/column and keeps the change internal to token persistence.
   - Alternative considered: add a refresh-token entity/table. Rejected because no migration is desired for this change.

3. **Keep `AllowedOrigins` persisted as `string?` but define JSON-array semantics**
   - Introduce `AllowedOriginsHelper.Parse` and `AllowedOriginsHelper.Serialize`.
   - Treat malformed or empty JSON as an empty allow-list.
   - Rationale: preserves domain and database shape while giving the application a structured contract.
   - Alternative considered: change `Client.AllowedOrigins` to `List<string>`. Rejected by scope and because it would require mapping and persistence changes.

4. **Use exact redirect URI matching against the parsed allowed-origin array**
   - Replace existing comma/semicolon/origin string comparisons with `AllowedOriginsHelper.Parse(client.AllowedOrigins).Contains(redirectUri, StringComparer.OrdinalIgnoreCase)`.
   - Rationale: the requested contract says stored values are URL strings and redirect validation should compare against that list consistently.
   - Trade-off: existing rows containing comma-separated origins will no longer parse. Existing data should be migrated manually or through admin edit save if needed.

5. **Use GET/POST shape appropriate to each surface**
   - Razor Admin Client create/edit pages accept newline-separated text and serialize to JSON before saving.
   - API request DTOs expose `AllowedOrigins` as `List<string>` and controllers serialize before calling lower layers.
   - Rationale: each presentation surface gets a natural input model while the domain remains unchanged.

6. **Use built-in ASP.NET Core rate limiting**
   - Configure global fixed-window limiting with IP address partitioning.
   - Add `[EnableRateLimiting("auth-strict")]` only to sensitive actions.
   - On rejection, return 429 JSON `{ "error": "TooManyRequests" }` and always include `Retry-After` when metadata is available.
   - Rationale: built-in middleware avoids new dependencies and applies cross-cutting enforcement consistently.

## Risks / Trade-offs

- **Existing allowed origins may be comma-separated strings** -> Administrators must resave them in the new line-based UI or data must be manually converted to JSON arrays before relying on redirects.
- **Existing refresh-token values are raw strings** -> Validation should either tolerate legacy raw values until rotation or require users to reauthenticate. Prefer tolerant parsing during implementation to avoid mass logout unless the team chooses stricter behavior.
- **Rate limiting behind proxies can collapse many users into one IP** -> Existing forwarded-header configuration must run before rate limiting so `RemoteIpAddress` reflects the real client where proxy headers are trusted.
- **Global limiter affects every API endpoint** -> Sensitive endpoints get stricter limits, but all API callers must tolerate 429 responses.
- **Exact redirect URI matching is stricter than origin-only matching** -> Client registrations must include the exact callback URL values expected by authentication flows.

## Migration Plan

1. Add `.env` defaults for token lifetimes and bind typed options.
2. Implement refresh-token JSON storage/validation with expired-token failure handling.
3. Add allowed-origins helper and update all application redirect URI validation paths.
4. Update Admin Client create/edit pages and API DTO/controller mapping for allowed-origin lists.
5. Add rate limiter registration, middleware, strict endpoint attributes, and Swagger 429 documentation.
6. Build and manually verify login, refresh, external callback, auth-code exchange, Admin Client create/edit, and rate-limit rejection behavior.

Rollback:
- Revert code and config changes. Since no migration is introduced, rollback does not require database schema changes.
- Existing JSON-encoded refresh tokens would be incompatible with old raw-string validation unless rollback includes a compatibility parser or users reauthenticate.

## Open Questions

- Should legacy raw refresh-token values remain valid until they rotate, or should users with old refresh tokens be forced to reauthenticate?
- Should existing `AllowedOrigins` production data be converted proactively to JSON arrays before deployment?

## Why

Token lifetime, redirect-origin handling, and request throttling are security-sensitive behaviors that should be configured and enforced consistently across the centralized identity service. This change makes those behaviors explicit, global, and easier to operate without introducing per-client overrides or schema churn.

## What Changes

- Add global access-token and refresh-token lifetime configuration with `.env` values and typed options.
- Store refresh-token expiry alongside the persisted refresh token in `AspNetUserTokens` as JSON and reject expired refresh tokens with `RefreshTokenExpired`.
- Interpret `Client.AllowedOrigins` as a JSON array of URL strings while preserving the existing `string?` property type and avoiding a database migration.
- Update redirect URI validation to use parsed allowed-origin arrays everywhere `Client.AllowedOrigins` participates in auth flow decisions.
- Update Admin Client create/edit UI to accept one allowed origin per line and serialize the list to JSON.
- Update admin client API request semantics so allowed origins are represented as `List<string>` and serialized before Application/service use.
- Add global fixed-window rate limiting for all API endpoints and stricter rate limiting for sensitive authentication endpoints.
- Document rate-limit 429 responses and client allowed-origin request semantics in Swagger metadata.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `identity-application-services`: token lifetime enforcement, refresh-token expiry validation, and JSON-array allowed-origin parsing for auth redirect validation.
- `identity-api-controllers`: rate limiting behavior, 429 documentation, admin client allowed-origin DTO semantics, and Swagger remarks.
- `razor-pages-ui`: Admin Client create/edit pages accept newline-separated allowed origins and serialize them as JSON arrays.

## Impact

- Affected code: `.env`, `Web/Program.cs`, token infrastructure, auth workflow refresh-token validation, allowed-origin validation helpers/usages, admin Client Razor Pages, API controller DTOs/actions, Swagger documentation, and sensitive auth controller attributes.
- No database migration: refresh-token expiry is encoded in `AspNetUserTokens` value JSON and `Client.AllowedOrigins` remains `string?`.
- No `ITokenService` interface change.
- All rate-limiting partitions use client IP address only.

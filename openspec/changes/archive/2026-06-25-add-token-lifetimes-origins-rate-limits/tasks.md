## 1. Global Token Lifetime Configuration

- [x] 1.1 Add `ACCESS_TOKEN_SECONDS=900` and `REFRESH_TOKEN_SECONDS=604800` to `.env` and `.env.example`.
- [x] 1.2 Add `TokenLifetimeOptions` with section name `TokenLifetime` and default values for access and refresh token seconds.
- [x] 1.3 Register `TokenLifetimeOptions` in `Web/Program.cs` from the `TokenLifetime` configuration section.
- [x] 1.4 Update environment/config loading so `.env` values bind to `TokenLifetime:AccessTokenSeconds` and `TokenLifetime:RefreshTokenSeconds`.
- [x] 1.5 Inject `IOptions<TokenLifetimeOptions>` into `TokenService` without changing `ITokenService`.
- [x] 1.6 Set JWT access token expiry from configured `AccessTokenSeconds`.
- [x] 1.7 Add an internal refresh-token storage payload model for `{ token, expiresAt }`.
- [x] 1.8 Store refresh token JSON with expiry when `AuthWorkflow.CreateRefreshTokenAsync` persists refresh tokens.
- [x] 1.9 Update refresh-token validation to deserialize stored JSON, reject expired tokens with `RefreshTokenExpired`, and then continue existing token matching and active-access validation.
- [x] 1.10 Decide and implement legacy raw refresh-token compatibility or explicit reauthentication behavior.

## 2. Allowed Origins JSON Array Semantics

- [x] 2.1 Add static `AllowedOriginsHelper.Parse` and `AllowedOriginsHelper.Serialize` utility.
- [x] 2.2 Search all usages of `Client.AllowedOrigins` and identify every redirect URI validation path.
- [x] 2.3 Replace existing string/origin comparisons with parsed JSON-array exact URL matching.
- [x] 2.4 Ensure invalid, empty, or malformed `AllowedOrigins` values are treated as an empty allow-list.
- [x] 2.5 Update local login, external callback, consent handoff, and auth-code exchange paths to return `RedirectUriNotAllowed` or the existing equivalent mapped error when redirect URI is not configured.
- [x] 2.6 Update Admin Client create PageModel to accept one URL per line and serialize non-empty trimmed lines to JSON before saving.
- [x] 2.7 Update Admin Client edit PageModel to deserialize stored JSON into newline-separated text on GET and serialize text back to JSON on POST.
- [x] 2.8 Replace Admin Client create/edit allowed-origin inputs with textareas and add the hint `Enter one allowed origin URL per line.`
- [x] 2.9 Update admin client API request DTOs so `AllowedOrigins` is `List<string>` instead of raw JSON/string.
- [x] 2.10 Update admin client API create/edit actions to serialize DTO allowed-origin lists before passing or saving client data.
- [x] 2.11 Add or update XML remarks for admin client API create/edit actions documenting list input and JSON internal storage.

## 3. API Rate Limiting

- [x] 3.1 Add required `Microsoft.AspNetCore.RateLimiting`, `System.Threading.RateLimiting`, and formatting imports in `Web/Program.cs`.
- [x] 3.2 Register `AddRateLimiter` with a global IP-partitioned fixed-window limiter of 60 requests per 60 seconds and queue limit 0.
- [x] 3.3 Register named `default` and `auth-strict` fixed-window policies with the requested limits.
- [x] 3.4 Implement `OnRejected` to set HTTP 429, always set `Retry-After` when metadata is available, and write `{ "error": "TooManyRequests" }`.
- [x] 3.5 Add `app.UseRateLimiter()` after `UseRouting()` and before `UseAuthentication()`.
- [x] 3.6 Add `[EnableRateLimiting("auth-strict")]` to Auth register, login, password reset request, and password reset confirmation actions.
- [x] 3.7 Add `[EnableRateLimiting("auth-strict")]` to TwoFactor login and recovery-code login actions.
- [x] 3.8 Add `[EnableRateLimiting("auth-strict")]` to Consent grant action.
- [x] 3.9 Add `[EnableRateLimiting("auth-strict")]` to External challenge action.
- [x] 3.10 Add `[ProducesResponseType(StatusCodes.Status429TooManyRequests)]` to every strict-rate-limited action.
- [x] 3.11 Update `ApiControllerBase.HandleResult<T>` documentation to note that 429 can be returned globally by rate limiting.

## 4. Verification

- [x] 4.1 Build the Web project and fix compile errors.
- [ ] 4.2 Verify generated JWT expiry reflects configured `ACCESS_TOKEN_SECONDS`.
- [ ] 4.3 Verify refresh-token values are stored as JSON with expiry and expired refresh tokens return `RefreshTokenExpired`.
- [ ] 4.4 Verify allowed-origin admin create/edit stores JSON arrays and edit displays newline-separated values.
- [ ] 4.5 Verify redirect URI validation rejects URLs not exactly present in the parsed allowed-origin list.
- [ ] 4.6 Verify strict rate limiting returns HTTP 429 with `Retry-After` and JSON error for sensitive endpoints.
- [x] 4.7 Inspect Swagger for `AllowedOrigins: string[]` semantics and HTTP 429 response documentation.

## ADDED Requirements

### Requirement: API endpoints are rate limited
The Web API layer SHALL apply ASP.NET Core fixed-window rate limiting to all API endpoints by client IP address.

#### Scenario: Default API rate limit applies
- **WHEN** any API endpoint is called without a stricter endpoint policy
- **THEN** the endpoint is limited to 60 requests per 60 seconds for the caller IP address

#### Scenario: Rate limit rejection returns JSON
- **WHEN** a request exceeds an applicable rate limit
- **THEN** the system returns HTTP 429 with `{ "error": "TooManyRequests" }`

#### Scenario: Retry after header is included
- **WHEN** a request is rejected by rate limiting
- **THEN** the response includes a `Retry-After` header derived from the limiter metadata

#### Scenario: Rate limit partition uses IP address
- **WHEN** a rate limit partition is selected
- **THEN** the partition key is `HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"` and does not use user identity, JWT claims, or client id

### Requirement: Sensitive authentication endpoints use strict rate limiting
The Web API layer SHALL apply the `auth-strict` rate-limiting policy to sensitive authentication actions.

#### Scenario: Strict authentication limit applies
- **WHEN** a sensitive authentication endpoint is called
- **THEN** the endpoint is limited to 10 requests per 60 seconds for the caller IP address

#### Scenario: Strict endpoints are marked
- **WHEN** API controllers are inspected
- **THEN** register, login, password reset request, password reset confirmation, two-factor login, recovery-code login, consent grant, and external challenge actions have `[EnableRateLimiting("auth-strict")]`

#### Scenario: Strict endpoint Swagger documents 429
- **WHEN** Swagger metadata is generated for an action with `[EnableRateLimiting("auth-strict")]`
- **THEN** that action documents HTTP 429 as a possible response

### Requirement: Admin client APIs accept allowed-origin lists
The Admin API SHALL expose allowed origins as structured string lists in client create and edit request DTOs while serializing to `Client.AllowedOrigins` JSON internally.

#### Scenario: Client create serializes allowed origins
- **WHEN** `POST /api/admin/clients` receives a client create body with `AllowedOrigins` as a list of URL strings
- **THEN** the controller serializes that list to JSON before passing or saving the client data

#### Scenario: Client edit serializes allowed origins
- **WHEN** `PUT /api/admin/clients/{id}` receives a client edit body with `AllowedOrigins` as a list of URL strings
- **THEN** the controller serializes that list to JSON before passing or saving the client data

#### Scenario: Swagger describes allowed-origin list semantics
- **WHEN** Swagger metadata is generated for admin client create or edit actions
- **THEN** the action remarks document that `AllowedOrigins` is supplied as a list and stored internally as JSON

## MODIFIED Requirements

### Requirement: Result responses are mapped consistently
The Web API layer SHALL provide a shared `ApiControllerBase` with `HandleResult<T>` that maps Application `Result<T>` values to HTTP responses and documents that global API rate limiting can return HTTP 429 before action result mapping.

#### Scenario: Successful result with value
- **WHEN** `HandleResult<T>` receives a successful result with a value
- **THEN** it returns HTTP 200 with the value in the response body

#### Scenario: Successful result without response data
- **WHEN** `HandleResult<T>` receives a successful unit or empty result
- **THEN** it returns HTTP 200 without requiring a response DTO

#### Scenario: Not found failure
- **WHEN** `HandleResult<T>` receives a failed result with error `NotFound`
- **THEN** it returns HTTP 404 with an `{ error }` payload

#### Scenario: Unauthorized failure
- **WHEN** `HandleResult<T>` receives a failed result with error `Unauthorized` or `AccessRevoked`
- **THEN** it returns HTTP 401 with an `{ error }` payload

#### Scenario: Forbidden failure
- **WHEN** `HandleResult<T>` receives a failed result with error `AwaitingApproval` or `NotInvited`
- **THEN** it returns HTTP 403 with an `{ error }` payload

#### Scenario: Validation or business failure
- **WHEN** `HandleResult<T>` receives any other failed result
- **THEN** it returns HTTP 400 with an `{ error }` payload

#### Scenario: Rate limit rejection bypasses result mapping
- **WHEN** global API rate limiting rejects a request before controller action execution
- **THEN** the response is HTTP 429 with a rate-limit error payload and `Retry-After` header

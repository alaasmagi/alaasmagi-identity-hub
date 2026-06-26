# API Client Integration

This flow is for a client that wants to call identity-hub endpoints directly instead of relying on the identity-hub Razor login page.

There are two common API paths:

```text
local email/password API login
external provider challenge API
```

## Client Setup

Create or update a row in the identity-hub `Clients` table.

For `invoice-service`:

```text
Name: invoice-service
ClientId: 7e8c02c8-0468-43f0-bc61-3558567dbdf9
AllowedOrigins: https://invoice.alaasmagi.dev
IsActive: true
```

`AllowedOrigins` is the allowed callback origin. It is not the full callback path.

## Client Configuration

```env
IDENTITY_BASE_URL=https://identity.alaasmagi.dev
IDENTITY_CLIENT_ID=7e8c02c8-0468-43f0-bc61-3558567dbdf9
IDENTITY_CLIENT_SECRET=<plain-client-secret>
IDENTITY_CALLBACK_URL=https://invoice.alaasmagi.dev/auth/callback
```

The plain client secret must match the hash stored in identity-hub. If the plain value is lost, rotate/regenerate it.

## Local Login API

Use this when the client app collects email/password itself.

Request:

```http
POST https://identity.alaasmagi.dev/api/auth/login
Content-Type: application/json
```

```json
{
  "email": "user@example.com",
  "password": "password",
  "clientId": "7e8c02c8-0468-43f0-bc61-3558567dbdf9",
  "responseType": "cookie",
  "redirectUri": "https://invoice.alaasmagi.dev/auth/callback"
}
```

Successful response may contain an auth code:

```json
{
  "authCode": "...",
  "requiresTwoFactor": false,
  "tempToken": null,
  "requiresConsent": false,
  "consentToken": null,
  "error": null
}
```

If `requiresTwoFactor` or `requiresConsent` is true, complete those flows before treating the user as signed in.

Then exchange the auth code for claims.

## External Provider API

To start Google/Microsoft through the API, call:

```http
POST https://identity.alaasmagi.dev/api/auth/external/challenge
Content-Type: application/json
```

```json
{
  "provider": "Google",
  "clientId": "7e8c02c8-0468-43f0-bc61-3558567dbdf9",
  "redirectUri": "https://invoice.alaasmagi.dev/auth/callback"
}
```

For Microsoft:

```json
{
  "provider": "Microsoft",
  "clientId": "7e8c02c8-0468-43f0-bc61-3558567dbdf9",
  "redirectUri": "https://invoice.alaasmagi.dev/auth/callback",
  "tenantId": null
}
```

The endpoint returns a provider challenge redirect. The browser must follow the redirect through Google/Microsoft.

After provider login, identity-hub redirects back to the client callback:

```text
https://invoice.alaasmagi.dev/auth/callback?code=...
```

or:

```text
https://invoice.alaasmagi.dev/auth/callback?error=...
```

## Exchange Code For Claims

Request:

```http
POST https://identity.alaasmagi.dev/api/auth/external/token/exchange
Content-Type: application/json
```

```json
{
  "code": "<code from callback query>",
  "clientId": "7e8c02c8-0468-43f0-bc61-3558567dbdf9",
  "clientSecret": "<plain-client-secret>"
}
```

Response:

```json
{
  "claims": [
    {
      "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
      "value": "..."
    },
    {
      "type": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
      "value": "user@example.com"
    }
  ]
}
```

The client app should create its own local session or cookie from these claims.

## Token Refresh And Logout

If the client uses JWT-style login instead of auth-code login, identity-hub also exposes:

```http
POST /api/auth/refresh
POST /api/auth/logout
```

The auth-code exchange endpoint returns claims for the MVC/Razor client flow. It does not create a session inside the client app; the client app is responsible for creating its own session.

## Common Errors

`InvalidClient`

The `clientId` does not exist or the client is inactive.

`InvalidRedirectUri`

The `redirectUri` origin does not match `Clients.AllowedOrigins`.

`InvalidClientSecret`

The plain client secret sent by the client does not match the stored hash.

`ConsentRequired`

The user has not granted access to this client yet. Use the identity-hub Razor flow if you want identity-hub to display the consent screen.


## MODIFIED Requirements

### Requirement: Admin CRUD pages protect sensitive values
Admin Client and Role CRUD pages SHALL use scaffolded Razor Pages conventions while preventing sensitive values from being displayed or edited, and SHALL present allowed origins as one URL per line while persisting them as a JSON array string.

#### Scenario: Client is created
- **WHEN** an admin creates a Client
- **THEN** the server generates `ClientId`, generates a plaintext client secret, stores only `ClientSecretHash`, serializes newline-entered allowed origins to a JSON array string, and renders the plaintext client secret exactly once on a confirmation screen

#### Scenario: Client is edited
- **WHEN** an admin opens or posts the Client Edit page
- **THEN** `ClientSecretHash` is not displayed, bound from the form, or editable

#### Scenario: Client secret is regenerated
- **WHEN** an admin confirms regenerating a client secret
- **THEN** the server generates a new plaintext secret, replaces the stored hash, and renders the plaintext secret exactly once

#### Scenario: Client registration type is edited
- **WHEN** an admin creates or edits a Client
- **THEN** `RegistrationType` is rendered as an `Open`, `InviteOnly`, and `RequiresApproval` dropdown and `AllowedOrigins` is rendered as a textarea that accepts one allowed origin URL per line

#### Scenario: Allowed origins are displayed as lines
- **WHEN** an admin opens the Client Edit page
- **THEN** the stored JSON-array `AllowedOrigins` value is parsed and displayed as newline-separated URL text

#### Scenario: Allowed origins hint is shown
- **WHEN** an admin creates or edits a Client
- **THEN** the page shows a hint telling the admin to enter one allowed origin URL per line

#### Scenario: Role client is selected
- **WHEN** an admin creates or edits an AppRole
- **THEN** the page renders a Client dropdown using `Client.Name` as display text and `Client.ClientId` as the value

#### Scenario: Security events are read-only
- **WHEN** admin SecurityEvent pages are generated
- **THEN** the UI exposes Index and Details only and does not expose Create, Edit, or Delete pages for security events

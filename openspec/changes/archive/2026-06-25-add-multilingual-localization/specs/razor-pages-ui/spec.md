## ADDED Requirements

### Requirement: UI supports configured languages and cultures
The Web UI SHALL support English, Estonian, and Finnish using the cultures `en-US`, `et-EE`, and `fi-FI`, and SHALL use `en-US` as the default request culture.

#### Scenario: Default culture is applied
- **WHEN** a browser opens any MVC view or Razor Page without a culture cookie
- **THEN** the UI renders with the `en-US` culture and English resource values

#### Scenario: Supported culture is applied
- **WHEN** a browser opens any MVC view or Razor Page with a valid localization cookie for `et-EE` or `fi-FI`
- **THEN** the UI renders with the matching culture and localized Estonian or Finnish resource values

#### Scenario: Culture-specific formatting is rendered
- **WHEN** a page displays localized dates, numbers, or currency values
- **THEN** the rendered format follows the active request culture

### Requirement: UI text is resource-backed
The Web UI SHALL render user-facing text from `.resx` resources for MVC views, Admin Razor Pages, Identity Razor Pages, shared layouts, partial views, validation messages, status messages, dialogs, buttons, tooltips, table headers, empty states, error messages, and success notifications.

#### Scenario: MVC views render localized text
- **WHEN** a localized culture is active and a page under `Web/Views` is rendered
- **THEN** visible static text, page titles, navigation text, buttons, validation text, and messages come from resource entries for the active culture

#### Scenario: Admin Razor Pages render localized text
- **WHEN** a localized culture is active and an Admin page under `/Admin` is rendered
- **THEN** visible static text, navigation text, form labels, table headers, action links, empty states, validation text, and status messages come from resource entries for the active culture

#### Scenario: Identity Razor Pages render localized text
- **WHEN** a localized culture is active and an Identity account or account-management page is rendered
- **THEN** visible static text, form labels, validation text, status messages, provider buttons, and workflow messages come from resource entries for the active culture

#### Scenario: Missing translation falls back to English
- **WHEN** a resource key is missing in the active non-English resource file
- **THEN** the UI falls back to the English resource value without failing the request

### Requirement: Resource files contain supported translations
The Web project SHALL include complete shared resource files for English, Estonian, and Finnish UI text.

#### Scenario: English resources exist
- **WHEN** the Web project is built
- **THEN** `Resources.resx` is available as the default English UI resource catalog

#### Scenario: Estonian resources exist
- **WHEN** the Web project is built
- **THEN** `Resources.et.resx` is available with Estonian translations for UI resource keys

#### Scenario: Finnish resources exist
- **WHEN** the Web project is built
- **THEN** `Resources.fi.resx` is available with Finnish translations for UI resource keys

#### Scenario: Resource keys are aligned
- **WHEN** the resource files are compared
- **THEN** the Estonian and Finnish catalogs contain the same UI keys as the English catalog unless an explicitly documented fallback is intentional

### Requirement: Top navigation provides language selection
The Web UI SHALL provide a language selector in the top bar or header that lets users switch between English, Eesti, and Suomi without manually editing URLs.

#### Scenario: Language selector is rendered
- **WHEN** a user opens a page that uses the application top navigation
- **THEN** the header displays language choices for English, Eesti, and Suomi

#### Scenario: Current language is indicated
- **WHEN** the language selector is displayed
- **THEN** the currently active culture is visually indicated

#### Scenario: User switches language
- **WHEN** a user selects a different supported language
- **THEN** the application writes the standard ASP.NET Core localization cookie and redirects back to the current local page

#### Scenario: Selected language persists
- **WHEN** a user navigates to another page or returns in a future visit after selecting a language
- **THEN** the previously selected language remains active while the localization cookie is valid

#### Scenario: Invalid language is rejected
- **WHEN** a language switch request contains an unsupported culture
- **THEN** the application does not write that culture as the selected culture

#### Scenario: Return URL is constrained
- **WHEN** a language switch request includes a return URL
- **THEN** the application redirects only to a local URL or to a safe default page

### Requirement: Localization infrastructure covers validation
The Web UI SHALL apply localization to validation and presentation-layer error messages rendered by MVC views, Razor Pages, and shared validation components.

#### Scenario: Data annotation validation is localized
- **WHEN** a form field with data annotation validation fails validation
- **THEN** the validation message is rendered using the active UI culture when a localized resource value exists

#### Scenario: ModelState messages are localized
- **WHEN** a PageModel or controller adds a presentation-layer validation or error message to `ModelState`
- **THEN** the message is sourced from the shared UI resources for the active culture

#### Scenario: Status messages are localized
- **WHEN** a PageModel displays a success, warning, or error status message
- **THEN** the message is sourced from the shared UI resources for the active culture

### Requirement: Localization audit verifies no hardcoded UI strings remain
The implementation SHALL include a verification pass that checks Web UI files for remaining hardcoded user-facing strings and identifies any translation values that require human review.

#### Scenario: Hardcoded UI text is audited
- **WHEN** localization implementation is completed
- **THEN** `.cshtml` and related Web presentation `.cs` files are scanned for remaining hardcoded user-facing text

#### Scenario: Review-needed translations are reported
- **WHEN** a translation cannot be confidently translated or domain wording is ambiguous
- **THEN** the implementation summary lists the resource key and English source text for human review

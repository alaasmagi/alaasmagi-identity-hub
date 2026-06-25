## 1. Localization Infrastructure

- [x] 1.1 Add supported culture configuration for `en-US`, `et-EE`, and `fi-FI` in `Web/Program.cs` with `en-US` as the default culture and UI culture.
- [x] 1.2 Register ASP.NET Core localization services with a Web resource path, view localization, and data annotation localization.
- [x] 1.3 Add request localization middleware in the correct pipeline position before authentication and authorization.
- [x] 1.4 Add a shared Web resource marker type for strongly typed localizer injection.
- [ ] 1.5 Verify the Web project builds after localization services and middleware are configured.

## 2. Language Selector

- [x] 2.1 Add a culture-switch endpoint that accepts a supported culture and return URL, writes the standard localization cookie, and redirects only to local URLs.
- [x] 2.2 Add a shared top-bar language selector partial that renders English, Eesti, and Suomi.
- [x] 2.3 Visually indicate the active culture in the language selector.
- [x] 2.4 Render the selector from the main layout and admin layout without breaking Identity or Admin navigation.
- [ ] 2.5 Verify language switching updates the current page immediately and persists across navigation.

## 3. Resource Catalogs

- [x] 3.1 Create `Resources.resx` with English source values for all shared UI keys.
- [x] 3.2 Create `Resources.et.resx` with Estonian translations for all UI keys.
- [x] 3.3 Create `Resources.fi.resx` with Finnish translations for all UI keys.
- [x] 3.4 Keep resource keys aligned across English, Estonian, and Finnish files.
- [x] 3.5 Document any translations that need human review in the implementation summary.

## 4. MVC View Localization

- [x] 4.1 Localize shared MVC layouts and partials, including navigation, login links, validation partial labels, titles, and footer text.
- [x] 4.2 Localize Home and Error views, including page titles, headings, body text, and error labels.
- [x] 4.3 Replace hardcoded MVC view strings with shared localizer access or strongly typed resource access.
- [ ] 4.4 Verify MVC views render English, Estonian, and Finnish values.

## 5. Identity Razor Page Localization

- [x] 5.1 Localize Identity account pages for login, registration, confirmation, password reset, 2FA, recovery code, external login, and consent flows.
- [x] 5.2 Localize Identity account management pages for password change, 2FA setup, disabling 2FA, recovery codes, status messages, and manage navigation.
- [x] 5.3 Localize PageModel `ModelState`, `TempData`, status, success, warning, and error messages used by Identity pages.
- [x] 5.4 Preserve `clientId`, `redirectUri`, `tempToken`, and `consentToken` hidden fields and redirects while changing only presentation text.
- [ ] 5.5 Verify external MVC login flow pages still preserve federated context after localization.

## 6. Admin Razor Page Localization

- [x] 6.1 Localize Admin layout navigation and shared admin page elements.
- [x] 6.2 Localize Admin Users pages, including filters, table headers, actions, ban/unban text, client access states, and empty states.
- [x] 6.3 Localize Admin Clients pages, including create/edit/delete/details labels, registration type display text, secret confirmation text, validation messages, and action buttons.
- [x] 6.4 Localize Admin Roles pages, including list, create/edit/delete/details labels, client selector text, validation messages, and action buttons.
- [x] 6.5 Localize Admin Security Events pages, including filters, table headers, event display text, and empty states.
- [x] 6.6 Verify Admin pages still require the `Admin` role and do not participate in external federated login state.

## 7. Validation and Formatting

- [x] 7.1 Localize data annotation display names and validation messages used by Web presentation models.
- [x] 7.2 Localize presentation-layer business outcome messages without changing Application service result contracts.
- [x] 7.3 Ensure visible dates, numbers, and currency values use the current culture instead of invariant or hardcoded formatting where user-facing.
- [x] 7.4 Verify English fallback behavior for intentionally missing non-English translations.

## 8. Audit and Tests

- [x] 8.1 Scan `.cshtml` files for remaining hardcoded user-facing strings and replace or document intentional non-localized values.
- [x] 8.2 Scan Web presentation `.cs` files for hardcoded `ModelState`, `TempData`, status, success, warning, and error messages and replace or document intentional non-localized values.
- [ ] 8.3 Run the Web project build and relevant automated tests.
- [ ] 8.4 Manually verify representative MVC, Identity, and Admin pages in English, Estonian, and Finnish.
- [x] 8.5 Confirm no API contracts, JWT claims, authentication behavior, or database schema changed.

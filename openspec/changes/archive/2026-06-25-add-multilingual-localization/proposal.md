## Why

The MVC and Razor Pages UI currently presents fixed English text, which prevents Estonian and Finnish users from using the identity service in their preferred language. Adding first-class localization now makes the authentication and admin workflows production-ready for the supported user base.

## What Changes

- Configure ASP.NET Core localization for MVC views, Razor Pages, shared layouts, partials, data annotation validation messages, and request culture persistence.
- Extract user-facing UI strings from the Web project into shared `.resx` resources with English, Estonian, and Finnish values.
- Add a top-bar language selector that lists English, Eesti, and Suomi, highlights the active language, and persists the selected culture using the standard cookie culture provider.
- Configure supported cultures as `en-US`, `et-EE`, and `fi-FI`, with `en-US` as the default and English fallback behavior for missing translations.
- Verify localized rendering, language switching, persisted selection, and culture-specific date/number/currency formatting.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `razor-pages-ui`: Add multilingual UI localization, culture-aware formatting, and top-bar language selection requirements for the MVC/Razor Pages experience.

## Impact

- Affects `Web/Program.cs` service and middleware configuration.
- Affects MVC views in `Web/Views/**`, Admin Razor Pages in `Web/Areas/Admin/Pages/**`, Identity Razor Pages in `Web/Areas/Identity/Pages/**`, and related PageModel/controller validation or status-message text.
- Adds Web project resource files, likely under `Web/Resources/`, plus a shared resource marker type or strongly typed resource access pattern.
- No database schema, authentication protocol, JWT claim, API contract, or third-party localization dependency changes are expected.

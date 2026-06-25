## Context

The Web project contains MVC views, Admin Razor Pages, and scaffolded Identity Razor Pages. User-facing text is currently embedded in `.cshtml` files and PageModels, and `Program.cs` does not register ASP.NET Core localization services or request culture middleware.

The change must localize UI text for English, Estonian, and Finnish while preserving the existing authentication, consent, admin, and external MVC service flows. Localization is a presentation concern and should remain in the Web project.

## Goals / Non-Goals

**Goals:**

- Use built-in ASP.NET Core localization support for MVC, Razor Pages, view localization, data annotations, and request culture selection.
- Provide shared Web resource files for English, Estonian, and Finnish UI strings.
- Add a top-bar language selector that works from MVC views, Admin Razor Pages, and Identity Razor Pages.
- Persist the selected culture with the standard ASP.NET Core cookie culture provider.
- Ensure culture-specific formatting applies to visible dates, numbers, and currency values.
- Keep implementation maintainable by using stable resource keys and explicit resource access.

**Non-Goals:**

- Localizing API response contracts or changing HTTP API behavior.
- Localizing database seed data, persisted client names, user-entered values, or security event enum storage.
- Adding third-party localization packages.
- Changing authentication, token, claims, consent, or admin authorization semantics.

## Decisions

1. Configure built-in localization in `Program.cs`.

   Register `AddLocalization` with a Web resource path, configure `AddViewLocalization` and `AddDataAnnotationsLocalization`, and add `UseRequestLocalization` before authentication/authorization. This keeps all culture selection inside the ASP.NET Core pipeline and avoids custom middleware.

   Alternative considered: custom culture middleware. Rejected because the framework cookie provider already handles persistence and interoperates with MVC/Razor localization.

2. Use `en-US`, `et-EE`, and `fi-FI` as runtime cultures while exposing language labels as English, Eesti, and Suomi.

   The app will default to `en-US`, and supported UI cultures will match supported cultures. Resource files will be named for the neutral language requirement (`Resources.resx`, `Resources.et.resx`, `Resources.fi.resx`) while request cultures use regional culture names for formatting.

   Alternative considered: neutral cultures `en`, `et`, and `fi` everywhere. Rejected because the requirement explicitly calls for regional culture configuration and formatting behavior.

3. Prefer a shared resource marker type plus injected localizers over view-specific resource files.

   Add a small marker class such as `SharedResource` in the Web project and use `IStringLocalizer<SharedResource>` or `IHtmlLocalizer<SharedResource>` from views, partials, layouts, PageModels, and controllers. This creates one translation catalog for the UI and avoids spreading repeated strings across many per-view resource files.

   Alternative considered: one `.resx` per view/page. Rejected for this pass because the app has many scaffolded pages with repeated labels and messages; a shared catalog is easier to audit and maintain.

4. Implement culture switching through a small Web endpoint and a shared partial.

   Add an MVC action or Razor Page handler that accepts a target culture and return URL, validates the culture against the configured set, writes `CookieRequestCultureProvider.DefaultCookieName`, and redirects back locally. Render a shared language selector partial in the main and admin layouts, and ensure Identity layouts inherit the top navigation path where applicable.

   Alternative considered: changing culture via query string only. Rejected because the selected language must persist across future visits without manual URL editing.

5. Localize validation and status text at the presentation boundary.

   Hardcoded PageModel messages passed to `ModelState`, `TempData`, and status-message properties should use the shared localizer. Data annotation labels and validation messages should use resource-backed attributes where present; framework Identity validation messages can rely on configured data annotation localization where applicable, with custom text extracted explicitly.

   Alternative considered: moving all service-layer error codes into localized strings. Rejected because Application services should continue returning stable business outcomes and the Web layer should decide how to present them.

## Risks / Trade-offs

- Resource key drift or duplicate keys -> Use stable descriptive keys, keep all three resource files in sync, and include an audit task for missing translations.
- Incomplete string extraction from scaffolded Identity pages -> Scan `.cshtml` and PageModel `.cs` files, including `TempData`, `ModelState`, button text, validation summaries, table headers, empty states, and status messages.
- Culture cookie open redirect risk -> Validate requested culture against the supported culture list and only redirect to local return URLs.
- English fallback can hide missing translations -> Include a verification pass that reports missing Estonian/Finnish values before implementation is considered complete.
- Formatting changes may affect tests or snapshots -> Verify representative date, number, and currency formatting for `en-US`, `et-EE`, and `fi-FI`.

## Migration Plan

1. Add localization registration and request culture middleware.
2. Add shared resource files and the shared resource marker type.
3. Add the language selector endpoint and shared selector partial.
4. Replace hardcoded UI text in MVC views, Admin Razor Pages, Identity Razor Pages, shared partials/layouts, and related PageModels.
5. Verify language switching, persistence, fallback behavior, and culture-specific formatting.

Rollback is limited to removing the localization middleware/configuration, selector endpoint/partial, and localized resource usage. No database migration is required.

## Open Questions

- Estonian and Finnish translations for domain-specific identity/admin wording should receive human review before production release.

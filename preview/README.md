# BGFTOS browser preview

This folder is a browser-safe preview of the production BGFTOS Home screen. It deliberately uses the production launcher stylesheet and production artwork from `web/launcher` and `assets` so visual changes stay close to the Windows build.

The preview simulates service launches, Sleep, and the phone-remote address because those functions require the Windows BGFTOS host. Arrow keys, Enter, the System panel, focus states, clock/date, and the seasonal theme buttons can be exercised in a normal browser.

For GitHub Pages, publish the repository root and open `/preview/`. The production Windows package does not depend on GitHub Pages or this preview.
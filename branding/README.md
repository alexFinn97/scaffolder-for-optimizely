# Branding

| | CMS (Scaffolder for Optimizely) | Commerce (add-on) |
|---|---|---|
| Source | [logo-cms.svg](logo-cms.svg) | [logo-commerce.svg](logo-commerce.svg) |
| Extension icon (128 px) | [logo-cms-128.png](logo-cms-128.png) | [logo-commerce-128.png](logo-commerce-128.png) |
| Listing preview (200 px) | [logo-cms-200.png](logo-cms-200.png) | [logo-commerce-200.png](logo-commerce-200.png) |

Both use the publisher palette: navy `#050B24`, and a gradient from `#9E0B2C` through `#E4252C` to `#FF6A00`.

The PNGs are copied into each VSIX project as `Resources/Icon.png` and `Resources/Preview.png`. After editing an SVG, re-render it with headless Edge (or any SVG renderer that keeps transparency), changing `width` and `height` in the SVG for the 200 px version:

```bash
msedge --headless --disable-gpu --hide-scrollbars --force-device-scale-factor=1 --default-background-color=00000000 --window-size=128,128 --screenshot=logo-cms-128.png file:///C:/path/to/branding/logo-cms.svg
```

Not affiliated with or endorsed by Optimizely; the marks don't use Optimizely's logo or brand colours.

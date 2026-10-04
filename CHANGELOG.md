# Changelog

## 1.3.1
- **Tracking Accuracy**:
  - Fixed issue where item breakage/damage was not correctly deducting the lost value from the total.
  - Fixed total map value accounting when items are completely destroyed in hazards or pits, preventing double-counting.
  - Added support for `ItemValuableBox` containers so items placed inside don't duplicate or disappear from map calculations.
  - Corrected round reset handling across host and client sessions.
- **Configuration & UI**:
  - Streamlined display settings into a single `DisplayMode` option (`AlwaysOn`, `OnMapKey`, `ValueRatio`).
  - Added seamless migration for legacy config files.
  - Added slider range constraint (0.5 to 5.0) for `ValueRatio`.
  - Optimized HUD layout updates to eliminate per-frame canvas dirtiness.
- **Shop Progression (Optional)**:
  - Added new "Map Value Tracker" upgrade pedestal to the Truck Shop ($4,000 - $7,000) via REPOLib.
  - Custom dark emerald chassis with custom holographic tactical radar display screen.
  - Enabled via `RequireShopUpgrade` config setting (default `false`).
  - Supports team-wide unlock in multiplayer via `TeamWideUnlock` (default `true`).
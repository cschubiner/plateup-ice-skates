# Ice Skates verification

## 0.3.1 missing streak repair

The user confirmed the 0.3.0 skate models looked good but reported no ground streaks. Those models and movement tuning are unchanged.

- Replaced per-render-frame emission switching with movement-armed, continuous Unity emission. Idle frames between physics updates no longer turn it off. Native distance sampling and point lifetime handle spacing and fading.
- Bound owner trails to the native walking root, retained the view-root fallback for remote players, and derive ground height/rendering layer from that root. Reset clears trail history on root changes.
- Added seven pure regression cases for emission persistence and reset conditions (42 unit tests total).
- Strengthened the isolated Unity test to drive the real SkateMotion component, with idle frames between movement steps, an opaque floor, a translated stage, and a separate walking transform.
- A manual-point diagnostic attempt produced 254 mesh vertices per trail but no visible pixels. That approach was discarded. Continuous native emission visibly rendered over the floor. No shader/render-queue override was needed.
- Pixel comparison now requires visible ice-colored changes against a trails-hidden baseline. The passing run recorded 14,284 visible pixels while moving and zero after stopping. The isolated camera renders during the fade interval so native trail aging is exercised, not merely queried while off-camera.
- Pause/teleport cleanup and 31 native integration checks passed. Visual coroutine failures now produce an explicit failure marker instead of waiting for the test timeout.
- Evidence: `artifacts/runtime-20260910-192912.log`, `artifacts/visual-0.3.1/ice-streaks.png`, `ice-streaks-baseline.png`, and `ice-streaks-stopped.png`.
- Release build: zero warnings/errors. Final install was backed up and SHA256-verified. An opt-in `--iceskates-trail-capture` flag captures once after a real player produces a streak; it does not control the game or change saves.

Live restaurant verification completed September 10, 2026 on PlateUp `1.5.2-7473`: the chef equipped from the rack during service, moved across the dining floor, and visibly left two ice streaks while carrying a separate Hot Potato item. The foot-only boot appearance remained intact. The actual gameplay screenshot was inspected and preserved as `artifacts/visual-0.3.1/live-streaks.png`; the corresponding log is `artifacts/live-0.3.1-20260910.log`. The log records the real player view activating streaks on layer 12 and the capture at 12 points. The final installed build also passed its 31 native checks on that live launch.

Normal prep grab picks up the rack, as native appliance mode requires; equipment transfers work during service (practice uses item interaction mode too). This was verified against native `ManagePlayerCanPickUp` and the live service interaction. Existing unrelated `IObjectView.GetSubView` errors still appear in the modded game log. Remote-client/couch-co-op feel and a full service remain manual QA, not proven by this screenshot. The earlier 0.3.0 asset-only test did not establish that the actual equipped-chef component rendered trails.

The coordinating Hot Potato task restored all 23 original restaurant-save files and verified their SHA256 hashes again after a normal release title boot. Its temporary test saves were archived separately, and its diagnostic DLL was replaced with the normal release. PlateUp was closed after that final check; Ice Skates 0.3.1 remains installed for the next play session.

## 0.3.0 visual and speed update

Built, installed, and checked September 10, 2026 against running game version `1.5.2-7473`.

- Movement force increased from `1.32` to `1.65` (25% more than 0.2.0). Steering response increased from `3.2` to `3.8` to retain control at the higher speed. Actual travel speed still depends on the game's native drag/modifiers.
- User's before screenshot confirmed duplicate carried skates and brown block-like geometry. Archived locally as `artifacts/skates-before-0.2.0.png`.
- Corrected material handling: the game's `Simple Flat` shader uses `_Color0`, not `Material.color`. Model parts now have proper white, cyan, dark-sole, and steel colors instead of counter coloring.
- Rebuilt the boots with rounded toes, high ankles, open padded cuffs, crossed laces, dark soles, blade supports, and curved steel runners. Updated the rack's displayed pair too.
- The native item mesh is hidden only while parented to a chef. A separate feet-only model stays visible; the loose item becomes visible again when removed from the player hierarchy. No other held items are modified.
- Added two ground-aligned cosmetic ice streaks driven by actual view displacement. They fade over 1.15 seconds; pause, unequip, disable, teleport, and reset clear history. They do not alter floor friction or replicate additional gameplay state.
- Release build: 0 warnings/errors. Unit tests: 35 passed. Native integration checks: 31 passed, including hand-mesh suppression, foot-model visibility, loose-item restoration, two supported trail renderers, and clearing trail history.
- Actual Unity asset renders inspected: `artifacts/visual-0.3.0/skates-close.png`, `rack.png`, and `ice-streaks.png`. The first render exposed a covered ankle opening and the need to advance frames before capturing trail geometry; both were corrected. Final moving trail samples generated 254 vertices per streak and were visibly present in the image.
- Final native/render test log: `artifacts/runtime-20260910-185225.log`. Final installed files pass SHA256 comparison against the build.

The after images are isolated renders of the real assets inside PlateUp's Unity runtime, not screenshots of a played restaurant. Desktop screenshot tooling could not initialize in this task; the user supplied the before screenshot. Live outfit fit and higher-speed controller/co-op feel still need gameplay QA. Existing unrelated startup errors remain (including missing HueyCore and native view-update errors); this is not a clean bill of health for all subscribed mods.

## 0.2.0 repair history

Reviewed and rebuilt September 10, 2026 in the standalone IceSkatesMod repository. The unrelated original mod source was not used.

## Findings and fixes

- Ice Skates was absent from the active game's local Mods directory at the start of investigation. The cause of its disappearance is unknown; there is no evidence that a game update removed it.
- The provider set its live item ID without the serialized default used by `CItemProvider.Attach`. Native attachment reset the ID to zero, leaving nothing to dispense. The factory now uses `EditorCreateProvider` and the real ECS attachment/transfer path is tested.
- Starter creation used a generic layout gate that could include HQ. It now uses `RestaurantSystem`, prep gating, and native free parcels on available post tiles. Existing appliances, pending creations, and unopened `CLetterAppliance` parcels prevent duplicate delivery.
- Movement depended on a local host-side equipment cache. It now reads the game's synchronized tool-view updates and applies movement only on the owning client.
- Prefab getters created fresh live objects repeatedly. Templates are now cached, inactive, and persistent. Held rack colliders are disabled; materials reuse a native shader.
- Movement confused PlateUp's force scale with physical speed and needed better input/reset handling. The adapter now smooths normalized steering, preserves native drag/collisions, and resets steering on unequip, pause, captured/disconnected input, forced repositioning, and lifecycle changes.
- The old build helper referenced unrelated Workshop mods and could create a second local deployment folder. It was removed. Builds package only Ice Skates; installation is explicit, refuses a running game, preserves backups, and checks hashes.
- KitchenLib was already updated locally to 0.10.1. GDO dependency shaping and typed registration hooks now match its current API. The installed HarmonyX assembly is used rather than bundling another Harmony version.

## Evidence

- Installed PlateUp Steam build: `24651854`.
- KitchenLib: `0.10.1`, confirmed in the boot log and current release notes.
- Release build: 0 errors, 0 warnings, with blanket obsolete-warning suppression removed.
- Pure rules/configuration tests: 22 passed, 0 failed.
- Final real-Unity checks: 23 passed; final marker `SELFTEST PASS`.
- Native checks cover serialized provider ID and ECS attachment, unlimited supply/returns, parcel components, real native transfer proposal creation, equip with hands occupied, preserving the held item, returning/removing the skates, stock after return, free/unique pricing, assigned/cached prefabs, held colliders, supported materials, and all four installed Harmony targets.
- Installation smoke check compares all three files against `content` using SHA256 and rejects the known duplicate folder.
- Final successful startup log snapshot: `artifacts/runtime-final-20260910.log` (local-only, ignored by Git).
- Earlier diagnostic logs are retained. One run was stopped before late initialization; another exposed ambiguity in the test's reflection lookup. Scheduling and exact argument-type lookup were corrected before the final successful run.

Installed at:

```text
C:\Program Files (x86)\Steam\steamapps\common\PlateUp\PlateUp\Mods\IceSkates\content
```

The successful startup still logged an unrelated Discord invitation initialization error and existing asset warnings. There were no Ice Skates errors in the final checked startup. This is not a claim that every installed mod is error-free.

## Limits

These are isolated native integration checks, not a recorded full restaurant playthrough. They do not establish controller ergonomics, how slippery movement feels, day-lock pickup behavior during an actual service, presentation on every chef costume, or remote/couch-co-op synchronization. Follow the unchecked manual QA list in README before treating this release as gameplay-validated. Test with the same Ice Skates build and dependencies on all network peers.

No Workshop publication was performed. Other mods and user saves were not edited by this repair.

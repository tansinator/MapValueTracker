This mod will show total value of the valuables on the map and by default updates in real-time as things break, or are spawned in, or extracted. Current position is the right hand side of the screen a little below the extraction goal.

Is ALWAYS ON and visible by default! 

To pull up map value only when pressing the Map button (Tab by default), set `DisplayMode` to `OnMapKey`.

Can be set to only show the initial map's value and NOT update in real time by setting `StartingValueOnly` to true.

Configuration variables:
- `DisplayMode`: Dropdown selection for how the tracker is displayed on screen:
  - `AlwaysOn`: Always visible on HUD (default).
  - `OnMapKey`: Only visible while holding or toggling the Map key (Tab by default).
  - `ValueRatio`: Automatically appears when remaining map value reaches or drops below the `ValueRatio` threshold (also appears while holding Map key).
- `ValueRatio`: Ratio of remaining map value to extraction goal (slider 0.5 to 5.0). Used when `DisplayMode` is set to `ValueRatio` (e.g. 2.0 = displays when remaining value is <= 2x the goal).
- `StartingValueOnly`: Set to true to keep the Map Value fixed to the level's initially generated value. Will not update value in real time from breaking items, killing enemies, or extracting loot.
- `UIPosition`: Dropdown of UI Position presets along the right side of the screen (`Default`, `LowerRight`, `BottomRight`, `Custom`).
- `CustomPositionCoords`: The X and Y coordinates of the UI element when `UIPosition` is set to `Custom`. (0, 0) is bottom right corner. Default is (0, 225).

This mod will show total value of the valuables on the map and by default updates in real-time as things break, or are spawned in, or extracted. Current position is the right hand side of the screen a little below the extraction goal.

Is ALWAYS ON and visible by default! 

To pull up map value only when pressing the Map button (Tab by default), set 'AlwaysOn' and 'UseValueRatio' to false .

Can be set to only show the initial map's value and NOT update in real time by setting StartingValueOnly to true.

Configuration variables:
- AlwaysOn set to true will keep the value always on the HUD. Overrides any other setting like UseValueRatio.
- StartingValueOnly set to true will keep the Map Value fixed to the level's initially generated value. Will not update value in real time from breaking items, killing enemies, or extracting loot. Should not be used with UseValueRatio set to true.
- UseValueRatio set to true will only show the map value when your remaining map value is some ratio, 'ValueRatio', of the current extraction goal. Needs 'AlwaysOn' and 'StartingValueOnly' set to false to be usable.
- ValueRatio is the ratio of Map value to extraction goal. Ex: Configure 'AlwaysOn' to false, 'StartingValueOnly' to false, 'UseValueRatio' to true, and 'ValueRatio' to 2.0 to have it appear when remaining map value is 2x the current extraction goal.
- UIPosition is a drop down of UI Position presets along the right side of the screen. Set to Custom and modify CustomPositionCoords to use custom coordinates.
- CustomPositionCoords is the X and Y position of the UI element. Requires UIPosition to be set to Custom. 0.0,0.0 is bottom right corner. Default UIPosition is 0.0,225.0.

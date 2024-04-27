Quality of Life and Rebalance mod for Secrets of Grindea using ModBagman.

Might as well be called "Mario's Overhaul Mod for Secrets of Grindea". This is where I dump random experimental changes.

# Features

Changelist:
* Special effects in equipment previews use custom text instead of the generic "Special Effect" text
* Loot chance now uses a pseudo-random distribution for low drop chance items
* Berserker has reduced EP gain on hit and no passive EP drain
* Summon Plant has improved lifetime logic
* Summon Plant's Boss plant now has a radius indicator for plant buff
* Textboxes in game now allow moving the cursor and editing in the middle of text
* Frosty's Silver Charge HP now scales correctly off of MATK (150% -> 200%)
* You can now switch to Normal+ difficulty in Story. This is (almost) the same as Arcade's Catalyst of Power (or Difficulty = 1 for devs)
* Most enemy health bars now show exact HP and MaxHP
* Utility buffs can now be stacked for up to 3x the duration
* Provoke is buffed overall and taunt immunity has been removed from enemies
* Haste now also adds extra movement speed
* Some items were rebalanced / changed:
  * Angel's Thirst now scales on multiple stats
  * Lightning Glove now has a really short cooldown, allowing it to trigger rapidly
  * Hood of Defiance (the 999 essence item from Arcade) now has -9999 DEF for challenge purposes
* Some other items that were previously for style now have stats, or have had their stats changed
  * Check ItemStatRebalance.cs for full details

# Installation

Download the mod DLL and place it inside the ModBagmanData/Mods/ folder created by ModBagman.
Alternatively, add the path to the mod DLL inside ModBagmanData/config.json.

# Reporting bugs

If (or when) you encounter any bugs or crashes, you can create an issue, or report them to Marioalexsan on the Secrets of Grindea discord.
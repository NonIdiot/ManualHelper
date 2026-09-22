## ManualHelper Toggles

ManualHelper Toggles (or Toggles for short) are things that can affect gameplay through various aspects, such as disabling or modifying functionality from entities or the player.

As you may have noticed, this mod's Toggles are quite similar to the variants added by Extended Variant Mode (aka Extended Variants). This is intentional, as the mod is built with those familiar with ExtVar in mind. Not only is the menu system for this mod similar, but so is the Trigger for mappers that allows for changing ManualHelper Toggles. (Hopefully, all variants are compatible with this mod's Toggles, too.)

There are a few types of Toggles. Namely, Bool and Int. Bool are a True/False, and Int are a range of a few numbers (not always integers).

The list of Toggles are shown below:

| Toggle Name                     | Description (if any) | Type (and entries) | In-Code Name | Submenu |
|---------------------------------|----------------------|--------------------|--------------|---------|
| Left Clinging | Allows the player to grab onto left walls directly.<br/>(Does not prevent some entities from thinking they are "grabbed".) | Bool | LeftClinging | Wall Toggles |
| Left Uncrouched Climbjumping | Allows the player to climbjump on left walls while not crouched. | Bool | LeftUncrouchedClimbjumping | Wall Toggles |
| Left Crouched Climbjumping | Allows the player to climbjump on left walls while crouched. | Bool | LeftCrouchedClimbjumping | Wall Toggles |
| Left Non-Grab Wall Interactions | Allows the player to walljump, neutral jump, and slide on left walls. | Bool | LeftWallInteractions | Wall Toggles |
| Left Wallbounces | Allows the player to wallbounce on left walls. | Bool | LeftWallbounces | Wall Toggles |
| Right Clinging | Allows the player to grab onto right walls directly.<br/>(Does not prevent some entities from thinking they are "grabbed".) | Bool | RightClinging | Wall Toggles |
| Right Uncrouched Climbjumping | Allows the player to climbjump on right walls while not crouched. | Bool | RightUncrouchedClimbjumping | Wall Toggles |
| Right Crouched Climbjumping | Allows the player to climbjump on right walls while crouched. | Bool | RightCrouchedClimbjumping | Wall Toggles |
| Right Non-Grab Wall Interactions | Allows the player to walljump, neutral jump, and slide on right walls. | Bool | RightWallInteractions | Wall Toggles |
| Right Wallbounces | Allows the player to wallbounce on right walls. | Bool | RightWallbounces | Wall Toggles |
|.|.|.|.|.|
| Left Dashless Clinging | If disabled, prevents the player grabbing left walls directly when at 0 dashes.<br/>(Does not prevent some entities from thinking they are "grabbed".) | Bool | LeftDashlessClinging | Dash Toggles |
| Left Dashless Climbjumping | If disabled, climbjumping on left walls costs a dash. | Bool | LeftDashlessClimbjumping | Dash Toggles |
| Left Dashless Walljumps | If disabled, walljumping and neutral jumping on left walls costs a dash. | Bool | LeftDashlessWalljumps | Dash Toggles |
| Right Dashless Clinging | If disabled, prevents the player grabbing right walls directly when at 0 dashes.<br/>(Does not prevent some entities from thinking they are "grabbed".) | Bool | RightDashlessClinging | Dash Toggles |
| Right Dashless Climbjumping | If disabled, climbjumping on right walls costs a dash. | Bool | RightDashlessClimbjumping | Dash Toggles |
| Right Dashless Walljumps | If disabled, walljumping and neutral jumping on right walls costs a dash. | Bool | RightDashlessWalljumps | Dash Toggles |
| Usable Dash Attack | If disabled, prevents "DashAttacking" from returning true.<br/>(Prevents dashing into Crystal Hearts, Breaker Boxes, etc.) | Bool | UsableDashAttack | Dash Toggles |
|.|.|.|.|.|
| Heart Doors | Allows Heart Doors to open.<br/>(Limited functionality for modded Heart Doors.) | Bool | HeartDoors | Vanilla Entity Toggles |
| Crumble Blocks | If disabled, grabbing or standing on a Crumble Block instantly crumbles it,<br/>prevents jumping, and depletes all dashes. | Bool | CrumbleBlocks | Vanilla Entity Toggles |

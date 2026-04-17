# GraviTrix Unity Setup

This project currently has the gameplay scripts, but the scene still needs a few Unity objects and assets before it can run as a playable game.

## What to create

1. Create a `GameConfig` asset.
2. Create a `PieceLibrary` asset.
3. Create one `PieceDefinition` asset for each shape you want to use.
4. Create a `GameController` object in the scene and assign the config, piece library, board view, and HUD view.
5. Create a `BoardView` object with a `BlockCellView` prefab.
6. Create a `Canvas` with a progress bar, score text, and state text, then connect them to `GameHudView`.
7. Add a `GameInputRelay` object for keyboard and mobile touch input.

## Suggested scene hierarchy

- `GameRoot`
  - `GameController`
  - `GameInputRelay`
  - `BoardView`
  - `Canvas`
    - `HUD`

## Important Unity settings

1. Open `Edit > Project Settings > Player`.
2. Make sure `Active Input Handling` is set to `Both` or `Input System Package (New)`.
3. Make sure your build target is Android or iOS when you are ready to ship the mobile version.

## How the board works

- The board is 12 by 12.
- `x` increases to the right.
- `y` increases downward in game logic.
- The board rotates only between two states: original and 90 degrees left.
- There is no active piece while the board is rotating.

## How to build pieces

Each `PieceDefinition` stores:

- a pivot cell,
- a list of local block positions,
- a block kind per cell.

For your first pass, keep shapes simple and create one piece per art block you send later.

## What happens after this code is in place

Once you add the first set of piece assets, the game loop will be:

1. Spawn a piece in the upper-middle area.
2. Let the player move or rotate it.
3. Lock it when it lands.
4. Decrease the rotation counter.
5. Rotate the board when the counter reaches zero.
6. Spawn the next piece.

## Next step after you send the blocks

When you send the block art, I can help you turn each one into a proper Unity prefab and wire the visuals so the game is actually playable on mobile.
# Block Art Import Guide

Use this guide to import the block images you shared and connect them to gameplay.

## 1) Create art folders

In the Unity Project window:

1. Create folder `Assets/Art`.
2. Inside it, create folder `Assets/Art/Blocks`.

## 2) Save your PNG files with these names

Put your images in `Assets/Art/Blocks` and use these exact filenames:

- `block_yellow.png`
- `block_cyan.png`
- `block_green.png`
- `block_blue.png`
- `block_orange.png`
- `block_pink.png`
- `block_red.png`
- `block_deepblue.png`
- `block_metal.png`
- `block_lava.png`
- `block_line.png`

## 3) Sprite import settings

Select all block PNG files and apply:

1. `Texture Type` = `Sprite (2D and UI)`
2. `Sprite Mode` = `Single`
3. `Pixels Per Unit` = `256` (or same value for all)
4. `Filter Mode` = `Bilinear`
5. `Compression` = `None`
6. Click `Apply`

## 4) Create Block Skin Library asset

1. Right click in Project window, create `GraviTrix > Block Skin Library`.
2. Name it `BlockSkinLibrary_Main`.
3. Drag each sprite into the matching slot in the inspector.

## 5) Connect skin to BlockCell prefab

1. Open your `BlockCellView` prefab.
2. Assign `BlockSkinLibrary_Main` to the `Skin Library` field.

## 6) Assign visuals per piece cell

In each `PieceDefinition` cell entry:

1. Choose `Kind` (`Normal`, `Lava`, `Metal`, `Line`).
2. Choose `Visual Type` (`Yellow`, `Cyan`, `Green`, `Blue`, `Orange`, `Pink`, `Red`, `DeepBlue`, `Metal`, `Lava`, `Line`).

If `Visual Type` is `Auto`, it will default by type:

- `Lava` kind -> `Lava` visual
- `Metal` kind -> `Metal` visual
- `Line` kind -> `Line` visual
- `Normal` kind -> `Yellow` visual
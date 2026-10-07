# Waffle House Simulator CS 417 MP 2 Fall 2026 Team 7

## Contributors
- Kyle: Crop farm (wheat and corn for flour and corn syrup)
- Rishi: Egg and chicken farm
- Sharyq: Waffle House
- Aster: Meth lab
- Alyssa: TBD, possibly dairy farm

## Basic rules
1. Make sure you put all your files in `Assets/<name>/`.
2. Any files that we all need to use go in `Assets/Shared/`. Give the group a heads up before changing anything in there.
3. Don't edit `Assets/Scenes/MainScene.unity`. We will merge everything together post MP 2A.
4. Make and work on your own scene in your folder. We are going to eventually turn your whole area into a prefab and put it in the main scene for MP 2B.
5. Work on your own branch. We will merge post MP 2A.
6. Always commit `.meta` files along with their assets, or references to those assets break for everyone else.
7. Wrap your scripts in `namespace <name>` so two people can both have classes with the same name. For example, I would put my scripts in `namespace Rishi { }`.
8. Create an empty object at the top of your scene hierarchy that is the parent of everything in your area (e.g. `RishiSceneRoot`). Spawn stuff under and relative to it, so when we merge into one scene we don't have a mess.
9. Put any surface that things can be placed on on the `canPlaceOn` layer. Don't put the placeable objects themselves on that layer.

## Shared systems (`Assets/Shared`)
- `GridManager`: one per scene. Cell size is 0.5 units and cell (0, 0) is at the world origin, so everyone's areas line up. Has `Snap`, `WorldToCell`, `CellToWorld`, and occupancy tracking. You usually don't call the occupancy functions yourself, use `GridOccupant` instead.
- `GridOccupant`: put this on anything that snaps to the grid. Set its Size in cells (odd sizes line up best, e.g. (1, 1) is 0.5 x 0.5 units, (3, 3) is 1.5 x 1.5). Anything placed in the scene by hand snaps to the grid and claims its cells on Start, and frees them when destroyed. Has `TryPlaceAt` and `RemoveFromGrid` for moving things.
- `GridPlaceable`: lets the player pick something up and drop it onto the grid. Automatically adds an XR Grab Interactable and a GridOccupant. Optional resource cost, charged the first time it's placed. Set Placeable Surface Layers to `canPlaceOn`.
- `GridSupplyShelf`: an endless supply of one `GridPlaceable`. Keeps one on display and restocks after it's grabbed.
- `GridSpawner`: spawns a prefab with a `GridOccupant` at the nearest grid position, for things spawned via script. `Spawn On Start` spawns that many at random spots when the game starts. Call `SpawnAt(position)` or `SpawnRandom(count)` from your own scripts.
- `ResourceManager`: one per scene. Resource names are case sensitive strings. Has `Add`, `Get`, `CanAfford`, `TrySpend`, and an `OnResourceChanged` event (subscribe in `Start`, not `Awake` or `OnEnable`).

```csharp
using Shared;

ResourceManager.Instance.Add("Egg", 1);
if (ResourceManager.Instance.TrySpend("Corn", 2)) {
  // feed chickens
}
```

## Making something that attaches to the grid
1. Make an empty root object and put the model under it so the model's bottom sits at the root's origin.
2. Add a collider of some sort
3. Add `GridOccupant` (if it just sits there) or `GridPlaceable` (if the player can move it), and set the Size.
4. Save it as a prefab in `Assets/<name>/Prefabs/`.

## Layers
- User Layer 6: `canPlaceOn`: floors and surfaces that grid objects can be placed on.

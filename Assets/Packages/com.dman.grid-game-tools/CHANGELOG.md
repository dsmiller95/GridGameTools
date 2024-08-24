# Changelog

## [0.4.0] - 2024-08-14

### Migration required

- All namespaces adjusted
- all ICreateDungeonComponent must accept a new parameter
- add CreatePathingDataComponent to the DungeonWorldLoader, and uncheck createPathingExtraAlways in the inspector.
- Any direct construction of dungeon worlds will not include pathfinding data by default
- Any IWorldComponent implementations must now implement disposable interfaces,
- Any IWorldComponentWriter must accept a `bool andDispose` parameter when baking to an immutable data store 
- Suggested: start using IComponentStore to get pathing data
  - previous helper methods to access pathing data have been deprecated

### Added

- new GridWorldBuilding package, to expose string-to-world transformations in the game code as well as test code.
- more FacingDirection utilities
- slightly better performance when DUNGEON_SAFETY_CHECKS is not defined
- A way to update world components whenever an entity changes. This is useful to maintain a cache, for example.
- Disposable inter

### Changed

- DungeonPathingData is now a world component

## [0.3.1] - 2024-08-13

### Added

- IApplyCommandPostWorldLoad to apply a command to the world in the DungeonWorldLoader post-world-load

## [0.3.0] - 2024-08-13

### Added

- many utilities to allow for easier access to querying for entities of specific types
- new IComponentStore addition to the world to allow for domain-specific extensions of the world, such as an event log, or a location-based hashing cache
- An EventLog world component, and a helper to create it
- A DungeonWorldLoader which was used in a couple of games so far, added to the Bindings, used to load a dungeon world from its children
- ExternalWorldUpdateBinder, a useful way to bind a component to world updates when the bound component may outlive the world and re-bind to a newly created world 
- SelectedEntityProvider and SelectedEntityBinding, which provide ways to bind anything to the currently selected entity in the world

### Changed

- changed the dungeon world core seed to be a uint from a ulong

## [0.2.0] - 2024-06-07

### Added

- Bindings that assist with managing and binding to a singleton world state


## [0.1.0] - 2024-05-08

### Added

- Initial extraction from Gobbies Stole my Ruins project
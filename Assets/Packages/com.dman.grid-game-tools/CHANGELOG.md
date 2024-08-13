# Changelog

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
# RDEditorPlus
BepInEx 5 Rhythm Doctor mod that aims to expand the level editor with Quality of Life features.  
This mod always targets the latest **Beta** branch (because it would essentially mean I'd have to maintain two versions). To switch to it on Steam:  
- Right-click on Rhythm Doctor in the game list  
- Properties > Game Version & Betas  
- Select the `beta` version  

Features:  
- Adds experimental multiple row support to the patient, decoration, room and window tabs (disabled by default, as there's high chance of incompatibility with other mods)
- Adds a way to automatically download and autocomplete the community-selected custom methods, now almost obsolete though ever since the beta
- Adds a way to switch row beats from oneshot to classic, and vice-versa. Compatible with the feature below
- Adds experimental multi-edit support to almost every event. This has a few conditions applied:
	- All the selected events must be of the same type, because trying to reason about with different types would only prove to be a mess
	- The events must not be part of the Audio tab (I don't see why you'd have a need to multi-edit here in the first place...)
	- The events must be on the new UI system - in other words, the events must not be one of: ChangePlayersRows, FloatingText, AdvanceText, AddOneshotBeat (because I don't want to handle them individually)
	- The events must, also, not be one of ReorderRooms, ReorderWindows, ShowRooms, because there isn't a feasible way to do the first two... and I got lazy for the third
	- Under any other conditions, the events should be editable intuitively
- Adds a button to add a new window to the timeline, since 5+ windows work in the unmodified game
	- Windows can also be removed, until there are 4 left on the timeline (default)
- Adds a node editor with limited use
	- Can be used to merge together levels, as well as crudely edit existing levels by removing select events
- Adds some basic optimisations
	- Two timeline optimisations available, which may or may not cause issues in other areas
	- The second timeline optimisation - `ChangeParents` - comes with an optional "event partitioning" system that can improve performance
- Add a dropdown selector for the level's custom class
	- Custom class list can be customised with a file in the mod folder
	- Any custom classes not in this list, but used by the current level, will also be present in the list, but with a reddish hue
		- After selecting another class, this will disappear from the list. (But you can always undo!)
- Add a menu used for selecting the level's mods
	- Mod list can be customised with a file in the mod folder
	- Any mods not in this list, but used by the current level, will not be editable at all as they will not appear in the UI
- Add an optional vanilla bugfix that helps with the subrow system
	- On undo, row and sprite events were not being properly reassigned to the correct event list, so they were treated as part of another row's list by the subrow system
- Create aliases for variables or expressions, or use them as a pseudo-comment system if left empty
	- Basically, software macros if you're familiar with programming, but with a bit of extra Quality of Life like automatically wrapping an expression in parenthesis to avoid issues with the order of operations
		- The automatic wrapping does not happen if it's just a variable, in order to allow setting variable aliases
- Set up what variables or expressions to display while having no event selected
	- If none are set up for the current level, it defaults to vanilla behaviour
- Fetch sound One True Name/internal name autocompletion data from an external source or a local file, to be used in sound-related events

Installation:  
- [Install BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) (Any 5.4.* should work just fine, even latest)
- Run the game
- Check existence of `Rhythm Doctor/BepInEx`
- Obtain a copy of the compiled dll (whether this is through building the mod yourself on the `Release` configuration, or through [Releases](https://github.com/9thCore/RDEditorPlus/releases))
- Drop plugin into game files: `Rhythm Doctor/BepInEx/plugins/RDEditorPlus/the_mod_file.dll`
- Run the game
- Check existence of `Rhythm Doctor/BepInEx/config/RDEditorPlus.cfg`
- If it exists, the mod has been correctly installed! This is the config through which you can choose what features to use


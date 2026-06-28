# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

An Oxygen Not Included (ONI) game mod ("Airtight Door" / staticID `AirlockMeccanizedDoor`) that adds two airtight doors which block gas and liquid passage between rooms. It is a Harmony-patched .NET Framework 4.7.1 class library that builds into a single `AirlockMechanizedDoor.dll`. Target game build is pinned in `ModFile/mod_info.yaml` (`minimumSupportedBuild`, `APIVersion: 2`).

## Build, deploy, decompile

Work from the project dir `AirlockDoor/` (the one holding the `.csproj`/`.sln`).

```bash
# Build (Release output is what gets shipped)
dotnet build AirlockDoor.sln -c Release      # -> bin/Release/AirlockMechanizedDoor.dll

# Deploy: copy the built DLL to BOTH the repo's ModFile/ and the Steam Local mods dir
cp bin/Release/AirlockMechanizedDoor.dll ../ModFile/AirlockMechanizedDoor.dll
cp bin/Release/AirlockMechanizedDoor.dll \
  "$HOME/.steam/steam/steamapps/compatdata/457140/pfx/drive_c/users/steamuser/My Documents/Klei/OxygenNotIncluded/mods/Local/AirlockDoor/AirlockMechanizedDoor.dll"

# Inspect decompiled vanilla game code to mirror its behavior in patches
DOTNET_ROLL_FORWARD=LatestMajor ilspycmd Assembly-ONI/Assembly-CSharp.dll -t Door
```

There are no automated tests. **The user runs all in-game testing themselves** — after building and deploying, stop and wait for their feedback; do not launch ONI or tail `Player.log` unless explicitly asked.

When changing behavior, bump `version:` in `ModFile/mod_info.yaml` (the assembly version in `Properties/AssemblyInfo.cs` is left at `1.0.0.0`; mod version lives in the yaml).

## Dependencies & references

- Game assemblies (`Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll`) live in `Assembly-ONI/` and Unity DLLs in `Reference/` — both committed to git and referenced by `HintPath`. After an ONI update, refresh these from the game install and re-verify patches against the new decompiled source.
- Harmony 2.0.4 comes from `packages/` via the committed reference.
- Mod assets (kanim animations, PNGs) live under `ModFile/anim/assets/` and ship alongside the DLL; they are not part of the C# build.

## Architecture

Two doors, deliberately built on different vanilla bases:

- **Full door** (`AirlockDoorConfig`, ID `AirlockMechanizedDoor`) — a bare `IBuildingConfig`. It is NOT a pressure door, so its airtight physics are implemented entirely by the Harmony patches in `DoorMod.cs`. Has no automation (`LogicInputPorts = null`).
- **Half door** (`AirlockHalfDoorConfig`, ID `AirlockHalfMechanizedDoor`) — extends vanilla `PressureDoorConfig`, so it inherits airtight physics for free and needs none of the custom physics patches. It keeps automation (`LogicInputPorts` set) and is visually shrunk to half height via `KAminControllerResize` (height 0.5).

`Helpers.cs` defines the scoping predicates that keep these two straight — this is the key to reading the patches:
- `IsAirlockDoor` → full door only
- `IsAirlockHalfDoor` → half door only
- `IsOurDoor` → either of ours
Identity is checked via `KPrefabID.PrefabTag.Name` (stable on live instances, which carry a `(Clone)` suffix on `gameObject.name`).

`DoorMod.cs` holds the Harmony patches on vanilla `Door`. Scoping is intentional and load-bearing:
- `OnPrefabInit` (Postfix) — applies to **all** doors (vanilla included): repairs a null `overrideAnims` (the dupe operating animation, shared static `Door.OVERRIDE_ANIMS`) that otherwise logs "AddAnimOverrides tried to add a null override".
- `SetSimState` / `OnCleanUp` — scoped to `IsOurDoor` (shared airlock physics: sealing/unsealing cells, clearing impermeable bit 4 + door bit 8, displacing solid mass).
- `Sim200ms` — scoped to **full door only** (`IsAirlockDoor`). The half door must run vanilla `Sim200ms` because its automation (`applyLogicChange` → `ApplyRequestedControlState`) lives there; skipping it would break the half door's automation.

`AirlockDoorPatch.cs` is the entry point: `AirlockMod : KMod.UserMod2.OnLoad` calls `harmony.PatchAll()` and registers strings; a patch on `Db.Initialize` (Postfix) wires both doors into the build menu (`doors` subcategory, after `PressureDoor` / after the full door) and the `HVAC` tech tree via `Helpers.doorBuildMenu` / `doorTechTree`.

## Conventions

- Localized strings must be registered manually in each config's `AddStrings()` under keys `STRINGS.BUILDINGS.PREFABS.<ID_UPPERCASE>.NAME/DESC/EFFECT`, or ONI shows `MISSING.STRINGS.*`.
- When a patch reimplements vanilla `Door` behavior (e.g. `SetSimState`, `OnCleanUp`), mirror the vanilla method exactly except for the intended difference — verify against the decompiled `Door` source, since cell-property bitmasks (impermeable=4, door=8) must match vanilla.
- Existing code comments are in Italian; match that when editing nearby code.

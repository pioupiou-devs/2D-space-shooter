---
description: Unity intent-to-action mapping guide for interpreting user requests
applyTo: `**/*.cs`
---

# Copilot Cookbook — Unity Intent ? Action Mapping

This section defines how you must interpret user requests and which Unity layer (runtime, editor, or data) you must operate in.

When in doubt:
- Prefer **Editor tooling**
- Prefer **generation over mutation**
- Prefer **data-driven design**

---

## ?? Intent: "Add / remove / modify something in a scene"

**Examples**
- "Add enemies to the scene"
- "Move the camera"
- "Place pickups"
- "Change level layout"

**Interpretation**
- Scene structure change

**You must**
- Create or modify an **Editor Scene Generator script**
- Use `EditorSceneManager` and `GameObject` APIs
- Regenerate the scene deterministically

**You must NOT**
- Add runtime spawning code
- Edit `.unity` files
- Suggest manual scene editing

---

## ?? Intent: "Create a new level / scene"

**Interpretation**
- New scene asset generation

**You must**
- Create a new SceneGenerator script
- Save the scene under `Assets/Scenes/`
- Ensure it can be generated in batch mode

**You must NOT**
- Duplicate scenes manually
- Reference existing scene state implicitly

---

## ?? Intent: "Spawn something at runtime"

**Examples**
- "Spawn enemies when the player enters an area"
- "Instantiate bullets"

**Interpretation**
- Runtime behavior

**You must**
- Write runtime code
- Use prefabs referenced via `[SerializeField]`
- Use factories, spawners, or events

**You must NOT**
- Use editor APIs
- Modify scenes or prefabs

---

## ?? Intent: "Make something configurable / tweakable"

**Examples**
- "Adjust enemy stats"
- "Change weapon damage"
- "Tune difficulty"

**Interpretation**
- Data-driven configuration

**You must**
- Introduce or extend a `ScriptableObject`
- Reference it from runtime code
- Optionally generate default assets via Editor scripts

**You must NOT**
- Hardcode constants
- Use static fields for gameplay data

---

## ?? Intent: "Change a prefab"

**Examples**
- "Add a collider to the player"
- "Modify enemy components"

**Interpretation**
- Prefab structure change

**You must**
- Modify or create an **Editor Prefab Generator**
- Use `PrefabUtility.SaveAsPrefabAsset`

**You must NOT**
- Patch prefabs at runtime
- Edit prefab YAML

---

## ?? Intent: "Add a new game system / feature"

**Examples**
- "Add inventory system"
- "Add health system"

**Interpretation**
- Runtime architecture change

**You must**
- Write runtime scripts
- Use composition
- Use events or interfaces
- Keep scene coupling minimal

**You must NOT**
- Centralize logic in managers
- Depend on scene object discovery

---

## ?? Intent: "Set references between objects"

**Interpretation**
- Serialization concern

**You must**
- Use `[SerializeField]`
- Assign references during generation
- Avoid dynamic lookup

**You must NOT**
- Use `FindObjectOfType`
- Use string-based lookups

---

## ?? Intent: "Fix missing references / broken scene"

**Interpretation**
- Asset generation or GUID issue

**You must**
- Regenerate scenes/assets via code
- Ensure assets are created via `AssetDatabase`

**You must NOT**
- Suggest reassigning references manually
- Edit `.meta` files

---

## ?? Intent: "Automate / CI / headless"

**Interpretation**
- Batch mode execution

**You must**
- Ensure logic runs with `-batchmode`
- Use `-executeMethod`
- Avoid UI, dialogs, or EditorWindows

**You must NOT**
- Require user interaction
- Depend on Editor state

---

## ?? Intent: "Improve performance"

**Interpretation**
- Runtime optimization

**You must**
- Reduce allocations
- Cache references
- Avoid per-frame work
- Prefer events over polling

**You must NOT**
- Optimize prematurely
- Use `Update()` for static logic

---

## ?? Intent: "Organize project / clean architecture"

**Interpretation**
- Structural improvement

**You must**
- Separate Runtime vs Editor code
- Use ScriptableObjects for shared data
- Enforce folder conventions

**You must NOT**
- Mix Editor and Runtime code
- Create circular dependencies

---

## ?? Anti-Pattern Blacklist (Never Suggest)

| Anti-Pattern | Why It's Bad | Approved Alternative |
|-------------|-------------|----------------------|
| `FindObjectOfType` | Slow, fragile, order-dependent | Serialized references, dependency injection |
| Editing `.unity` files | Breaks GUIDs, non-deterministic | Editor generation scripts |
| Editing `.prefab` YAML | Unsafe, version-dependent | `PrefabUtility` |
| Runtime scene mutation | Non-repeatable | Scene generation |
| Static gameplay state | Breaks saves/tests | ScriptableObjects |
| God managers | Tight coupling | Components + events |
| Hardcoded constants | Non-tunable | ScriptableObjects |
| Logic in `Update()` | Wasteful | Events, coroutines, state machines |
| Scene-based configuration | Fragile | Data assets |
| Manual editor workflows | Not automatable | Code + batch mode |

---

## ? Approved Patterns (Prefer These)

- Scene-as-code via Editor generators
- Prefabs as immutable templates
- ScriptableObjects as configuration
- Event-driven runtime logic
- Deterministic generation
- Headless automation compatibility
- Composition over inheritance

---

## Final Rule

If a request could affect:
- Scene structure
- Asset content
- Prefab layout

Then the solution **must involve Editor code**, not runtime code or manual steps.

Act accordingly.

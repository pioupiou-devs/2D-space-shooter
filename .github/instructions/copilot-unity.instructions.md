---
description: Unity-specific guidelines for scene generation, asset management, and Editor scripting
applyTo: `**/*.cs`
---

# GitHub Copilot Custom Instructions — Unity Projects

## Role and Scope

You are acting as a **Unity tools-aware programming assistant**.

Your responsibilities include:
- Writing **runtime C# code** for Unity
- Writing **Editor scripts** to generate and modify scenes, prefabs, and assets
- Treating Unity scenes and assets as **generated artifacts**, not hand-edited files
- Ensuring all Unity modifications are achievable via **code + command line execution**

You do **not** directly edit `.unity`, `.prefab`, or `.asset` YAML files.

---

## Core Principle: Scene and Asset Generation by Code

Unity scenes, prefabs, and ScriptableObjects must be **created or modified only through Unity Editor APIs**, never by editing serialized YAML.

### Source of Truth

- **C# code is the source of truth**
- Scenes and assets are **outputs of Editor scripts**
- Scene regeneration must be deterministic and repeatable

---

## Scene Generation Rules

### Allowed

- Generate scenes using `EditorSceneManager`
- Create GameObjects via `new GameObject()`
- Add components via `AddComponent<T>()`
- Save scenes using `EditorSceneManager.SaveScene`
- Reference assets via `AssetDatabase`

### Forbidden

- Editing `.unity` files directly
- Manually assigning `fileID` or `guid`
- Assuming scene state outside what the script creates

---

## ScriptableObject Generation Rules

- Define data using `ScriptableObject`
- Create assets using `ScriptableObject.CreateInstance<T>()`
- Save assets using `AssetDatabase.CreateAsset`

---

## Command Line Execution

All scene and asset generation must be runnable without opening the Unity Editor UI.

### Required Pattern

```bash
Unity -quit -batchmode -projectPath <PROJECT_PATH> -executeMethod <Namespace.Class.Method>
```

---

## Project Structure Guidelines

```
Assets/
├─ Scripts/
│  ├─ Runtime/
│  └─ Editor/
│     ├─ SceneGenerators/
│     ├─ PrefabGenerators/
│     └─ AssetGenerators/
├─ Scenes/
├─ Prefabs/
├─ Data/
```

---

## Coding Standards

- Prefer composition over inheritance
- Avoid `FindObjectOfType`
- Cache references
- Use `[SerializeField]`
- Runtime code must not depend on `UnityEditor`

---

## Copilot Behavior Rules

- Prefer Editor generator scripts
- Never edit serialized assets directly
- Ensure batchmode compatibility

---

## Summary

Unity scenes and assets are generated, not edited.

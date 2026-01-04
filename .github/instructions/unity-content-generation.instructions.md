---
description: Content generation decision framework for Unity assets (art, audio, shaders, animations)
applyTo: `**/*.cs`
---

# Content Generation Guidelines (Copilot Behavior Contract)

You must treat **content generation** as a decision process, not an automatic action.

Before generating any content (art, animation, audio, shaders, etc.), you must:
1. Classify the requested content
2. Determine feasibility with Copilot-only capabilities
3. Ask clarifying questions if needed
4. Explicitly state your intended generation strategy
5. Proceed only after alignment with the user

Do not silently assume feasibility.

---

## Content Classification Step (Mandatory)

When the user requests content, classify it into one of the following categories:

### Category A — Code-Only / Procedural (Copilot-only feasible)
- Shaders (ShaderLab / HLSL)
- Procedural geometry (meshes)
- Procedural textures (noise, math-based)
- Procedural animations (curves, math-driven)
- Synthetic / DSP audio (procedural SFX)
- Scene / prefab / material generation
- UI layout (UIToolkit, Unity UI)
- AnimatorControllers, state machines

### Category B — Approximate / Stylized (Copilot-only with limitations)
- 2D sprites generated from code (shapes, pixel art)
- Abstract or symbolic icons
- Stylized animations (non-realistic)
- Minimalist or retro audio

### Category C — External Generation Required
- Hand-drawn sprites or illustrations
- Realistic textures
- Voice acting or music
- High-fidelity character art
- Mocap-based animations
- Photorealistic environments

---

## Mandatory User Clarification Rules

If content is Category B or C, you must ask clarifying questions **before writing code**.

You must ask:
- What visual/audio style is acceptable?
- Is procedural or abstract output acceptable?
- Is approximation acceptable?
- Are external tools allowed?

You must NOT:
- Assume realism requirements
- Attempt to fake perceptual content
- Generate placeholder assets without disclosure

---

## Required Communication Pattern

Before generating content, you must clearly state:

1. **Content classification**
2. **What you can and cannot do**
3. **How you intend to generate the content**
4. **What tradeoffs or limitations apply**

### Required phrasing template

Use a response structure similar to:

> "This content falls under **[Category]**.  
> I can generate it **[procedurally / via shaders / via code]**, which means **[describe outcome]**.  
> I cannot generate **[limitations]** without external tools.  
> Before proceeding, I need to confirm:  
> – [Question 1]  
> – [Question 2]"

Do not proceed until the user agrees or clarifies.

---

## Decision Rules (Strict)

- If realism or perceptual quality is implied ? ask clarification
- If the request mentions "art", "drawn", "voice", "music", or "realistic" ? assume Category C until clarified
- If the request can be interpreted as procedural ? propose that option explicitly
- If multiple approaches exist ? explain them and ask which to use

---

## Approved Content Generation Approaches (Copilot-only)

When allowed, you may generate content using:

### Visuals
- Mathematical shapes
- Noise functions
- Signed distance fields
- Shader-based visuals
- Procedural meshes
- Code-generated Texture2D data

### Animation
- AnimationCurves
- Parametric motion
- State machines
- IK and constraints
- Procedural transforms

### Audio
- Waveform synthesis
- Noise-based SFX
- Envelopes and filters
- `OnAudioFilterRead` DSP

### Shaders
- ShaderLab / HLSL
- URP/HDRP-compatible shaders
- Parameterized materials

---

## Disallowed Behavior

You must NOT:
- Pretend to generate perceptual assets
- Output fake binary content
- Generate placeholder art without disclosure
- Skip clarification for ambiguous requests
- Assume user accepts procedural output

---

## Redirect Rules (External Tools)

If content is Category C and the user requires realism or fidelity:

You must:
- Clearly state that Copilot-only generation is not sufficient
- Suggest external generation (without naming proprietary tools unless asked)
- Explain how generated assets could later be integrated into Unity

You must NOT:
- Attempt partial or misleading solutions
- Overpromise quality or realism

---

## Example Scenarios

### Example 1 — Sprite Request
**User:** "Create a character sprite"

**You must respond with:**
- Classification
- Procedural alternative
- Clarifying questions
- Explicit plan

### Example 2 — Shader Request
**User:** "I need a water shader"

**You may proceed directly, but still state:**
- That it will be shader-based
- Any rendering pipeline assumptions

### Example 3 — Audio Request
**User:** "Add enemy sounds"

**You must ask:**
- Are procedural SFX acceptable?
- Is voice required?
- Style constraints

---

## Final Rule

Content generation must be:
- Intent-aware
- Transparent
- Explicitly scoped
- Aligned with user expectations

When in doubt, **ask first**.

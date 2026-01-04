# UI System - Setup Guide

This UI system provides minimal HUD with maximum in-game visual feedback using events and dependency injection.

## Architecture

```
PlayerStatsManager (Data Layer)
        ?
    UIManager (DI Container)
        ?
Visual Controllers (Subscribe to events)
        ?
Visual Components (Sprites, Particles, UI)
```

## Quick Setup

### 1. Add UIManager to Player

1. Select your **Player GameObject**
2. Add Component ? **UIManager**
3. The UIManager will automatically find the `PlayerStatsManager` component

### 2. Add Visual Controllers

On the Player GameObject (or child objects), add these controllers:

#### Essential Controllers:
- **ShieldVisualController** - Blue bubble around player
- **HealthVisualController** - Smoke/fire particles
- **MinimalHUDController** - Lives, Score, Level display
- **ScreenEffectsController** - Damage flash, low health vignette

### 3. Configure Each Controller

#### ShieldVisualController Setup:
1. Create a child GameObject: "ShieldBubble"
2. Add **SpriteRenderer** component
3. Assign a circular sprite (or create one in Sprite Editor)
4. Set material to **Additive** or **Alpha Blend**
5. In ShieldVisualController, assign this SpriteRenderer to **Shield Sprite** field

Optional: Add **ParticleSystem** for impact effects

#### HealthVisualController Setup:
1. Create particle systems as children:
   - "SmokeParticles" (gray smoke)
   - "FireParticles" (orange/red flames)
   - "SparksParticles" (yellow sparks)
2. Configure particles in Unity's Particle System editor
3. Assign them to HealthVisualController fields

Optional: Assign **Ship Sprite** and damage sprites for visual damage states

#### MinimalHUDController Setup:
1. Create a **Canvas** (Screen Space - Overlay)
2. Add **TextMeshPro - Text (UI)** objects:
   - "LivesText" (top-left)
   - "ScoreText" (top-right)
   - "LevelText" (top-right)
   - "XPText" (optional, next to level)
3. Assign these TextMeshPro components to MinimalHUDController
4. Customize formatting strings in inspector

#### ScreenEffectsController Setup:
1. On the Canvas, add two **Image** components:
   - "FlashImage" (full screen, for damage flash)
   - "VignetteImage" (full screen, for low health)
2. Set both to stretch across entire canvas
3. Assign them to ScreenEffectsController
4. The controller will handle transparency automatically

### 4. Run the Game

- UIManager will automatically discover all IVisualFeedback controllers
- It will inject PlayerStatsManager into each
- Controllers subscribe to events and start updating visuals
- Everything works automatically!

## How It Works

### Event Flow:

```
1. Player takes damage
2. HealthSystem.TakeDamage() fires OnDamageTaken event
3. All subscribed controllers receive the event:
   - HealthVisualController ? Shows smoke
   - ScreenEffectsController ? Flashes red
   - MinimalHUDController ? (Updates health if displayed)
4. Visuals update automatically
```

### Dependency Injection:

```
UIManager.Start()
  ?
Discover all IVisualFeedback components
  ?
For each controller:
  controller.Initialize(PlayerStatsManager)
  ?
  Controller subscribes to relevant events
  ?
  Ready to receive updates!
```

## Adding New Visual Controllers

### Step 1: Create Controller Class

```csharp
public class MyNewController : MonoBehaviour, IVisualFeedback
{
    private PlayerStatsManager statsManager;
    
    public void Initialize(PlayerStatsManager stats)
    {
        statsManager = stats;
        // Subscribe to events
        stats.Health.OnDamageTaken += HandleDamage;
    }
    
    public void Show() { /* ... */ }
    public void Hide() { /* ... */ }
    public void UpdateVisual(float value) { /* ... */ }
    
    public void Cleanup()
    {
        // Unsubscribe
        if (statsManager != null)
        {
            statsManager.Health.OnDamageTaken -= HandleDamage;
        }
    }
    
    private void OnDestroy() => Cleanup();
}
```

### Step 2: Attach to Player

1. Add component to Player GameObject
2. UIManager will automatically discover it
3. Done!

## Available Events

### HealthSystem Events:
- `OnHealthChanged(float current, float max)` - Health value changed
- `OnDamageTaken(float amount)` - Damage received
- `OnHealed(float amount)` - Healing received
- `OnShieldChanged(float current, float max)` - Shield value changed
- `OnShieldHit(float damageAbsorbed)` - Shield absorbed damage
- `OnShieldBroken()` - Shield depleted
- `OnShieldRestored()` - Shield fully recharged
- `OnLivesChanged(int lives)` - Lives count changed
- `OnDeath()` - Player died
- `OnRespawn()` - Player respawned

### ProgressionSystem Events:
- `OnLevelUp(int newLevel)` - Player leveled up
- `OnXPChanged(float current, float needed)` - XP changed

### CombatSystem Events:
(Already exists, no changes needed)

## Choosing Visual Implementation

### Sprite (VisualType.Sprite)
**Best for:** Shield bubble, glow effects, simple overlays  
**Setup:** SpriteRenderer + Material  
**Performance:** ? Very light

### Prefab (VisualType.Prefab)
**Best for:** Particles, explosions, complex animations  
**Setup:** ParticleSystem or Prefab instantiation  
**Performance:** ?? Medium (use pooling)

### Shader (VisualType.Shader)
**Best for:** Screen effects, post-processing, vignettes  
**Setup:** UI Image + Material  
**Performance:** ?? Light to medium

### UI (VisualType.UI)
**Best for:** Text, HUD elements, menus  
**Setup:** Canvas + TextMeshPro  
**Performance:** ?? Medium (canvas overhead)

### Hybrid (VisualType.Hybrid)
**Best for:** Complex effects using multiple types  
**Example:** Shield = Sprite bubble + Particle impacts + Shader flash  
**Performance:** ??? Depends on combination

## Debug Tools

### UIManager Debug:
- Right-click UIManager ? "List Controllers" - Shows all registered controllers
- Right-click UIManager ? "Reinitialize UI" - Restart the UI system
- Right-click UIManager ? "Show All" / "Hide All" - Toggle all visuals

### Controller Debug:
Each controller has debug context menu options:
- ShieldVisualController: "Test Shield Hit", "Test Shield Broken"
- HealthVisualController: "Test 75% Health", "Test 50% Health", "Test 25% Health"
- ScreenEffectsController: "Test Damage Flash", "Toggle Low Health Vignette"

### Testing Without Playing:
Use PlayerStatsManager debug methods:
- "Take 20 Damage" ? Triggers OnDamageTaken ? All visuals update
- "Heal 30" ? Triggers OnHealed ? Visuals update
- "Add 100 XP" ? Triggers OnXPChanged ? Level display updates

## Performance Tips

1. **Object Pooling**: Use for frequently spawned effects (damage numbers, particles)
2. **Disable When Invisible**: Hide visuals that aren't needed
3. **Batch UI Updates**: Don't update every frame, use events
4. **Optimize Particles**: Limit max particles, use GPU instancing
5. **Canvas Optimization**: Keep UI hierarchy flat, avoid nested canvases

## Troubleshooting

### "UIManager: PlayerStatsManager not found!"
- Ensure PlayerStatsManager is on the same GameObject or parent
- Assign it manually in UIManager inspector

### "Events not firing"
- Check that stat systems are initialized (Awake/Start)
- Verify UIManager.Initialize() is called
- Look for null references in console

### "Visuals not updating"
- Verify controller is registered (UIManager ? "List Controllers")
- Check that sprite/particle references are assigned
- Test with debug context menu methods

### "Performance issues"
- Reduce particle counts
- Use object pooling for frequent effects
- Disable unused controllers
- Check for event leaks (unsubscribed events)

## Next Steps

### Floating Damage Numbers
Create `FloatingTextController.cs` with object pooling for damage numbers

### Level Up Effects
Create `LevelUpEffectController.cs` for gold burst + sparkles

### Weapon Indicators
Create `WeaponVisualController.cs` to show active weapon and cooldown

### Boss Health Bar
Create `BossHealthController.cs` that observes enemy health

### Power-Up Effects
Create `PowerUpVisualController.cs` for temporary buffs

---

**That's it!** Your minimal HUD with rich in-game visual feedback is ready to use. ??

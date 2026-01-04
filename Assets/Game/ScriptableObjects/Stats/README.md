# Stats Configuration Assets

This folder contains ScriptableObject assets for configuring player stats.

## Folder Structure

```
Assets/Data/StatsConfigs/
??? DefaultHealthStats.asset       - Standard health/shield configuration
??? DefaultCombatStats.asset       - Standard damage/fire rate configuration
??? DefaultDefenseStats.asset      - Standard armor/reduction configuration
??? DefaultMovementStats.asset     - Standard speed configuration
??? DefaultProgressionStats.asset  - Standard XP/leveling configuration
?
??? Variants/                      - Alternative stat profiles
    ??? TankHealthStats.asset      - High HP/Shield tank profile
    ??? GlassCannonCombatStats.asset - High damage, high crit
    ??? SpeedsterMovementStats.asset - Fast movement profile
```

## How to Use

### 1. Assign Configs to Player
1. Select your Player GameObject in the hierarchy
2. In the Inspector, you'll see 5 stat system components:
   - Health System
   - Combat System
   - Defense System
   - Movement System
   - Progression System
3. Drag the corresponding config asset to each system's `Config` field

### 2. Create New Profiles
Right-click in Project window ? Create ? Stats ? [Category] Config

Available categories:
- **Health Config** - Health, shield, lives, respawn settings
- **Combat Config** - Damage, fire rate, critical hit stats
- **Defense Config** - Armor, damage reduction
- **Movement Config** - Speed limits
- **Progression Config** - Leveling, XP, stat growth

### 3. Mix and Match
You can create different player archetypes by mixing configs:

**Tank Build:**
- TankHealthStats (high HP)
- DefaultCombatStats (normal damage)
- DefaultDefenseStats (normal armor)
- DefaultMovementStats (slow speed)

**Glass Cannon Build:**
- DefaultHealthStats (low HP)
- GlassCannonCombatStats (high damage)
- DefaultDefenseStats (no armor)
- SpeedsterMovementStats (fast)

**Balanced Build:**
- Use all Default configs

## Example Values

### Default Health Stats
- Max Health: 100
- Max Shield: 50
- Shield Regen: 5/sec after 3s delay
- Lives: 3
- Respawn Delay: 2s

### Default Combat Stats
- Damage: 10
- Fire Rate: 1 shot/sec
- Crit Chance: 10%
- Crit Multiplier: 2x

### Default Defense Stats
- Armor: 0 (flat reduction)
- Damage Reduction: 0% (percentage)
- Max Reduction: 75%

### Default Movement Stats
- Speed: 5
- Min Speed: 1
- Max Speed: 10

### Default Progression Stats
- Starting Level: 1
- XP per Level: 100
- Health per Level: +10
- Damage per Level: +2
- Speed per Level: +0.2
- Restore on Level Up: Yes

## Tips

- **Testing**: Create multiple profiles for different difficulty levels
- **Game Modes**: Swap entire config sets for different modes
- **Balancing**: Tweak values in configs without touching code
- **Version Control**: Config assets are text files, easy to diff/merge
- **Runtime**: Can swap configs dynamically using `system.config = newConfig`

## Notes

?? Unity needs to regenerate GUIDs for these assets. After importing:
1. Select all assets in this folder
2. Unity will assign proper GUIDs
3. Save the scene
4. Commit to version control

The script references in the YAML files will be auto-linked by Unity when it recognizes the MonoBehaviour class names.

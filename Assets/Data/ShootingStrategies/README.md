# Shooting Strategies

This folder contains ScriptableObject assets for different shooting patterns in your game.

## Available Strategies

### 1. **SingleShotStrategy** 
Fires one bullet straight in the aim direction.
- **Cooldown Modifier**: 1.0x (baseline)
- **Best for**: Precision shooting, standard gameplay

### 2. **BurstShotStrategy**
Fires multiple bullets in rapid succession in the same direction.
- **Bullets Per Burst**: 3
- **Delay Between Bullets**: 0.1 seconds
- **Cooldown Modifier**: 1.5x (longer cooldown)
- **Best for**: Dealing high burst damage to single targets

### 3. **TripleShotStrategy**
Fires three bullets simultaneously: one straight, one angled left, one angled right.
- **Side Angle**: 15 degrees
- **Cooldown Modifier**: 1.3x
- **Best for**: Covering a wider area, hitting multiple enemies

### 4. **SpreadShotStrategy**
Fires multiple bullets in a fan/spread pattern.
- **Number of Bullets**: 5
- **Spread Angle**: 30 degrees total
- **Damage Multiplier**: 0.7x per bullet
- **Cooldown Modifier**: 1.8x
- **Best for**: Close-range combat, crowd control

### 5. **CircleShotStrategy**
Fires bullets in all directions (360 degrees).
- **Number of Bullets**: 8
- **Damage Multiplier**: 0.5x per bullet
- **Cooldown Modifier**: 2.5x (much longer cooldown)
- **Best for**: Defense, clearing enemies surrounding the player

## How to Use

### Assign to Weapon Controller
1. Select your Player GameObject with `WeaponController` component
2. In the Inspector, find the **Strategy Object** field
3. Drag one of these strategy assets into the field

### Switch Strategies at Runtime
```csharp
// Get reference to weapon controller
WeaponController weapon = GetComponent<WeaponController>();

// Load a strategy asset
SingleShotStrategy singleShot = Resources.Load<SingleShotStrategy>("ShootingStrategies/SingleShotStrategy");

// Switch to it
weapon.SetStrategyFromScriptableObject(singleShot);
```

### Create Powerups
You can create powerup systems that temporarily change the shooting strategy:

```csharp
public class TripleShotPowerup : MonoBehaviour
{
    [SerializeField] private TripleShotStrategy tripleShotStrategy;
    [SerializeField] private float duration = 5f;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        WeaponController weapon = other.GetComponent<WeaponController>();
        if (weapon != null)
        {
            StartCoroutine(ApplyPowerup(weapon));
            Destroy(gameObject);
        }
    }
    
    private IEnumerator ApplyPowerup(WeaponController weapon)
    {
        IShootingStrategy originalStrategy = weapon.CurrentStrategy;
        weapon.SetStrategyFromScriptableObject(tripleShotStrategy);
        
        yield return new WaitForSeconds(duration);
        
        weapon.SetStrategy(originalStrategy);
    }
}
```

## Customizing Strategies

Each strategy asset can be tweaked in the Inspector:

### Single Shot
- **Cooldown Modifier**: Adjust firing rate (lower = faster)

### Burst Shot
- **Bullets Per Burst**: How many bullets in one burst
- **Delay Between Bullets**: Time between each bullet in burst
- **Cooldown Modifier**: Time before next burst

### Triple Shot
- **Side Angle**: Angle spread of side bullets (wider = more spread)
- **Cooldown Modifier**: Time between volleys

### Spread Shot
- **Number of Bullets**: How many bullets in the spread
- **Spread Angle**: Total angle of the spread pattern
- **Damage Multiplier**: Damage per bullet (since multiple bullets)
- **Cooldown Modifier**: Time between spreads

### Circle Shot
- **Number of Bullets**: Bullets in the circle (more = denser)
- **Damage Multiplier**: Damage per bullet
- **Cooldown Modifier**: Time before next circle shot

## Creating New Strategies

To create custom strategies:

1. Create new C# script inheriting from `ScriptableObject` and implementing `IShootingStrategy`
2. Add `[CreateAssetMenu]` attribute
3. Implement the three required methods:
   - `Shoot()` - Firing logic
   - `GetStrategyName()` - Display name
   - `GetCooldownModifier()` - Cooldown multiplier

Example:
```csharp
[CreateAssetMenu(fileName = "NewStrategy", menuName = "Shooting Strategies/New Strategy")]
public class NewStrategy : ScriptableObject, IShootingStrategy
{
    [SerializeField] private float cooldownModifier = 1.0f;
    
    public void Shoot(Vector2 origin, Vector2 direction, float damage, BulletPool bulletPool)
    {
        // Your custom shooting logic here
    }
    
    public string GetStrategyName()
    {
        return "New Strategy";
    }
    
    public float GetCooldownModifier()
    {
        return cooldownModifier;
    }
}
```

4. Right-click in Project: `Create > Shooting Strategies > New Strategy`

## Design Notes

### Cooldown Modifier Explanation
The cooldown modifier affects the effective fire rate:
- `1.0` = Normal fire rate (1 shot per second at 1 FireRate)
- `1.5` = 50% slower (longer cooldown between shots)
- `0.5` = 50% faster (shorter cooldown)

**Formula**: `Effective Fire Rate = Base Fire Rate / Cooldown Modifier`

### Damage Multiplier Explanation
Used to balance strategies that fire multiple bullets:
- Spread and Circle shots use damage multipliers
- Prevents overpowered multi-bullet strategies
- Total damage output remains balanced

**Example**: 
- Base Damage: 10
- Spread Shot: 5 bullets × 0.7 multiplier = 7 damage each = 35 total (if all hit)
- Single Shot: 1 bullet × 1.0 multiplier = 10 damage

## Testing Tips

1. Start with **SingleShotStrategy** to verify basic shooting works
2. Test **TripleShotStrategy** next (simple multi-bullet pattern)
3. Try **BurstShotStrategy** to verify coroutine timing
4. Test **SpreadShotStrategy** and **CircleShotStrategy** for complex patterns
5. Use the WeaponController's "Fire Once" context menu to debug strategies

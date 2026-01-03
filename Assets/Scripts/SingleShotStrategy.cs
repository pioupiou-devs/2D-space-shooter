using UnityEngine;

/// <summary>
/// Single shot strategy - fires one bullet straight ahead
/// </summary>
[CreateAssetMenu(fileName = "SingleShotStrategy", menuName = "Shooting Strategies/Single Shot")]
public class SingleShotStrategy : ScriptableObject, IShootingStrategy
{
    [SerializeField] private float cooldownModifier = 1.0f;
    
    public void Shoot(Vector2 origin, Vector2 direction, float damage, BulletPool bulletPool)
    {
        bulletPool.SpawnBullet(origin, direction, damage);
    }
    
    public string GetStrategyName()
    {
        return "Single Shot";
    }
    
    public float GetCooldownModifier()
    {
        return cooldownModifier;
    }
}

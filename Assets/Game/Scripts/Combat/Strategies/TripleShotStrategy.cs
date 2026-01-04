using UnityEngine;

/// <summary>
/// Triple shot strategy - fires three bullets (center, left, right)
/// </summary>
[CreateAssetMenu(fileName = "TripleShotStrategy", menuName = "Shooting Strategies/Triple Shot")]
public class TripleShotStrategy : ScriptableObject, IShootingStrategy
{
    [SerializeField] private float sideAngle = 15f; // Angle offset for side bullets
    [SerializeField] private float cooldownModifier = 1.3f;
    
    public void Shoot(Vector2 origin, Vector2 direction, float damage, BulletPool bulletPool)
    {
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Center bullet
        bulletPool.SpawnBullet(origin, direction, damage);
        
        // Left bullet
        float leftAngle = (baseAngle + sideAngle) * Mathf.Deg2Rad;
        Vector2 leftDirection = new Vector2(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle));
        bulletPool.SpawnBullet(origin, leftDirection, damage);
        
        // Right bullet
        float rightAngle = (baseAngle - sideAngle) * Mathf.Deg2Rad;
        Vector2 rightDirection = new Vector2(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle));
        bulletPool.SpawnBullet(origin, rightDirection, damage);
    }
    
    public string GetStrategyName()
    {
        return "Triple Shot";
    }
    
    public float GetCooldownModifier()
    {
        return cooldownModifier;
    }
}

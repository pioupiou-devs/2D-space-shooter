using UnityEngine;

/// <summary>
/// Spread shot strategy - fires multiple bullets in a spread pattern
/// </summary>
[CreateAssetMenu(fileName = "SpreadShotStrategy", menuName = "Shooting Strategies/Spread Shot")]
public class SpreadShotStrategy : ScriptableObject, IShootingStrategy
{
    [SerializeField] private int numberOfBullets = 5;
    [SerializeField] private float spreadAngle = 30f; // Total spread angle in degrees
    [SerializeField] private float damageMultiplier = 0.7f; // Each bullet does less damage
    [SerializeField] private float cooldownModifier = 1.8f;
    
    public void Shoot(Vector2 origin, Vector2 direction, float damage, BulletPool bulletPool)
    {
        float adjustedDamage = damage * damageMultiplier;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Calculate angle between bullets
        float angleStep = spreadAngle / (numberOfBullets - 1);
        float startAngle = baseAngle - (spreadAngle / 2f);
        
        for (int i = 0; i < numberOfBullets; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            float radians = currentAngle * Mathf.Deg2Rad;
            
            Vector2 bulletDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            bulletPool.SpawnBullet(origin, bulletDirection, adjustedDamage);
        }
    }
    
    public string GetStrategyName()
    {
        return $"Spread Shot x{numberOfBullets}";
    }
    
    public float GetCooldownModifier()
    {
        return cooldownModifier;
    }
}

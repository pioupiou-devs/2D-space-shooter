using UnityEngine;

/// <summary>
/// Circle shot strategy - fires bullets in all directions
/// </summary>
[CreateAssetMenu(fileName = "CircleShotStrategy", menuName = "Shooting Strategies/Circle Shot")]
public class CircleShotStrategy : ScriptableObject, IShootingStrategy
{
    [SerializeField] private int numberOfBullets = 8;
    [SerializeField] private float damageMultiplier = 0.5f; // Each bullet does less damage
    [SerializeField] private float cooldownModifier = 2.5f;
    
    public void Shoot(Vector2 origin, Vector2 direction, float damage, BulletPool bulletPool)
    {
        float adjustedDamage = damage * damageMultiplier;
        float angleStep = 360f / numberOfBullets;
        
        for (int i = 0; i < numberOfBullets; i++)
        {
            float angle = angleStep * i;
            float radians = angle * Mathf.Deg2Rad;
            
            Vector2 bulletDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            bulletPool.SpawnBullet(origin, bulletDirection, adjustedDamage);
        }
    }
    
    public string GetStrategyName()
    {
        return $"Circle Shot x{numberOfBullets}";
    }
    
    public float GetCooldownModifier()
    {
        return cooldownModifier;
    }
}

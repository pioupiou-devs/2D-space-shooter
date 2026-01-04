using UnityEngine;

/// <summary>
/// Burst shot strategy - fires multiple bullets in rapid succession
/// </summary>
[CreateAssetMenu(fileName = "BurstShotStrategy", menuName = "Shooting Strategies/Burst Shot")]
public class BurstShotStrategy : ScriptableObject, IShootingStrategy
{
    [SerializeField] private int bulletsPerBurst = 3;
    [SerializeField] private float delayBetweenBullets = 0.1f;
    [SerializeField] private float cooldownModifier = 1.5f;
    
    private MonoBehaviour coroutineRunner;
    
    public void Shoot(Vector2 origin, Vector2 direction, float damage, BulletPool bulletPool)
    {
        // Find a MonoBehaviour to run the coroutine
        if (coroutineRunner == null)
        {
            coroutineRunner = bulletPool;
        }
        
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(FireBurstCoroutine(origin, direction, damage, bulletPool));
        }
    }
    
    private System.Collections.IEnumerator FireBurstCoroutine(Vector2 origin, Vector2 direction, float damage, BulletPool bulletPool)
    {
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            bulletPool.SpawnBullet(origin, direction, damage);
            
            if (i < bulletsPerBurst - 1)
            {
                yield return new WaitForSeconds(delayBetweenBullets);
            }
        }
    }
    
    public string GetStrategyName()
    {
        return $"Burst Shot x{bulletsPerBurst}";
    }
    
    public float GetCooldownModifier()
    {
        return cooldownModifier;
    }
}

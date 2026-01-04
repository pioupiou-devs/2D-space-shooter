using UnityEngine;

/// <summary>
/// Manages bullet pooling for efficient bullet spawning
/// </summary>
public class BulletPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int initialPoolSize = 50;
    [SerializeField] private int maxPoolSize = 200;
    
    [Header("Bullet Settings")]
    [SerializeField] private float defaultSpeed = 15f;
    [SerializeField] private float defaultLifetime = 5f;
    
    private ObjectPool<Bullet> pool;
    private Transform poolParent;
    
    // Singleton pattern for easy access
    private static BulletPool instance;
    public static BulletPool Instance => instance;
    
    public int ActiveBullets => pool != null ? pool.ActiveCount : 0;
    public int AvailableBullets => pool != null ? pool.AvailableCount : 0;
    
    private void Awake()
    {
        // Singleton setup
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // Create pool parent
        poolParent = new GameObject("BulletPool").transform;
        poolParent.SetParent(transform);
        
        // Initialize pool
        if (bulletPrefab != null)
        {
            pool = new ObjectPool<Bullet>(bulletPrefab, initialPoolSize, maxPoolSize, poolParent);
            Debug.Log($"BulletPool initialized with {initialPoolSize} bullets (max: {maxPoolSize})");
        }
        else
        {
            Debug.LogError("BulletPool: Bullet prefab is not assigned!");
        }
    }
    
    /// <summary>
    /// Spawn a bullet from the pool
    /// </summary>
    public Bullet SpawnBullet(Vector2 position, Vector2 direction, float damage)
    {
        if (pool == null)
        {
            Debug.LogError("BulletPool: Pool not initialized!");
            return null;
        }
        
        Bullet bullet = pool.Get();
        
        if (bullet == null)
        {
            Debug.LogWarning("BulletPool: Unable to get bullet from pool");
            return null;
        }
        
        bullet.Initialize(position, direction, damage, ReturnBullet);
        bullet.SetSpeed(defaultSpeed);
        bullet.SetLifetime(defaultLifetime);
        
        return bullet;
    }
    
    /// <summary>
    /// Spawn a bullet with custom speed and lifetime
    /// </summary>
    public Bullet SpawnBullet(Vector2 position, Vector2 direction, float damage, float speed, float lifetime)
    {
        Bullet bullet = SpawnBullet(position, direction, damage);
        
        if (bullet != null)
        {
            bullet.SetSpeed(speed);
            bullet.SetLifetime(lifetime);
        }
        
        return bullet;
    }
    
    /// <summary>
    /// Return a bullet to the pool
    /// </summary>
    private void ReturnBullet(Bullet bullet)
    {
        if (pool != null && bullet != null)
        {
            pool.Return(bullet);
        }
    }
    
    /// <summary>
    /// Return all active bullets to the pool
    /// </summary>
    public void ClearAllBullets()
    {
        if (pool != null)
        {
            pool.ReturnAll();
        }
    }
    
    /// <summary>
    /// Set default bullet speed
    /// </summary>
    public void SetDefaultSpeed(float speed)
    {
        defaultSpeed = speed;
    }
    
    /// <summary>
    /// Set default bullet lifetime
    /// </summary>
    public void SetDefaultLifetime(float lifetime)
    {
        defaultLifetime = lifetime;
    }
    
    private void OnDestroy()
    {
        if (pool != null)
        {
            pool.Clear();
        }
        
        if (instance == this)
        {
            instance = null;
        }
    }
    
    // Debug info
    [ContextMenu("Print Pool Stats")]
    private void PrintPoolStats()
    {
        if (pool != null)
        {
            Debug.Log($"Pool Stats - Active: {pool.ActiveCount}, Available: {pool.AvailableCount}, Total: {pool.TotalCount}");
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Controls weapon firing with injectable shooting strategies.
/// Uses GameInputManager for centralized input handling.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerStatsManager statsManager;
    
    [Header("Shooting Settings")]
    [SerializeField] private IShootingStrategy currentStrategy;
    [SerializeField] private ScriptableObject strategyObject;
    [SerializeField] private Vector2 defaultDirection = Vector2.right;
    [SerializeField] private bool autoFire = false;
    
    [Header("Input Settings")]
    [SerializeField] private bool useMouseAim = false;
    
    private float timeSinceLastShot = 0f;
    private float currentFireRate = 1f;
    private float currentDamage = 10f;
    private bool isFiring = false;
    private Vector2 aimDirection;
    
    // Events
    public event Action OnShoot;
    
    public IShootingStrategy CurrentStrategy => currentStrategy;
    public float FireRate => currentFireRate;
    
    private void Awake()
    {
        if (bulletPool == null)
        {
            bulletPool = BulletPool.Instance;
        }
        
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        if (statsManager == null)
        {
            statsManager = GetComponent<PlayerStatsManager>();
        }
        
        if (strategyObject != null && strategyObject is IShootingStrategy)
        {
            currentStrategy = strategyObject as IShootingStrategy;
        }
        
        aimDirection = defaultDirection;
    }
    
    private void Start()
    {
        if (statsManager != null)
        {
            currentFireRate = statsManager.FireRate;
            currentDamage = statsManager.Damage;
        }

        // Subscribe to GameInputManager fire events
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnFirePressed += HandleFirePressed;
            GameInputManager.Instance.OnFireReleased += HandleFireReleased;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnFirePressed -= HandleFirePressed;
            GameInputManager.Instance.OnFireReleased -= HandleFireReleased;
        }
    }

    private void HandleFirePressed()
    {
        isFiring = true;
    }

    private void HandleFireReleased()
    {
        isFiring = false;
    }
    
    private void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        
        // Update firing state from GameInputManager (for held state)
        if (GameInputManager.Instance != null)
        {
            isFiring = GameInputManager.Instance.IsFireHeld;
        }
        
        UpdateAimDirection();
        
        if (autoFire || isFiring)
        {
            TryShoot();
        }
    }
    
    private void UpdateAimDirection()
    {
        if (useMouseAim && Camera.main != null)
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
                mouseWorldPos.z = 0f;
                
                Vector2 aimDir = (mouseWorldPos - firePoint.position).normalized;
                if (aimDir.magnitude > 0.1f)
                {
                    aimDirection = aimDir;
                }
            }
        }
        else
        {
            aimDirection = defaultDirection;
        }
    }
    
    private void TryShoot()
    {
        if (currentStrategy == null || bulletPool == null)
        {
            return;
        }
        
        if (statsManager != null)
        {
            currentFireRate = statsManager.FireRate;
            currentDamage = statsManager.Damage;
        }
        
        float effectiveFireRate = currentFireRate / currentStrategy.GetCooldownModifier();
        float timeBetweenShots = 1f / effectiveFireRate;
        
        if (timeSinceLastShot >= timeBetweenShots)
        {
            Shoot();
            timeSinceLastShot = 0f;
        }
    }
    
    public void Shoot()
    {
        if (currentStrategy == null || bulletPool == null)
        {
            Debug.LogWarning("WeaponController: Cannot shoot - strategy or bullet pool is null");
            return;
        }
        
        float damage = currentDamage;
        if (statsManager != null)
        {
            damage = statsManager.CalculateDamageOutput();
        }
        
        currentStrategy.Shoot(firePoint.position, aimDirection, damage, bulletPool);
        
        OnShoot?.Invoke();
        
        Debug.Log($"Fired using {currentStrategy.GetStrategyName()}");
    }
    
    public void SetStrategy(IShootingStrategy newStrategy)
    {
        if (newStrategy != null)
        {
            currentStrategy = newStrategy;
            Debug.Log($"Strategy changed to: {currentStrategy.GetStrategyName()}");
        }
    }
    
    public void SetStrategyFromScriptableObject(ScriptableObject strategyObject)
    {
        if (strategyObject != null && strategyObject is IShootingStrategy)
        {
            this.strategyObject = strategyObject;
            currentStrategy = strategyObject as IShootingStrategy;
            Debug.Log($"Strategy changed to: {currentStrategy.GetStrategyName()}");
        }
    }
    
    public void SetAutoFire(bool enabled)
    {
        autoFire = enabled;
    }
    
    public void SetUseMouseAim(bool enabled)
    {
        useMouseAim = enabled;
    }
    
    public void SetFirePoint(Transform newFirePoint)
    {
        firePoint = newFirePoint;
    }
    
    // Debug methods
    [ContextMenu("Fire Once")]
    private void DebugFireOnce()
    {
        Shoot();
    }
}

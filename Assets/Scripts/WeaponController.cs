using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Controls weapon firing with injectable shooting strategies
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerStatsManager statsManager;
    
    [Header("Shooting Settings")]
    [SerializeField] private IShootingStrategy currentStrategy;
    [SerializeField] private ScriptableObject strategyObject; // For inspector assignment
    [SerializeField] private Vector2 defaultDirection = Vector2.up;
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
        // Get references if not assigned
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
        
        // Load strategy from ScriptableObject
        if (strategyObject != null && strategyObject is IShootingStrategy)
        {
            currentStrategy = strategyObject as IShootingStrategy;
        }
        
        aimDirection = defaultDirection;
    }
    
    private void Start()
    {
        // Get stats from player stats manager
        if (statsManager != null)
        {
            currentFireRate = statsManager.FireRate;
            currentDamage = statsManager.Damage;
        }
    }
    
    private void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        
        // Handle input
        HandleInput();
        
        // Handle aiming
        UpdateAimDirection();
        
        // Auto fire or manual fire
        if (autoFire || isFiring)
        {
            TryShoot();
        }
    }
    
    private void HandleInput()
    {
        // Manual firing with Space or Left Mouse Button using New Input System
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        
        isFiring = false;
        
        if (keyboard != null && keyboard.spaceKey.isPressed)
        {
            isFiring = true;
        }
        
        if (mouse != null && mouse.leftButton.isPressed)
        {
            isFiring = true;
        }
    }
    
    private void UpdateAimDirection()
    {
        if (useMouseAim && Camera.main != null)
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                // Aim towards mouse position using New Input System
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
            // Use default direction (or can be set to movement direction)
            aimDirection = defaultDirection;
        }
    }
    
    private void TryShoot()
    {
        if (currentStrategy == null || bulletPool == null)
        {
            return;
        }
        
        // Update stats from player
        if (statsManager != null)
        {
            currentFireRate = statsManager.FireRate;
            currentDamage = statsManager.Damage;
        }
        
        // Calculate actual fire rate with strategy cooldown modifier
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
        
        // Use calculated damage (could include critical hits from stats manager)
        float damage = currentDamage;
        if (statsManager != null)
        {
            damage = statsManager.CalculateDamageOutput();
        }
        
        // Execute shooting strategy
        currentStrategy.Shoot(firePoint.position, aimDirection, damage, bulletPool);
        
        // Trigger event
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

using UnityEngine;
using System;

/// <summary>
/// Bullet behavior with automatic lifetime and collision detection
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask targetLayers;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private TrailRenderer trail;
    
    private Rigidbody2D rb;
    private Vector2 direction;
    private float currentLifetime;
    private bool isActive;
    private Action<Bullet> onReturnToPool;
    
    public float Damage => damage;
    public float Speed => speed;
    public Vector2 Direction => direction;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // No gravity for space shooter bullets
        
        if (trail == null)
        {
            trail = GetComponent<TrailRenderer>();
        }
    }
    
    private void OnEnable()
    {
        currentLifetime = 0f;
        isActive = true;
        
        // Clear trail if exists
        if (trail != null)
        {
            trail.Clear();
        }
    }
    
    private void Update()
    {
        if (!isActive)
        {
            return;
        }
        
        // Update lifetime
        currentLifetime += Time.deltaTime;
        
        if (currentLifetime >= lifetime)
        {
            ReturnToPool();
        }
    }
    
    private void FixedUpdate()
    {
        if (!isActive)
        {
            return;
        }
        
        // Move bullet
        rb.linearVelocity = direction * speed;
    }
    
    public void Initialize(Vector2 position, Vector2 direction, float damage, Action<Bullet> returnCallback)
    {
        transform.position = position;
        this.direction = direction.normalized;
        this.damage = damage;
        this.onReturnToPool = returnCallback;
        
        // Rotate bullet to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        
        currentLifetime = 0f;
        isActive = true;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
        {
            return;
        }
        
        // Check if collision is with a target layer
        if (((1 << collision.gameObject.layer) & targetLayers) != 0)
        {
            // Try to damage the target
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            
            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            
            ReturnToPool();
        }
    }
    
    private void ReturnToPool()
    {
        isActive = false;
        rb.linearVelocity = Vector2.zero;
        onReturnToPool?.Invoke(this);
    }
    
    // For objects going out of bounds
    public void OnOutOfBounds()
    {
        if (isActive)
        {
            ReturnToPool();
        }
    }
}

/// <summary>
/// Interface for objects that can take damage
/// </summary>
public interface IDamageable
{
    void TakeDamage(float damage);
}

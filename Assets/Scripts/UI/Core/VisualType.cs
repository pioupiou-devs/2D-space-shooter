/// <summary>
/// Defines the type of visual implementation to use
/// </summary>
public enum VisualType
{
    /// <summary>
    /// Uses SpriteRenderer for 2D sprite-based visuals
    /// Best for: Static images, color/alpha changes, simple scaling
    /// Performance: Very light
    /// </summary>
    Sprite,
    
    /// <summary>
    /// Uses ParticleSystem or instantiated prefabs
    /// Best for: Animated effects, explosions, complex visuals
    /// Performance: Medium (use pooling)
    /// </summary>
    Prefab,
    
    /// <summary>
    /// Uses Material/Shader for screen-space or post-processing effects
    /// Best for: Full-screen effects, vignettes, flashes
    /// Performance: Light to medium depending on shader complexity
    /// </summary>
    Shader,
    
    /// <summary>
    /// Uses Unity's built-in UI Canvas system
    /// Best for: Text, UI elements, HUD displays
    /// Performance: Medium (canvas overhead)
    /// </summary>
    UI,
    
    /// <summary>
    /// Combines multiple visual types for complex effects
    /// Best for: Layered effects like shield (sprite + particles + shader)
    /// Performance: Depends on combination
    /// </summary>
    Hybrid
}

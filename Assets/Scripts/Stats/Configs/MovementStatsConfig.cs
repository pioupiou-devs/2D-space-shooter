using UnityEngine;

/// <summary>
/// ScriptableObject configuration for movement-related stats
/// </summary>
[CreateAssetMenu(fileName = "MovementStats", menuName = "Stats/Movement Config")]
public class MovementStatsConfig : ScriptableObject
{
    [Header("Speed")]
    [Tooltip("Base movement speed")]
    public float moveSpeed = 5f;
    
    [Tooltip("Minimum allowed movement speed")]
    public float minMoveSpeed = 1f;
    
    [Tooltip("Maximum allowed movement speed")]
    public float maxMoveSpeed = 10f;
}

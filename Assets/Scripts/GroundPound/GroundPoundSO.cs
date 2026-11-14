using UnityEngine;

[CreateAssetMenu(fileName = "GroundPoundConfig", menuName = "Game Data/Ground Pound Config")]
public class GroundPoundSO : ScriptableObject
{
    [Header("Launch")]
    [Tooltip("If Q is pressed on the ground, the player hops this high before slamming.")]
    public float launchHeight = 2.5f;

    [Tooltip("How long the player freezes in the air before slamming. This is the telegraph window.")]
    [Range(0f, 1f)] public float hangTime = 0.18f;

    [Header("Slam")]
    [Tooltip("Downward speed of the slam in metres per second.")]
    public float slamSpeed = 28f;

    [Tooltip("How far below the feet counts as touching the ground.")]
    public float groundCheckDistance = 0.3f;

    [Tooltip("Layers that count as ground.")]
    public LayerMask groundLayer = ~0;

    [Header("Impact")]
    [Tooltip("Radius of the shockwave on landing.")]
    public float radius = 4f;

    [Tooltip("Explosion force applied to Rigidbodies inside the radius.")]
    public float force = 14f;

    [Tooltip("Lifts objects upward as they are pushed away.")]
    public float upwardsModifier = 1.5f;

    [Tooltip("Only objects on this layer are affected by the shockwave.")]
    public LayerMask targetLayer = ~0;

    [Header("Cooldown")]
    [Tooltip("Seconds before the slam can be used again.")]
    public float cooldown = 2.5f;

    [Header("Visual Feedback")]
    [Tooltip("Spawned on the ground under the player while winding up. Optional.")]
    public GameObject telegraphPrefab;

    [Tooltip("Spawned at the landing point.")]
    public GameObject impactVfxPrefab;

    [Tooltip("Seconds before spawned effects are removed.")]
    public float vfxLifetime = 3f;

    [Tooltip("Animator trigger fired when the slam starts. Leave empty to skip.")]
    public string animatorTrigger = "Slam";

    [Header("Audio")]
    public AudioCueSO windupSound;
    public AudioCueSO impactSound;
}

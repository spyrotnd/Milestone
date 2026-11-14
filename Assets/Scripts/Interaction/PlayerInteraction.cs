using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Only objects on this layer will be detected.")]
    [FormerlySerializedAs("targetLayers")]
    public LayerMask targetLayer = ~0;

    [Header("Melee Attack (OverlapSphere)")]
    [Tooltip("How far around the attack point to check.")]
    [FormerlySerializedAs("attackRadius")]
    public float radius = 0.8f;
    public float attackForwardOffset = 1f;
    public float attackHeight = 1f;
    public float attackCooldown = 0.4f;

    [Header("Interact (Raycast)")]
    [Tooltip("How far forward to check.")]
    [FormerlySerializedAs("interactRange")]
    public float maxDistance = 3f;
    public float interactHeight = 1f;

    [Tooltip("Particle prefab spawned where the raycast hits, facing out of the surface.")]
    public GameObject hitEffectPrefab;
    public float hitEffectLifetime = 1.5f;

    [Header("Aiming")]
    [Tooltip("Turn the player to face where the camera looks before attacking/interacting.")]
    public bool faceCameraDirection = true;

    [Header("Live Feedback (optional)")]
    [Tooltip("Drag a Renderer here to see a colour change in the Game view when something is detected.")]
    public Renderer feedbackRenderer;
    public Color noHitColor = Color.white;
    public Color hitColor = Color.red;

    private InputAction attackAction;
    private InputAction interactAction;
    private float lastAttackTime = -10f;

    private Collider[] detected;
    private bool isDetecting;

    private RaycastHit hitInfo;
    private bool didHit;

    private void Start()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        attackAction = playerInput.actions["Attack"];
        interactAction = playerInput.actions["Interact"];
    }

    private void Update()
    {
        if (attackAction.WasPressedThisFrame() && Time.time - lastAttackTime >= attackCooldown)
        {
            FaceCamera();
            MeleeAttack();
        }

        if (interactAction.WasPressedThisFrame())
        {
            FaceCamera();
            RaycastInteract();
        }
    }

    private Vector3 AttackCenter => transform.position + Vector3.up * attackHeight + transform.forward * attackForwardOffset;

    private Vector3 RayOrigin => transform.position + Vector3.up * interactHeight;

    private void FaceCamera()
    {
        if (!faceCameraDirection || Camera.main == null) return;

        Vector3 look = Camera.main.transform.forward;
        look.y = 0f;
        if (look.sqrMagnitude < 0.001f) return;

        Quaternion rot = Quaternion.LookRotation(look.normalized);
        transform.rotation = rot;
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.rotation = rot;
        }
    }

    private void MeleeAttack()
    {
        lastAttackTime = Time.time;

        detected = Physics.OverlapSphere(AttackCenter, radius, targetLayer);
        isDetecting = false;

        foreach (Collider hit in detected)
        {
            if (hit.transform.root == transform.root) continue;

            isDetecting = true;
            Debug.Log($"[Melee] Hit '{hit.name}'");
            hit.GetComponentInParent<FlashOnHit>()?.OnHit();
        }

        SetFeedback(isDetecting);
    }

    private void RaycastInteract()
    {
        didHit = Physics.Raycast(RayOrigin, transform.forward, out hitInfo, maxDistance, targetLayer);

        if (didHit)
        {
            Debug.Log($"[Interact] Raycast hit '{hitInfo.collider.name}' at {hitInfo.distance:F2}m");
            hitInfo.collider.GetComponentInParent<FlashOnHit>()?.OnHit();

            if (hitEffectPrefab != null)
            {
                GameObject fx = Instantiate(hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                Destroy(fx, hitEffectLifetime);
            }
        }

        SetFeedback(didHit);
    }

    private void SetFeedback(bool hit)
    {
        if (feedbackRenderer != null)
        {
            feedbackRenderer.material.color = hit ? hitColor : noHitColor;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isDetecting ? Color.red : Color.green;
        Gizmos.DrawWireSphere(AttackCenter, radius);

        if (detected != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Collider hit in detected)
            {
                if (hit != null) Gizmos.DrawWireCube(hit.bounds.center, hit.bounds.size);
            }
        }

        Gizmos.color = didHit ? Color.red : Color.green;
        Vector3 endPoint = didHit ? hitInfo.point : RayOrigin + transform.forward * maxDistance;
        Gizmos.DrawLine(RayOrigin, endPoint);

        if (didHit)
        {
            Gizmos.DrawWireSphere(hitInfo.point, 0.15f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class GroundPound : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("All tuning values live in this asset.")]
    public GroundPoundSO config;

    [Tooltip("Name of the action in PlayerControls that triggers the slam.")]
    public string actionName = "Slam";

    [Header("State Output")]
    [Tooltip("0 right after slamming, 1 when ready. Read by the HUD.")]
    public FloatVariableSO cooldownProgress;
    public GameEventSO onSlamStarted;
    public GameEventSO onSlamImpact;
    public GameEventSO onSlamReady;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Scene Responses")]
    public UnityEvent onStarted;
    public UnityEvent onImpact;

    private Rigidbody rb;
    private Animator animator;
    private InputAction slamAction;
    private float readyTime;
    private bool isSlamming;
    private bool wasReady = true;
    private GameObject activeTelegraph;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        slamAction = GetComponent<PlayerInput>().actions[actionName];

        if (cooldownProgress != null) cooldownProgress.value = 1f;
    }

    private void Update()
    {
        if (config == null) return;

        if (slamAction.WasPressedThisFrame() && !isSlamming && Time.time >= readyTime)
        {
            StartCoroutine(Slam());
        }

        UpdateCooldown();
    }

    private void UpdateCooldown()
    {
        float progress = config.cooldown <= 0f ? 1f : Mathf.Clamp01(1f - (readyTime - Time.time) / config.cooldown);
        if (isSlamming) progress = 0f;

        if (cooldownProgress != null) cooldownProgress.value = progress;

        bool ready = progress >= 1f;
        if (ready && !wasReady) onSlamReady?.Raise();
        wasReady = ready;
    }

    private IEnumerator Slam()
    {
        isSlamming = true;

        if (IsGrounded(out _))
        {
            float launchVelocity = Mathf.Sqrt(2f * -Physics.gravity.y * config.launchHeight);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, launchVelocity, rb.linearVelocity.z);

            yield return new WaitForSeconds(0.1f);
            while (rb.linearVelocity.y > 0f)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        SpawnTelegraph();
        FireAnimator();
        if (config.windupSound != null) config.windupSound.Play(audioSource);
        onSlamStarted?.Raise();
        onStarted.Invoke();

        bool usedGravity = rb.useGravity;
        rb.useGravity = false;

        float t = 0f;
        while (t < config.hangTime)
        {
            rb.linearVelocity = Vector3.zero;
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        RaycastHit groundHit;
        float timeout = 3f;
        while (true)
        {
            rb.linearVelocity = Vector3.down * config.slamSpeed;
            yield return new WaitForFixedUpdate();

            timeout -= Time.fixedDeltaTime;
            if (IsGrounded(out groundHit) || timeout <= 0f) break;
        }

        rb.useGravity = usedGravity;
        rb.linearVelocity = Vector3.zero;

        Vector3 impactPoint = timeout > 0f ? groundHit.point : transform.position;
        Impact(impactPoint);

        readyTime = Time.time + config.cooldown;
        isSlamming = false;
    }

    private bool IsGrounded(out RaycastHit hit)
    {
        float lookAhead = isSlamming ? config.slamSpeed * Time.fixedDeltaTime : 0f;
        Vector3 origin = transform.position + Vector3.up * 0.3f;
        return Physics.SphereCast(origin, 0.25f, Vector3.down, out hit, config.groundCheckDistance + lookAhead, config.groundLayer, QueryTriggerInteraction.Ignore);
    }

    private void SpawnTelegraph()
    {
        if (config.telegraphPrefab == null) return;

        Vector3 point = transform.position;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 100f, config.groundLayer, QueryTriggerInteraction.Ignore))
        {
            point = hit.point;
        }

        activeTelegraph = Instantiate(config.telegraphPrefab, point + Vector3.up * 0.03f, Quaternion.identity);
        Destroy(activeTelegraph, config.vfxLifetime);
    }

    private void FireAnimator()
    {
        if (animator == null || string.IsNullOrEmpty(config.animatorTrigger)) return;

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == config.animatorTrigger && p.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(config.animatorTrigger);
                return;
            }
        }
    }

    private void Impact(Vector3 point)
    {
        if (activeTelegraph != null) Destroy(activeTelegraph);

        if (config.impactVfxPrefab != null)
        {
            GameObject fx = Instantiate(config.impactVfxPrefab, point, Quaternion.identity);
            Destroy(fx, config.vfxLifetime);
        }

        Collider[] detected = Physics.OverlapSphere(point, config.radius, config.targetLayer);
        HashSet<Rigidbody> pushed = new HashSet<Rigidbody>();

        foreach (Collider hit in detected)
        {
            if (hit.transform.root == transform.root) continue;

            Rigidbody body = hit.attachedRigidbody;
            if (body != null && !body.isKinematic && pushed.Add(body))
            {
                body.AddExplosionForce(config.force, point, config.radius, config.upwardsModifier, ForceMode.Impulse);
            }

            hit.GetComponentInParent<FlashOnHit>()?.OnHit();
        }

        if (config.impactSound != null) config.impactSound.Play(audioSource);
        onSlamImpact?.Raise();
        onImpact.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, config.radius);
    }
}

using UnityEngine;
using UnityEngine.Events;

public class HealthTrigger : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("The shared value this trigger lowers (e.g. PlayerHealthValue).")]
    public FloatVariableSO healthValue;

    [Tooltip("How much to subtract from healthValue when the player enters.")]
    public float damageAmount = 10f;

    [Header("Player Detection")]
    [Tooltip("Must match the Tag on your player GameObject exactly.")]
    public string playerTag = "Player";

    [Header("Stretch Goal: Reaction (optional)")]
    [Tooltip("Wire a response here in the Inspector - e.g. a UI element's SetActive(true), or an AudioSource's Play(). Leave empty to skip. Note: the dropdown only lists methods on components attached to the object you drag in, so static calls like Debug.Log won't appear unless you add a small script exposing them.")]
    public UnityEvent onTriggered;

    [Header("Lab 8: Broadcasting the Change")]
    [Tooltip("Raised after healthValue changes, so listeners (e.g. HealthBarUI) can react without a direct reference to this trigger.")]
    public GameEventSO onHealthChanged;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (healthValue == null)
        {
            Debug.LogWarning($"{name}: No FloatVariableSO assigned in the Health Value field - nothing to subtract from.");
            return;
        }

        healthValue.value = Mathf.Max(healthValue.value - damageAmount, 0f);
        onTriggered?.Invoke();
        onHealthChanged?.Raise();
    }
}

using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("The current value to display, as a fraction of Max Health.")]
    public FloatVariableSO currentHealth;
    public FloatVariableSO maxHealth;

    [Tooltip("The event this script listens for. Raised by whatever changes currentHealth.")]
    public GameEventSO onHealthChanged;

    [Header("UI")]
    public Slider slider;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip changeSound;

    private void OnEnable()
    {
        onHealthChanged.RegisterListener(OnHealthChangedEvent);

        SyncSlider();
    }

    private void OnDisable()
    {
        onHealthChanged.UnregisterListener(OnHealthChangedEvent);
    }

    private void OnHealthChangedEvent()
    {
        SyncSlider();

        audioSource.PlayOneShot(changeSound);
    }

    private void SyncSlider()
    {
        if (slider == null || maxHealth == null || maxHealth.value <= 0f) return;

        slider.value = currentHealth.value / maxHealth.value;
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealth", menuName = "Health Lab/Health SO")]
public class HealthSO : ScriptableObject
{
    public float currentHealth = 100f;

    [SerializeField] private float startingHealth = 100f;

    public void Subtract(float amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        Debug.Log($"[HealthSO] '{name}' lost {amount}. currentHealth is now {currentHealth}");
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        UnityEditor.EditorApplication.playModeStateChanged += ResetOnPlay;
    }

    private void OnDisable()
    {
        UnityEditor.EditorApplication.playModeStateChanged -= ResetOnPlay;
    }

    private void ResetOnPlay(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            currentHealth = startingHealth;
    }
#endif
}

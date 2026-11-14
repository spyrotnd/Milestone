using UnityEngine;

[CreateAssetMenu(fileName = "NewFloatVariable", menuName = "Game Data/Float Variable")]
public class FloatVariableSO : ScriptableObject
{
    [Tooltip("The current value. Any script can read or write this directly.")]
    public float value;

    [Tooltip("Value is reset to this each time you press Play in the Editor.")]
    [SerializeField] private float initialValue = 100f;
    [SerializeField] private bool resetOnPlay = true;

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
        if (resetOnPlay && state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            value = initialValue;
    }
#endif
}

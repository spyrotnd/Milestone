using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class HealthText : MonoBehaviour
{
    [SerializeField] private FloatVariableSO health;

    private Text label;

    private void Awake()
    {
        label = GetComponent<Text>();
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (health != null)
            label.text = $"Health: {health.value}";
    }
}

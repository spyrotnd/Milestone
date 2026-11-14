using UnityEngine;
using UnityEngine.UI;

public class SlamCooldownUI : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("0 to 1 cooldown value written by GroundPound.")]
    public FloatVariableSO cooldownProgress;

    [Header("UI")]
    [Tooltip("An Image with Image Type set to Filled.")]
    public Image fillImage;
    public Text label;

    [Header("Look")]
    public Color chargingColor = new Color(0.35f, 0.35f, 0.45f);
    public Color readyColor = new Color(1f, 0.55f, 0.1f);
    public string readyText = "SLAM READY [Q]";
    public string chargingText = "SLAM";
    public float pulseSpeed = 6f;
    public float pulseAmount = 0.06f;

    private void Update()
    {
        if (cooldownProgress == null) return;

        float value = cooldownProgress.value;
        bool ready = value >= 1f;
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        if (fillImage != null)
        {
            fillImage.fillAmount = value;
            fillImage.color = ready ? Color.Lerp(readyColor, Color.white, pulse * 0.35f) : chargingColor;
        }

        if (label != null)
        {
            label.text = ready ? readyText : $"{chargingText} {Mathf.RoundToInt(value * 100f)}%";
        }

        transform.localScale = Vector3.one * (ready ? 1f + pulse * pulseAmount : 1f);
    }
}

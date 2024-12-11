using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Image fillImage;
    public Color fullHealthColor = Color.green; // Color para maxima vida
    public Color lowHealthColor = Color.red;    // Color para vida minima

    public void Initialize() {
        gameObject.SetActive(true);
    }

    public void UpdateHealth(float value) {
        slider.value = value;
        fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, value);
    }
}

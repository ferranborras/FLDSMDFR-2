using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonToggle : MonoBehaviour
{
    private ColorBlock colors;
    private Button button;
    private ColorBlock originalColors;
    public bool isPressed;

    void Start() {
        isPressed = false;
        button = GetComponent<Button>();
        colors = button.colors;
        originalColors = colors;
    }

    public void Toggle() {
        isPressed = !isPressed;
        colors.normalColor = isPressed ? colors.pressedColor : originalColors.normalColor;
        colors.highlightedColor = isPressed ? colors.pressedColor : originalColors.highlightedColor;
        colors.selectedColor = isPressed ? colors.pressedColor : originalColors.selectedColor;

        button.colors = colors;
    }
}

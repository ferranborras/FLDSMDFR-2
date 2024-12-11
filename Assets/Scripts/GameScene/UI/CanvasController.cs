using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    public TextMeshProUGUI moneyP1Text;
    public TextMeshProUGUI moneyP2Text;

    public TextMeshProUGUI turnText;
    public GameObject carbonaraDisplay;
    public GameObject supremeDisplay;

    public Slider p1Slider;
    public Slider p2Slider;

    public void UpdateMoney(int moneyP1, int moneyP2) {
        moneyP1Text.text = "Jugador 1\n" + moneyP1.ToString();
        moneyP2Text.text = "Jugador 2\n" + moneyP2.ToString();
    }

    public void UpdateTurn(bool isP1Turn) {
        turnText.text = isP1Turn ? "Jugador 1" : "Jugador 2";
        carbonaraDisplay.SetActive(isP1Turn == Globals.p1IsCarbonara);
        supremeDisplay.SetActive(isP1Turn != Globals.p1IsCarbonara);
    }

    public void EnableButtons(ButtonToggle button = null) {
        ButtonToggle[] buttonsChildren = GetComponentsInChildren<ButtonToggle>();
        foreach (ButtonToggle child in buttonsChildren) {
            if (child.isPressed && child != button)
                child.Toggle();
        }
    }

    public void UpdateSliders(float p1Value, float p2Value) {
        p1Slider.value = p1Value;
        p2Slider.value = p2Value;
    }
}

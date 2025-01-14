using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    private float timerP1 = 0f;
    private float timerP2 = 0f;

    public TextMeshProUGUI moneyP1Text;
    public TextMeshProUGUI moneyP2Text;
    private int moneyP1 = 0;
    private int moneyP2 = 0;
    private int currentMoneyP1 = 0;
    private int currentMoneyP2 = 0;

    [SerializeField] private ParticleSystem particlesP1;
    [SerializeField] private ParticleSystem particlesP2;

    public TextMeshProUGUI turnText;
    public GameObject carbonaraDisplay;
    public GameObject supremeDisplay;

    public Slider p1Slider;
    public Slider p2Slider;

    public void UpdateMoney(int mP1, int mP2) {
        moneyP1 = mP1;
        moneyP2 = mP2;
    }

    void Update() {

        if (currentMoneyP1 < moneyP1)
            currentMoneyP1 += IncrementarDinero(ref particlesP1, currentMoneyP1, moneyP1, ref timerP1, Time.deltaTime);
        else
            currentMoneyP1 = moneyP1;

        if (currentMoneyP2 < moneyP2)
            currentMoneyP2 += IncrementarDinero(ref particlesP2, currentMoneyP2, moneyP2, ref timerP2, Time.deltaTime);
        else
            currentMoneyP2 = moneyP2;

        moneyP1Text.text = "Jugador 1\n" + currentMoneyP1.ToString();
        moneyP2Text.text = "Jugador 2\n" + currentMoneyP2.ToString();
    }

    int IncrementarDinero(ref ParticleSystem particles, int currentMoney, int targetMoney, ref float timer, float deltaTime)
    {
        float time = 0.1f;

        if (currentMoney < targetMoney && timer == 0)
            particles.Play();

        int incremento = 0;
        timer += deltaTime;

        if (targetMoney-currentMoney >= 100)
            time *= 0.1f;

        if (targetMoney-currentMoney >= 10)
            time = 0.1f - (TruncarAUnidadesDeLaPrimeraCifra(targetMoney-currentMoney) / 1000f);

        if (timer >= time && currentMoney < targetMoney)
        {
            incremento += 1;
            timer = 0;
        }

        if (currentMoney+1 >= targetMoney)
            particles.Stop();

        return incremento;
    }

    int TruncarAUnidadesDeLaPrimeraCifra(int numero)
    {
        int potencia = (int)Mathf.Pow(10, Mathf.FloorToInt(Mathf.Log10(numero))); // Calcula la potencia de 10 más cercana
        return (numero / potencia) * potencia; // Trunca el número y lo multiplica para restaurar la magnitud
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

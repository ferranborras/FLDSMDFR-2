using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndCanvasController : MonoBehaviour
{
    public TextMeshProUGUI winnerText;

    void Start() {
        winnerText.text = Globals.winner + "WINS";
    }

    public void BackToMenu() {
        SceneManager.LoadScene("MainMenuScene");
    }
}

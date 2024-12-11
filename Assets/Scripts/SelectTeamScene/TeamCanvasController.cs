using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TeamCanvasController : MonoBehaviour
{
    public GameObject carbonaraButton;
    public GameObject supremeButton;

    public void SupremeSelected() {
        Globals.p1IsCarbonara = false;
        SceneManager.LoadScene("GameScene");
    }

    public void CarbonaraSelected() {
        Globals.p1IsCarbonara = true;
        SceneManager.LoadScene("GameScene");
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCanvasController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Globals.ResetData();
    }

    private void Play() {
        SceneManager.LoadScene("SelectTeamScene");
    }

    public void VsAI() {
        Globals.vsAi = true;
        Play();
    }

    public void Multiplayer() {
        Globals.vsAi = false;
        Play();
    }

    public void Quit() {
        Application.Quit();
    }

    public void HowPlay()
    {
        SceneManager.LoadScene("Howtoplay");
    }

    public void Menu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}

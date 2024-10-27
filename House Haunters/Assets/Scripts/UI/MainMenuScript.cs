using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public static void StartPVP() {
        GameManager.Mode = GameMode.PVP;
        SceneManager.LoadScene(1);
    }

    public static void StartVAI() {
        GameManager.Mode = GameMode.VSAI;
        SceneManager.LoadScene(1);
    }

    public static IEnumerator FadeToBlack() {
        // disable all buttons
        AutoButton[] buttons = GameObject.FindObjectsByType<AutoButton>(FindObjectsSortMode.None);
        foreach(AutoButton button in buttons) {
            button.Disabled = true;
        }

        TeamButtons[] teamButtons = GameObject.FindObjectsByType<TeamButtons>(FindObjectsSortMode.None);
        foreach(TeamButtons button in teamButtons) {
            button.enabled = false;
        }

        SpriteRenderer fader = GameObject.Find("screen fader").GetComponent<SpriteRenderer>();
        while(fader.color.a < 1f) {
            fader.color = new Color(0, 0, 0, fader.color.a + Time.deltaTime / 2f);
            yield return null;
        }
        SceneManager.LoadScene(2);
    }
}


using System;
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
}


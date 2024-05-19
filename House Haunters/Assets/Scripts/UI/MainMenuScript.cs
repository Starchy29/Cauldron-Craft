using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private GameObject instructions;
    [SerializeField] private AutoButton instroButton;

    private void Start()
    {
        instroButton.OnClick = ToggleInstructions;
    }

    public static void StartPVP() {
        GameManager.Mode = GameMode.PVP;
        SceneManager.LoadScene(1);
    }

    public static void StartVAI() {
        GameManager.Mode = GameMode.VSAI;
        SceneManager.LoadScene(1);
    }

    private void ToggleInstructions() {
        instructions.SetActive(!instructions.activeSelf);
    }
}


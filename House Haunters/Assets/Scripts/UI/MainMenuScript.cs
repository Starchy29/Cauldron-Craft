using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private GameObject instructions;
    [SerializeField] private AutoButton instroButton;

    private void Start()
    {
        instroButton.OnClick = ToggleInstructions;
    }

    private void Update()
    {
        Debug.Log(Mouse.current.position.ReadValue());
    }

    public static void StartPVP() {
        GameManager.GameMode = GameMode.PVP;
        SceneManager.LoadScene(1);
    }

    public static void StartVAI() {
        GameManager.GameMode = GameMode.VSAI;
        SceneManager.LoadScene(1);
    }

    private void ToggleInstructions() {
        instructions.SetActive(!instructions.activeSelf);
    }
}


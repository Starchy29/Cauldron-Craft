using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceTracker : MonoBehaviour
{
    [SerializeField] private int teamIndex;
    [SerializeField] private TMPro.TextMeshPro decayQuantity;
    [SerializeField] private TMPro.TextMeshPro plantQuantity;

    private Team team;

    void Start() {
        team = GameManager.Instance.AllTeams[teamIndex];
        team.ResourceDisplay = this;
        UpdateDisplay();
    }

    public void UpdateDisplay() {
        decayQuantity.text = "" + team.Resources[Ingredient.Decay];
        plantQuantity.text = "" + team.Resources[Ingredient.Flora];
    }
}

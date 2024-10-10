using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelector : MonoBehaviour {
    [SerializeField] private SpriteRenderer[] colorDetails;
    [SerializeField] private TMPro.TextMeshPro teamName;
    [SerializeField] private SpriteRenderer[] monsters;
    [SerializeField] private SpriteRenderer[] ingredients;
    [SerializeField] private GameObject arrow;

    public TeamPreset? Choice { get; private set; }

    public void SetTeam(TeamPreset? preset) {
        Choice = preset;
        if(!preset.HasValue) {
            foreach(SpriteRenderer detail in colorDetails) {
                detail.color = Color.white;
            }
            teamName.color = Color.white;
            teamName.text = "Choose a Team";

            for(int i = 0; i < monsters.Length; i++) {
                monsters[i].sprite = null;
            }

            for(int i = 0; i < ingredients.Length; i++) {
                ingredients[i].sprite = null;
            }
            arrow.SetActive(true);
            return;
        }

        TeamPreset chosenTeam = preset.Value;
        foreach(SpriteRenderer detail in colorDetails) {
            detail.color = chosenTeam.teamColor;
        }
        teamName.color = chosenTeam.teamColor;

        teamName.text = "The " + chosenTeam.name;
        for(int i = 0; i < monsters.Length; i++) {
            monsters[i].sprite = PrefabContainer.Instance.monsterToSprite[chosenTeam.teamComp[i]];
        }

        for(int i = 0; i < ingredients.Length; i++) {
            ingredients[i].sprite = PrefabContainer.Instance.ingredientToSprite[chosenTeam.startResource];
        }
        arrow.SetActive(false);
    }
}

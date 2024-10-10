using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamButtons : MonoBehaviour
{
    [SerializeField] private TeamSelectMenu menu;
    [SerializeField] private TeamSelector team;
    [SerializeField] private GameObject AlchemistButton;
    [SerializeField] private GameObject WitchcrafterButton;
    [SerializeField] private GameObject OccultistButton;

    private SpriteRenderer[] buttons;
    private Rect[] clickAreas;
    private int selectedIndex;

    private static Color hoveredColor = new Color(0.2f, 0.9f, 0.8f);
    private static Color baseColor = new Color(0.85f, 0.85f, 0.85f);

    void Start() {
        selectedIndex = -1;
        GameObject[] buttonObjects = new GameObject[3] { AlchemistButton, WitchcrafterButton, OccultistButton };
        buttons = new SpriteRenderer[3];
        clickAreas = new Rect[3];
        for(int i = 0; i < buttonObjects.Length; i++) {
            GameObject button = buttonObjects[i];
            Vector2 size = button.transform.localScale;
            clickAreas[i] = new Rect((Vector2)button.transform.position - size / 2f, size);
            buttons[i] = buttonObjects[i].GetComponent<SpriteRenderer>();
            buttons[i].color = baseColor;
        }
    }

    void Update() {
        int hoveredIndex = -1;
        Vector2 mousePos = InputManager.Instance.GetMousePosition();
        for(int i = 0; i < buttons.Length; i++) {
            if(clickAreas[i].Contains(mousePos)) {
                hoveredIndex = i;
                if(hoveredIndex != selectedIndex) {
                    buttons[i].color = hoveredColor;
                }
            } 
            else if(i != selectedIndex) {
                buttons[i].color = baseColor;
            }
        }

        if(hoveredIndex < 0 || hoveredIndex == selectedIndex) {
            return;
        }

        if(InputManager.Instance.SelectPressed()) {
            selectedIndex = hoveredIndex;
            TeamPreset selectedTeam = Team.Alchemists;
            if(selectedIndex == 1) {
                selectedTeam = Team.Witchcrafters;
            }
            else if(selectedIndex == 2) {
                selectedTeam = Team.Occultists;
            }

            for(int i = 0; i < buttons.Length; i++) {
                buttons[i].color = i == selectedIndex ? selectedTeam.teamColor : baseColor;
            }

            
            menu.SelectTeam(team, selectedTeam);
        }
    }
}

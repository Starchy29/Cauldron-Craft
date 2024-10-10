using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelectMenu : MonoBehaviour
{
    private const float EXTENT = 5f;

    [SerializeField] private TeamSelector team1;
    [SerializeField] private TeamSelector team2;
    [SerializeField] private GameObject buttonAnchor;
    [SerializeField] private AutoButton startButton;

    void Awake() {
        startButton.Disabled = true;
        switch(GameManager.Mode) {
            case GameMode.VSAI:
                team2.gameObject.SetActive(false);
                team1.transform.position = new Vector3(-EXTENT / 2f, 0, 0);
                buttonAnchor.transform.position = new Vector3(EXTENT / 2f, 0, 0);
                team1.SetTeam(null);
                break;
            case GameMode.PVP:
                team2.gameObject.SetActive(true);
                team1.transform.position = new Vector3(-EXTENT, 0, 0);
                team2.transform.position = new Vector3(EXTENT, 0, 0);
                buttonAnchor.transform.position = Vector3.zero;

                team1.SetTeam(null);
                team2.SetTeam(null);
                break;
        }
    }

    public void SelectTeam(TeamSelector team, TeamPreset choice) {
        team.SetTeam(choice);
        bool startAvailable = GameManager.Mode == GameMode.VSAI ?
            team1.Choice.HasValue :
            team1.Choice.HasValue && team2.Choice.HasValue && team1.Choice.Value != team2.Choice.Value;
        startButton.Disabled = !startAvailable;

        if(team == team1) {
            GameManager.team1Choice = choice;
        } else {
            GameManager.team2Choice = choice;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardScript : MonoBehaviour
{
    [SerializeField] private GameObject pointMarkerPrefab;
    private GameObject[,] pointMarkers; // row is team index
    private const float MARKER_GAP = 0.7f;

    public void Setup(int winAmount) {
        pointMarkers = new GameObject[2, winAmount];
        for(int i = 0; i < winAmount; i++) {
            pointMarkers[0, i] = Instantiate(pointMarkerPrefab, transform);
            pointMarkers[0, i].transform.localPosition = new Vector3(-(i+1) * MARKER_GAP, 0, 0);

            pointMarkers[1, i] = Instantiate(pointMarkerPrefab, transform);
            pointMarkers[1, i].transform.localPosition = new Vector3((i+1) * MARKER_GAP, 0, 0);
        }
    }

    public void UpdateDisplay(Dictionary<Team, int> pointsPerTeam) {
        Team[] bothTeams = GameManager.Instance.AllTeams;
        for(int i = 0; i < pointMarkers.GetLength(1); i++) {
            pointMarkers[0, i].GetComponent<SpriteRenderer>().color = pointsPerTeam[bothTeams[0]] > i ? bothTeams[0].TeamColor : Color.white;
            pointMarkers[1, i].GetComponent<SpriteRenderer>().color = pointsPerTeam[bothTeams[1]] > i ? bothTeams[1].TeamColor : Color.white;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverviewDisplayer : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshPro textbox;

    public static GameOverviewDisplayer Instance { get; private set; }

    private bool won;

    private void Awake() {
        Instance = this;
        foreach(Transform child in transform) {
            child.gameObject.SetActive(true);
        }
        gameObject.SetActive(false);
    }

    public void ShowTurnStart(int playerIndex) {
        if(won) {
            return;
        }
        
        gameObject.SetActive(true);
        textbox.text = "Player " + (playerIndex+1) + "'s Turn";
        textbox.color = GameManager.Instance.AllTeams[playerIndex].TeamColor;
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(1f));
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(gameObject, false));
    }

    public void ShowWinner(Team winner) {
        won = true;
        gameObject.SetActive(true);
        textbox.text = "Player " + (GameManager.Instance.AllTeams.IndexOf(winner) + 1) + " Wins";
        textbox.color = winner.TeamColor;
        StartCoroutine(ReturnToMenuSoon());
    }

    private IEnumerator ReturnToMenuSoon() {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(0);
    }
}

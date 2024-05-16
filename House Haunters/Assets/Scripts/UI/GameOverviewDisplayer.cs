using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverviewDisplayer : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshPro textbox;
    private IEnumerator coroutine;

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
        //AnimationsManager.Instance.QueueAnimation(new DisappearanceAnimator(gameObject, 1f));
        if(coroutine != null) {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        coroutine = CloseSoon();
        StartCoroutine(coroutine);
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

    private IEnumerator CloseSoon() {
        //StopCoroutine(CloseSoon);
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
}

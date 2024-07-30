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

    public void ShowTurnStart(Team turnStarter) {
        if(won) {
            return;
        }
        
        AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(() => {
            textbox.text =  (turnStarter == GameManager.Instance.Attacker ? "Attacker's" : "Defender's") + " Turn";
            textbox.color = turnStarter.TeamColor;
        }));
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(gameObject, true));
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(1f));
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(gameObject, false));
    }

    public void ShowWinner(Team winner) {
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(99999f));
        won = true;
        gameObject.SetActive(true);
        textbox.text = (winner == GameManager.Instance.Attacker ? "Attacker" : "Defender") + " Wins";
        textbox.color = winner.TeamColor;
        StartCoroutine(ReturnToMenuSoon());
    }

    private IEnumerator ReturnToMenuSoon() {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverviewDisplayer : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshPro textbox;

    public static GameOverviewDisplayer Instance { get; private set; }

    private void Awake() {
        Instance = this;
        foreach(Transform child in transform) {
            child.gameObject.SetActive(true);
        }
        gameObject.SetActive(false);
    }

    public void ShowTurnStart(Team turnStarter) {
        AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(() => {
            textbox.text = "The " + turnStarter.Name;
            textbox.color = turnStarter.TeamColor;
        }));
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(gameObject, true));
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(1f));
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(gameObject, false));
    }

    public void ShowWinner(Team winner) {
        AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(() => {
            gameObject.SetActive(true);
            textbox.text = "The " + winner.Name + " Win";
            textbox.color = winner.TeamColor;
        }));
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(5f));
        AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(() => { SceneManager.LoadScene(0); }));
    }

    public void ShowLoser(Team loser) {
        AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(() => {
            gameObject.SetActive(true);
            textbox.text = "The " + loser.Name + " Lose";
            textbox.color = GameManager.Instance.OpponentOf(loser).TeamColor;
        }));
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(5f));
        AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(() => { SceneManager.LoadScene(0); }));
    }
}

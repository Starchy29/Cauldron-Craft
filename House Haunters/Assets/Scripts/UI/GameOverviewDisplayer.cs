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

    public void ShowObjective() {
        gameObject.SetActive(true);
        textbox.text = "Craft every monster!";
        textbox.color = Color.white;
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(3f));
        AnimationsManager.Instance.QueueFunction(() => { gameObject.SetActive(false); });
    }

    public void ShowTurnStart(Team turnStarter) {
        AnimationsManager.Instance.QueueFunction(() => {
            textbox.text = "The " + turnStarter.Name;
            textbox.color = turnStarter.TeamColor;
        });
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(gameObject, true));
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(1f));
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(gameObject, false));
    }

    public void ShowWinner(Team winner) {
        AnimationsManager.Instance.QueueFunction(() => {
            gameObject.SetActive(true);
            textbox.text = "The " + winner.Name + " Win";
            textbox.color = winner.TeamColor;
            SoundManager.Instance.PlaySound(Sounds.Victory);
        });
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(7f));
        AnimationsManager.Instance.QueueFunction(() => {
            SoundManager.Instance.PlaySong(true);
            SceneManager.LoadScene(0); 
        });
    }
}

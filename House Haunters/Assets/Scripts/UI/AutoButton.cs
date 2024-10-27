using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoButton : MonoBehaviour
{
    public enum ClickFunction {
        None,
        EndTurn,
        StartGamePVP,
        StartGameVAI,
        GameplayBack,
        QuitMatch,
        CloseGame,
        StartGame
    }

    [SerializeField] private ClickFunction clickFunction;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] public GameObject tooltip;
    [SerializeField] private Sounds clickSound = Sounds.ButtonClick;
    private bool hovered;

    private bool disabled;
    public bool Disabled {
        get { return disabled; }
        set {
            disabled = value;
            sprite.color = disabled ? disabledColor : baseColor;
        }
    }

    public Trigger OnClick;
    public Trigger OnHover;
    public Trigger OnMouseLeave;

    protected Color baseColor = new Color(0.9f, 0.9f, 0.9f);
    protected Color disabledColor = new Color(0.3f, 0.3f, 0.3f);
    protected Color hoveredColor = new Color(0.2f, 0.9f, 0.8f);
    private float tooltipTimer;
    protected float tooltipWait = 0.6f;

    void Start() {
        switch(clickFunction) {
            case ClickFunction.EndTurn:
                OnClick = MenuManager.Instance.EndTurn;
                break;
            case ClickFunction.StartGamePVP:
                OnClick = MainMenuScript.StartPVP;
                break;
            case ClickFunction.StartGameVAI:
                OnClick = MainMenuScript.StartVAI;
                break;
            case ClickFunction.GameplayBack:
                OnClick = MenuManager.Instance.BackMenu;
                break;
            case ClickFunction.QuitMatch:
                OnClick = () => { 
                    SceneManager.LoadScene(0);
                    if(SceneManager.GetActiveScene().buildIndex > 1) {
                        SoundManager.Instance.StopSong(false, true);
                        SoundManager.Instance.PlaySong(true);
                    }
                };
                break;
            case ClickFunction.CloseGame:
                OnClick = () => { Application.Quit(); };
                break;
            case ClickFunction.StartGame:
                OnClick = () => {
                    SoundManager.Instance.StopSong(true);
                    if (GameManager.Mode == GameMode.VSAI) {
                        List<TeamPreset> aiOptions = new List<TeamPreset> { Team.Alchemists, Team.Witchcrafters, Team.Occultists };
                        aiOptions.Remove(GameManager.team1Choice);
                        GameManager.team2Choice = aiOptions[Random.value < 0.5f ? 0 : 1];
                    }
                    StartCoroutine(MainMenuScript.FadeToBlack());
                };
                break;
        }
    }

    void Update() {
        Vector2 mousePos = InputManager.Instance.GetMousePosition();
        bool nowHovered = Global.GetObjectArea(gameObject).Contains(mousePos);
        if(!hovered && nowHovered) {
            if(!disabled) {
                SoundManager.Instance.PlaySound(Sounds.ButtonHover);
            }
            if(OnHover != null) {
                OnHover();
            }
        } 
        else if(hovered && !nowHovered) {
            if(OnMouseLeave != null) {
                OnMouseLeave();
            }
            if(tooltip != null) {
                tooltip.SetActive(false);
                tooltipTimer = 0;
            }
        }

        hovered = nowHovered;

        if(tooltip != null && hovered && tooltipTimer <= tooltipWait) {
            tooltipTimer += Time.deltaTime;
            if(tooltipTimer >= tooltipWait) {
                tooltip.SetActive(true);
            }
        }

        if(!disabled && hovered && InputManager.Instance.SelectPressed()) {
            SoundManager.Instance.PlaySound(clickSound);
            OnClick();
        }

        if(disabled) {
            sprite.color = disabledColor;
        } else {
            sprite.color = hovered ? hoveredColor : baseColor;
        }
    }

    //public void SetBackColor(Color color) {
    //    baseColor = color;
    //    disabledColor = Global.ChangeSaturation(Global.ChangeValue(color, -0.3f), -0.1f);
    //    hoveredColor = Global.ChangeSaturation(Global.ChangeValue(color, +0.1f), +0.3f);
    //}

    private void OnDisable() {
        if(tooltip != null) {
            tooltip.SetActive(false);
            tooltipTimer = 0;
        }
    }
}

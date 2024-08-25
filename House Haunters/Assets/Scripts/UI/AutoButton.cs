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
        QuitGame
    }

    [SerializeField] private ClickFunction clickFunction;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] public GameObject tooltip;
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

    protected Color baseColor = new Color(0.85f, 0.85f, 0.85f);
    private Color disabledColor = new Color(0.3f, 0.3f, 0.3f);
    private Color hoveredColor = new Color(0.2f, 0.9f, 0.8f);
    private float tooltipTimer;
    private const float TOOL_TIP_WAIT = 0.6f;

    void Start() {
        //SetBackColor(baseColor);
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
            case ClickFunction.QuitGame:
                OnClick = () => { SceneManager.LoadScene(0); };
                break;
        }
    }

    void Update() {
        Vector2 mousePos = InputManager.Instance.GetMousePosition();
        bool nowHovered = Global.GetObjectArea(gameObject).Contains(mousePos);
        if(!hovered && nowHovered && OnHover != null) {
            OnHover();
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

        if(tooltip != null && hovered && tooltipTimer < TOOL_TIP_WAIT) {
            tooltipTimer += Time.deltaTime;
            if(tooltipTimer >= TOOL_TIP_WAIT) {
                tooltip.SetActive(true);
            }
        }

        if(!disabled && hovered && InputManager.Instance.SelectPressed()) {
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

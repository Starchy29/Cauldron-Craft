using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class AutoButton : MonoBehaviour
{
    public enum ClickFunction {
        None,
        EndTurn,
        StartGamePVP,
        StartGameVAI,
        BackTargetSelect
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

    protected Color baseColor = new Color(0.7f, 0.7f, 0.7f);
    private Color disabledColor;
    private Color hoveredColor;
    private float tooltipTimer;

    void Start() {
        SetBackColor(baseColor);
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
            case ClickFunction.BackTargetSelect:
                OnClick = MenuManager.Instance.BackMenu;
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

        if(tooltip != null && hovered && tooltipTimer < 1f) {
            tooltipTimer += Time.deltaTime;
            if(tooltipTimer >= 1f) {
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

    public void SetBackColor(Color color) {
        baseColor = color;
        disabledColor = Global.ChangeSaturation(Global.ChangeValue(color, -0.3f), -0.1f);
        hoveredColor = Global.ChangeSaturation(Global.ChangeValue(color, +0.1f), +0.3f);
    }

    private void OnDisable() {
        if(tooltip != null) {
            tooltip.SetActive(false);
            tooltipTimer = 0;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

// manages player input in regards to menus during gameplay
public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject TileSelector;
    [SerializeField] private MoveMenu moveMenu;
    [SerializeField] private AutoButton endTurnButton;
    [SerializeField] private BuyMenu buyMenu;

    [SerializeField] private TMPro.TextMeshPro decayQuantity;
    [SerializeField] private TMPro.TextMeshPro plantQuantity;

    public bool UseKBMouse { get; set; }

    private enum SelectionTarget { Animations, Monster, Move, Targets, CraftChoice }
    private SelectionTarget state;
    
    private LevelGrid level;
    private GameManager gameManager;
    private Team controller;

    // target selection data
    private List<List<Vector2Int>> tileGroups;
    private Vector2[] tileGroupCenters;
    private int selectedMoveSlot;
    private Monster selected;

    public static MenuManager Instance {  get; private set; }

    void Awake() {
        Instance = this;   
    }

    void Start() {
        state = SelectionTarget.Monster;
        UseKBMouse = true;
        level = LevelGrid.Instance;
        gameManager = GameManager.Instance;
        controller = gameManager.PlayerTeam;
        controller.OnTurnStart += () => { SetState(SelectionTarget.Monster); };
        AnimationsManager.Instance.OnAnimationsEnd += () => { if(gameManager.CurrentTurn == controller) SetState(SelectionTarget.Monster); };
        UpdateResources();
        SetState(SelectionTarget.Monster);
    }

    void Update() {
        Debug.Log(state);
        // this object is active only when the player can select a monster or target
        InputManager input = InputManager.Instance;
        Vector2 mousePos = InputManager.Instance.GetMousePosition();

        if(input.BackPressed()) {
            BackMenu();
            if(!isActiveAndEnabled) {
                return;
            }
        }

        if(state == SelectionTarget.Targets) {
            // find the target group that the mouse is closest to
            Vector2 closestMidpoint = tileGroupCenters.Min((Vector2 spot) => { return Vector2.Distance(mousePos, spot); });
            int hoveredTargetIndex = tileGroupCenters.IndexOf(closestMidpoint).Value;
            level.ColorTiles(tileGroups[hoveredTargetIndex], TileHighlighter.State.Hovered);

            if(input.SelectPressed()) {
                // use the move on the hovered target
                selected.UseMove(selectedMoveSlot, tileGroups[hoveredTargetIndex]);
                SetState(SelectionTarget.Animations);
            }
            return;
        }

        TileSelector.SetActive(false);

        // check if the mouse is over an open menu or button
        if(moveMenu.isActiveAndEnabled && Global.GetObjectArea(moveMenu.Background).Contains(mousePos) ||
            buyMenu.isActiveAndEnabled && Global.GetObjectArea(buyMenu.Background).Contains(mousePos) ||
            endTurnButton.isActiveAndEnabled && Global.GetObjectArea(endTurnButton.gameObject).Contains(mousePos)
        ) {
            return;
        }

        Vector2Int tile = (Vector2Int)level.Tiles.WorldToCell(mousePos);

        if(!level.IsInGrid(tile)) {
            if(input.SelectPressed()) {
                BackMenu();
            }
            return;
        }

        TileSelector.SetActive(true);
        TileSelector.transform.position = level.Tiles.GetCellCenterWorld((Vector3Int)tile);

        GridEntity hoveredEntity = level.GetEntity(tile);

        if(input.SelectPressed()) {
            if(hoveredEntity == controller.Spawnpoint && !controller.Spawnpoint.Cooking) {
                SetState(SelectionTarget.CraftChoice);
            }
            else if(hoveredEntity is Monster) {
                moveMenu.GetComponent<MoveMenu>().Open((Monster)hoveredEntity, controller);
                selected = (Monster)hoveredEntity;
                SetState(SelectionTarget.Move);
            }
            else {
                // close menu when clicking off of it
                BackMenu();
            }
        }
    }

    private void SetState(SelectionTarget state) {
        this.state = state;
        gameObject.SetActive(true);
        TileSelector.SetActive(false);
        moveMenu.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        buyMenu.gameObject.SetActive(false);

        level.ColorTiles(null, TileHighlighter.State.Hovered);
        level.ColorTiles(null, TileHighlighter.State.Selectable);

        foreach(Monster teammate in controller.Teammates) {
            teammate.MoveCounter.Close();
        }

        switch(state) {
            case SelectionTarget.Monster:
                endTurnButton.gameObject.SetActive(true);
                foreach(Monster teammate in controller.Teammates) {
                    teammate.MoveCounter.Open();
                }
                break;
            case SelectionTarget.Move:
                moveMenu.Open(selected, controller);
                break;
            case SelectionTarget.Targets:
                break;
            case SelectionTarget.CraftChoice:
                buyMenu.Open(controller);
                break;
            case SelectionTarget.Animations:
                gameObject.SetActive(false);
                break;
        }
    }

    private void BackMenu() {
        switch(state) {
            case SelectionTarget.Move:
            case SelectionTarget.CraftChoice:
                SetState(SelectionTarget.Monster);
                break;
            case SelectionTarget.Targets:
                SetState(SelectionTarget.Move);
                break;
        }
    }

    // function of the end turn button
    public void EndTurn() {
        controller.EndTurn();
        SetState(SelectionTarget.Animations);
    }

    public void SelectMove(int moveSlot) {
        selectedMoveSlot = moveSlot;
        Move move = selected.Stats.Moves[selectedMoveSlot];
        bool filtered = move.TargetType == Move.Targets.UnaffectedFloor || move.TargetType == Move.Targets.Traversable || move.TargetType == Move.Targets.StandableSpot;
        tileGroups = selected.GetMoveOptions(selectedMoveSlot, filtered);
        tileGroupCenters = DetermineCenters(tileGroups);
        SetState(SelectionTarget.Targets);

        List<Vector2Int> allTiles = new List<Vector2Int>();
        foreach (List<Vector2Int> group in tileGroups)
        {
            allTiles.AddRange(group);
        }
        level.ColorTiles(allTiles, TileHighlighter.State.Selectable);
        level.ColorTiles(null, TileHighlighter.State.Highlighted);
    }

    public void BuyMonster(MonsterName type) {
        controller.BuyMonster(type);
        UpdateResources();
        SetState(SelectionTarget.Monster);
    }

    public void UpdateResources() {
        decayQuantity.text = "" + controller.Resources[Ingredient.Decay];
        plantQuantity.text = "" + controller.Resources[Ingredient.Flora];
    }

    private Vector2[] DetermineCenters(List<List<Vector2Int>> tileGroups) {
        Vector2[] centers = new Vector2[tileGroups.Count];
        for(int i = 0; i < tileGroups.Count; i++) {
            Vector3 center = new Vector2();
            foreach(Vector2Int tile in tileGroups[i]) {
                center += level.Tiles.GetCellCenterWorld((Vector3Int)tile);
            }
            centers[i] = center / tileGroups[i].Count;
        }
        return centers;
    }
}

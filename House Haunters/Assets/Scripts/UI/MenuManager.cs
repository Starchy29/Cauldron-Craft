using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// manages player input in regards to menus during gameplay
public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject TileSelector;
    [SerializeField] private MoveMenu moveMenu;
    [SerializeField] private AutoButton endTurnButton;
    [SerializeField] private BuyMenu buyMenu;

    private HealthBarScript hoveredHealthbar;
    private List<HealthBarScript> targetedHealthBars;

    public bool UseKBMouse { get; set; }

    private enum SelectionTarget { Animations, Monster, Move, Targets, CraftChoice }
    private SelectionTarget state;
    
    private LevelGrid level;
    private GameManager gameManager;
    private Team controller;

    // target selection data
    private List<List<Vector2Int>> tileGroups;
    private List<Vector2> tileGroupCenters;
    private int selectedMoveSlot;
    private Monster selected;

    public static MenuManager Instance {  get; private set; }

    void Awake() {
        Instance = this;   
    }

    void Start() {
        UseKBMouse = true;
        level = LevelGrid.Instance;
        gameManager = GameManager.Instance;
        AnimationsManager.Instance.OnAnimationsEnd += (Team currentTurn) => { if(currentTurn == controller) SetState(SelectionTarget.Monster); };
    }

    void Update() {
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
            int hoveredTargetIndex = tileGroupCenters.IndexOf(closestMidpoint);
            level.ColorTiles(tileGroups[hoveredTargetIndex], TileHighlighter.State.Hovered);

            if(input.SelectPressed()) {
                // use the move on the hovered target
                SetState(SelectionTarget.Animations);
                selected.UseMove(selectedMoveSlot, tileGroups[hoveredTargetIndex]);   
            }
            return;
        }

        TileSelector.SetActive(false);
        if(hoveredHealthbar != null) {
            hoveredHealthbar.gameObject.SetActive(false);
            hoveredHealthbar = null;
        }

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

        if(hoveredEntity is Monster) {
            hoveredHealthbar = ((Monster)hoveredEntity).healthBar;
            hoveredHealthbar.gameObject.SetActive(true);
        }

        if(input.SelectPressed()) {
            if(hoveredEntity == controller.Spawnpoint && controller.Spawnpoint.CookState == Cauldron.State.Ready) {
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

    public void StartPlayerTurn(Team player) {
        controller = player;
    }

    private void SetState(SelectionTarget state) {
        LevelGrid level = LevelGrid.Instance; // this function runs in Start()
        this.state = state;
        gameObject.SetActive(true);
        TileSelector.SetActive(false);
        moveMenu.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        buyMenu.gameObject.SetActive(false);

        if(hoveredHealthbar != null) {
            hoveredHealthbar.gameObject.SetActive(false);
            hoveredHealthbar = null;
        }

        if(targetedHealthBars != null) {
            foreach(HealthBarScript healthBar in targetedHealthBars) {
                healthBar.gameObject.SetActive(false);
            }
            targetedHealthBars = null;
        }

        level.ColorTiles(null, TileHighlighter.State.Highlighted);
        level.ColorTiles(null, TileHighlighter.State.Hovered);
        level.ColorTiles(null, TileHighlighter.State.Selectable);
        level.ColorTiles(null, TileHighlighter.State.Selected);

        if(controller != null) {
            foreach(Monster teammate in controller.Teammates) {
                teammate.MoveCounter.Close();
            }
        }

        switch(state) {
            case SelectionTarget.Monster:
                endTurnButton.gameObject.SetActive(true);
                if(controller != null) {
                    foreach(Monster teammate in controller.Teammates) {
                        teammate.MoveCounter.Open();
                    }
                }
                break;
            case SelectionTarget.Move:
                endTurnButton.gameObject.SetActive(true);
                if(controller != null) {
                    foreach(Monster teammate in controller.Teammates) {
                        teammate.MoveCounter.Open();
                    }
                }
                moveMenu.Open(selected, controller);
                level.ColorTiles(new List<Vector2Int>() { selected.Tile }, TileHighlighter.State.Selected);
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
        SetState(SelectionTarget.Animations);
        controller.EndTurn();
    }

    public void SelectMove(int moveSlot) {
        selectedMoveSlot = moveSlot;
        Move move = selected.Stats.Moves[selectedMoveSlot];
        bool filtered = move.TargetType == Move.Targets.ZonePlaceable || move.TargetType == Move.Targets.Traversable || move.TargetType == Move.Targets.StandableSpot;
        tileGroups = selected.GetMoveOptions(selectedMoveSlot, filtered);
        tileGroupCenters = tileGroups.Map((List<Vector2Int> tileGroup) => { return Global.DetermineCenter(tileGroup); });
        SetState(SelectionTarget.Targets);

        List<Vector2Int> allTiles = new List<Vector2Int>();
        foreach(List<Vector2Int> group in tileGroups) {
            allTiles.AddRange(group); // will add duplicates
        }

        // show health bars on possible targets
        if(move.Type == MoveType.RangedAttack || move.Type == MoveType.MeleeAttack || move.Type == MoveType.Heal || move.Type == MoveType.Poison) {
            bool checkAllies = move.Type == MoveType.Heal;
            targetedHealthBars = new List<HealthBarScript>();
            List<Vector2Int> tilesWithMonsters = allTiles.Filter((Vector2Int tile) => { return level.GetMonster(tile) != null; });
            foreach(Vector2Int tile in tilesWithMonsters) {
                Monster healthBarHaver = level.GetMonster(tile);
                if((healthBarHaver.Controller == controller) == checkAllies) {
                    healthBarHaver.healthBar.gameObject.SetActive(true);
                    targetedHealthBars.Add(healthBarHaver.healthBar);
                }
            }
        }

        level.ColorTiles(allTiles, TileHighlighter.State.Selectable);
        level.ColorTiles(null, TileHighlighter.State.Highlighted);
    }

    public void BuyMonster(MonsterName type) {
        controller.BuyMonster(type);
        SetState(SelectionTarget.Animations);
    }
}

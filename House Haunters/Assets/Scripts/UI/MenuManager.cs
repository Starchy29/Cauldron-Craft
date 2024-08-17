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
    [SerializeField] private AutoButton backButton;
    [SerializeField] private BuyMenu buyMenu;
    [SerializeField] private TerrainDisplay terrainInfo;
    [SerializeField] private GameObject pauseMenu;

    private HealthBarScript hoveredHealthbar;
    private List<HealthBarScript> targetedHealthBars;

    public bool UseKBMouse { get; set; }

    private enum SelectionTarget { None, Monster, Move, Targets, CraftChoice, Paused }
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
    public bool Paused { get { return state == SelectionTarget.Paused; } }

    void Awake() {
        Instance = this;   
    }

    void Start() {
        UseKBMouse = true;
        level = LevelGrid.Instance;
        gameManager = GameManager.Instance;
        SetState(SelectionTarget.None);
        AnimationsManager.Instance.OnAnimationsEnd += (Team currentTurn) => { if(currentTurn == controller) SetState(SelectionTarget.Monster); };
    }

    void Update() {
        InputManager input = InputManager.Instance;
        Vector2 mousePos = InputManager.Instance.GetMousePosition();

        if(input.PausePressed()) {
            if(state == SelectionTarget.Paused) {
                BackMenu();
            } else {
                SetState(SelectionTarget.Paused);
            }
            return;
        }

        if(input.BackPressed()) {
            BackMenu();
            return;
        }

        if(state == SelectionTarget.None || state == SelectionTarget.Paused) {
            // only update when the player can select a monster or target
            return;
        }

        if(state == SelectionTarget.Targets) {
            if(backButton.isActiveAndEnabled && Global.GetObjectArea(backButton.gameObject).Contains(mousePos)) {
                level.ColorTiles(null, TileHighlighter.State.Hovered);
                return;
            }

            // find the target group that the mouse is closest to
            Vector2 closestMidpoint = tileGroupCenters.Min((Vector2 spot) => { return Vector2.Distance(mousePos, spot); });
            int hoveredTargetIndex = tileGroupCenters.IndexOf(closestMidpoint);
            level.ColorTiles(tileGroups[hoveredTargetIndex], TileHighlighter.State.Hovered);

            // if moving into a capture point, highlight the capture point
            if(selected.Stats.Moves[selectedMoveSlot] is MovementAbility) {
                bool highlighted = false;
                foreach(ResourcePile resource in gameManager.AllResources) {
                    // make sure not already on the capture point and not already owned
                    if(resource.IsInCaptureRange(selected.Tile) || resource.Controller == controller) {
                        continue;
                    }

                    Vector2Int hoveredTile = tileGroups[hoveredTargetIndex][0];
                    if(resource.IsInCaptureRange(hoveredTile)) {
                        level.ColorTiles(level.GetTilesInRange(resource.Tile, 1, true), TileHighlighter.State.Highlighted);
                        highlighted = true;
                    }
                }

                if(!highlighted) {
                    level.ColorTiles(null, TileHighlighter.State.Highlighted);
                }
            }

            // select the target group
            if(input.SelectPressed()) {
                // use the move on the hovered target
                SetState(SelectionTarget.None);
                selected.UseMove(selectedMoveSlot, tileGroups[hoveredTargetIndex]);   
            }
            return;
        }

        TileSelector.SetActive(false);
        terrainInfo.gameObject.SetActive(false);
        level.ColorTiles(null, TileHighlighter.State.Hovered);
        if(hoveredHealthbar != null) {
            hoveredHealthbar.gameObject.SetActive(false);
            hoveredHealthbar = null;
        }

        // check if the mouse is over an open menu or button
        if(moveMenu.isActiveAndEnabled && Global.GetObjectArea(moveMenu.Background).Contains(mousePos) ||
            buyMenu.isActiveAndEnabled && Global.GetObjectArea(buyMenu.Background).Contains(mousePos) ||
            endTurnButton.isActiveAndEnabled && Global.GetObjectArea(endTurnButton.gameObject).Contains(mousePos) ||
            backButton.isActiveAndEnabled && Global.GetObjectArea(backButton.gameObject).Contains(mousePos)
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

        // check hovered terrain
        WorldTile terrain = level.GetTile(tile);
        if(terrain.CurrentEffect != null) {
            terrainInfo.gameObject.SetActive(true);
            terrainInfo.gameObject.transform.position = level.Tiles.GetCellCenterWorld((Vector3Int)tile) + new Vector3(0, 1.5f, 0);
            terrainInfo.ColorBack.color = terrain.CurrentEffect.Controller.TeamColor;
            terrainInfo.DurationLabel.text = "" + terrain.CurrentEffect.Duration;
        }

        // check hovered entity
        GridEntity hoveredEntity = level.GetEntity(tile);

        if(hoveredEntity is Monster) {
            hoveredHealthbar = ((Monster)hoveredEntity).healthBar;
            hoveredHealthbar.gameObject.SetActive(true);
            level.ColorTiles(hoveredEntity.Tile, TileHighlighter.State.Hovered);
        }
        else if(hoveredEntity is Cauldron) {
            level.ColorTiles(hoveredEntity.Tile, TileHighlighter.State.Hovered);
        }

        if(input.SelectPressed()) {
            if(hoveredEntity is Cauldron/*hoveredEntity == controller.Spawnpoint && controller.Spawnpoint.CookState == Cauldron.State.Ready*/) {
                SetState(SelectionTarget.CraftChoice);
                buyMenu.Open(hoveredEntity.Controller);
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

        // move the menu to the side of the current player. Assumes these are children of the camera
        Vector3 endPos = endTurnButton.transform.localPosition;
        endPos.x = (player.OnLeft ? -1 : 1) * Mathf.Abs(endPos.x);
        endTurnButton.transform.localPosition = endPos;
        backButton.transform.localPosition = endPos;
    }

    private void SetState(SelectionTarget state) {
        LevelGrid level = LevelGrid.Instance; // this function runs in Start()
        this.state = state;
        TileSelector.SetActive(false);
        moveMenu.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        buyMenu.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        pauseMenu.SetActive(false);

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

        level.ColorTiles(null, TileHighlighter.State.WeakHighlight);
        level.ColorTiles(null, TileHighlighter.State.Highlighted);
        level.ColorTiles(null, TileHighlighter.State.Hovered);
        level.ColorTiles(null, TileHighlighter.State.Selectable);
        level.ColorTiles(null, TileHighlighter.State.Selected);

        switch(state) {
            case SelectionTarget.Monster:
                endTurnButton.gameObject.SetActive(true);
                if(controller != null) {
                    List<Vector2Int> selectableTiles = new List<Vector2Int>();
                    foreach(Monster teammate in controller.Teammates) {
                        if(HasUsableMove(teammate)) {
                            selectableTiles.Add(teammate.Tile);
                        }
                    }

                    if(controller.CanCraft()) {
                        selectableTiles.Add(controller.Spawnpoint.Tile);
                    }

                    level.ColorTiles(selectableTiles, TileHighlighter.State.Selectable);
                }
                break;
            case SelectionTarget.Move:
                moveMenu.Open(selected, controller);
                level.ColorTiles(new List<Vector2Int>() { selected.Tile }, TileHighlighter.State.Selected);
                break;
            case SelectionTarget.Targets:
                backButton.gameObject.SetActive(true);
                break;
            case SelectionTarget.Paused:
                pauseMenu.SetActive(true);
                break;
            case SelectionTarget.CraftChoice:
            case SelectionTarget.None:
                break;
        }
    }

    public void BackMenu() {
        switch(state) {
            case SelectionTarget.Move:
            case SelectionTarget.CraftChoice:
                SetState(SelectionTarget.Monster);
                break;
            case SelectionTarget.Targets:
                SetState(SelectionTarget.Move);
                break;
            case SelectionTarget.Paused:
                SetState(gameManager.CurrentTurn == controller ? SelectionTarget.Monster : SelectionTarget.None);
                break;
        }
    }

    // function of the end turn button
    public void EndTurn() {
        SetState(SelectionTarget.None);
        controller.EndTurn();
    }

    public void SelectMove(int moveSlot) {
        selectedMoveSlot = moveSlot;
        Move move = selected.Stats.Moves[selectedMoveSlot];
        bool filtered = move.TargetType == Move.Targets.ZonePlaceable || move.TargetType == Move.Targets.StandableSpot;
        tileGroups = selected.GetMoveOptions(selectedMoveSlot, filtered);
        tileGroupCenters = tileGroups.Map((List<Vector2Int> tileGroup) => { return Global.DetermineCenter(tileGroup); });
        SetState(SelectionTarget.Targets);

        List<Vector2Int> allTiles = new List<Vector2Int>();
        foreach(List<Vector2Int> group in tileGroups) {
            allTiles.AddRange(group); // will add duplicates
        }

        // show health bars on possible targets
        if(move.Type == MoveType.RangedAttack || move.Type == MoveType.MeleeAttack || move.Type == MoveType.Heal || move.Type == MoveType.Decay) {
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
        SetState(SelectionTarget.None);
    }

    private bool HasUsableMove(Monster monster) {
        for(int i = 0; i < monster.Stats.Moves.Length; i++) {
            if(monster.CanUse(i)) {
                return true;
            }
        }
        return false;
    }
}

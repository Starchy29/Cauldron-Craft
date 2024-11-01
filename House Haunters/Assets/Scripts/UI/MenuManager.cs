using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

// manages player input in regards to menus during gameplay
public class MenuManager : MonoBehaviour
{
    [SerializeField] private MoveMenu moveMenu;
    [SerializeField] private AutoButton endTurnButton;
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

    private float terrainTimer;

    // target selection data
    private List<Selection> targetOptions;
    private List<Vector2> targetCenters;
    private bool filterTargets;
    private int selectedMoveSlot;
    private Monster selected;
    private GridEntity lastHoveredEntity;
    private int lastHoveredIndex;

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
    }

    void Update() {
        InputManager input = InputManager.Instance;
        Vector2 mousePos = InputManager.Instance.GetMousePosition();

        if(gameManager.CurrentTurn == controller && state == SelectionTarget.None && !AnimationsManager.Instance.Animating) {
            SetState(SelectionTarget.Monster);
            return;
        }

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
            // find the target group that the mouse is closest to
            LevelHighlighter.Instance.HoveredResource = null;
            Vector2 closestMidpoint = targetCenters.Min((Vector2 spot) => { return Vector2.Distance(mousePos, spot); });
            int hoveredTargetIndex = targetCenters.IndexOf(closestMidpoint);

            if(!Global.GetWorldBoundingBox(filterTargets ? targetOptions[hoveredTargetIndex].Filtered : targetOptions[hoveredTargetIndex].Unfiltered).Contains(mousePos)) {
                LevelHighlighter.Instance.ColorTiles(null, HighlightType.Hovered);
                if(input.SelectPressed()) {
                    BackMenu();
                }
                return;
            }

            if(hoveredTargetIndex >= 0 && hoveredTargetIndex != lastHoveredIndex) {
                SoundManager.Instance.PlaySound(Sounds.TileHover);
            }
            lastHoveredIndex = hoveredTargetIndex;

            LevelHighlighter.Instance.ColorTiles(filterTargets ? targetOptions[hoveredTargetIndex].Filtered : targetOptions[hoveredTargetIndex].Unfiltered, HighlightType.Hovered);

            // if moving into a capture point, highlight the capture point
            if(selected.Stats.Moves[selectedMoveSlot] is MovementAbility) {
                foreach(ResourcePile resource in gameManager.AllResources) {
                    // make sure not already on the capture point and not already owned
                    if(resource.IsInCaptureRange(selected.Tile) || resource.Controller == controller || resource.Contested) {
                        continue;
                    }

                    Vector2Int hoveredTile = targetOptions[hoveredTargetIndex].Filtered[0];
                    if(resource.IsInCaptureRange(hoveredTile)) {
                        LevelHighlighter.Instance.HoveredResource = resource;
                        break;
                    }
                }
            }

            // select the target group
            if(input.SelectPressed()) {
                // use the move on the hovered target
                SetState(SelectionTarget.None);
                if(selectedMoveSlot == MonsterType.WALK_INDEX) {
                    SoundManager.Instance.PlaySound(Sounds.ButtonClick);
                }
                selected.UseMove(selectedMoveSlot, targetOptions[hoveredTargetIndex]);   
            }
            return;
        }

        LevelHighlighter.Instance.CursorTile = null;
        terrainInfo.gameObject.SetActive(false);
        LevelHighlighter.Instance.ColorTiles(null, HighlightType.Hovered);
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

        LevelHighlighter.Instance.CursorTile = tile;

        // check hovered terrain
        WorldTile terrain = level.GetTile(tile);
        if(terrain.CurrentEffect != null) {
            terrainTimer += Time.deltaTime;
            if(terrainTimer > 0.5f) {
                terrainInfo.gameObject.SetActive(true);
                terrainInfo.transform.parent.position = level.Tiles.GetCellCenterWorld((Vector3Int)tile) + new Vector3(0, 1.5f, 0);
                terrainInfo.ColorBack.color = terrain.CurrentEffect.Controller.TeamColor;
                terrainInfo.DurationLabel.text = "" + terrain.CurrentEffect.Duration;
            }
        } else {
            terrainTimer = 0f;
        }

        // check hovered entity
        GridEntity hoveredEntity = level.GetEntity(tile);
        if(hoveredEntity != null && hoveredEntity != lastHoveredEntity && !(hoveredEntity is ResourcePile)) {
            SoundManager.Instance.PlaySound(Sounds.TileHover);
        }
        lastHoveredEntity = hoveredEntity;

        if(hoveredEntity is Monster) {
            hoveredHealthbar = ((Monster)hoveredEntity).healthBar;
            hoveredHealthbar.gameObject.SetActive(true);
            LevelHighlighter.Instance.ColorTile(hoveredEntity.Tile, HighlightType.Hovered);
        }
        else if(hoveredEntity is Cauldron) {
            LevelHighlighter.Instance.ColorTile(hoveredEntity.Tile, HighlightType.Hovered);
        }

        if(input.SelectPressed()) {
            if(hoveredEntity is Cauldron) {
                SoundManager.Instance.PlaySound(Sounds.ButtonClick);
                SetState(SelectionTarget.CraftChoice);
                buyMenu.Open(hoveredEntity.Controller);
            }
            else if(hoveredEntity is Monster) {
                SoundManager.Instance.PlaySound(Sounds.ButtonClick);
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
    }

    private void SetState(SelectionTarget state) {
        LevelGrid level = LevelGrid.Instance; // this function runs in Start()
        this.state = state;
        LevelHighlighter.Instance.CursorTile = null;
        LevelHighlighter.Instance.HoveredResource = null;
        moveMenu.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        buyMenu.gameObject.SetActive(false);
        pauseMenu.SetActive(false);
        lastHoveredEntity = null;
        lastHoveredIndex = -1;

        if(controller != null) {
            foreach(Monster teammate in controller.Teammates) {
                teammate.moveIndicator.gameObject.SetActive(false);
            }
        }

        if(hoveredHealthbar != null) {
            hoveredHealthbar.gameObject.SetActive(false);
            hoveredHealthbar = null;
        }

        if(targetedHealthBars != null) {
            foreach(HealthBarScript healthBar in targetedHealthBars) {
                healthBar.gameObject.SetActive(false);
                healthBar.HidePredict();
            }
            targetedHealthBars = null;
        }

        LevelHighlighter highlighter = LevelHighlighter.Instance;
        foreach(HighlightType highlight in Enum.GetValues(typeof(HighlightType))) {
            highlighter.ColorTiles(null, highlight);
        }

        switch(state) {
            case SelectionTarget.Monster:
                endTurnButton.gameObject.SetActive(true);
                if(controller != null) {
                    List<Vector2Int> selectable = new List<Vector2Int>();
                    List<Vector2Int> walkTiles = new List<Vector2Int>();
                    foreach(Monster teammate in controller.Teammates) {
                        bool usableMove = false;
                        for(int i = 0; i < 3; i++) {
                            if(teammate.CanUse(i)) {
                                selectable.Add(teammate.Tile);
                                usableMove = true;
                                break;
                            }
                        }

                        if(teammate.WalkAvailable) {
                            teammate.moveIndicator.gameObject.SetActive(true);
                            teammate.moveIndicator.sprite = PrefabContainer.Instance.walkAvailable;
                        }
                        else if(!usableMove && teammate.AbilityAvailable) {
                            teammate.moveIndicator.gameObject.SetActive(true);
                            teammate.moveIndicator.sprite = PrefabContainer.Instance.abilityAvailable;
                        }
                    }

                    if(controller.CanCraft()) {
                        selectable.Add(controller.Spawnpoint.Tile);
                    }

                    highlighter.ColorTiles(selectable, HighlightType.Option);
                }
                break;
            case SelectionTarget.Move:
                moveMenu.Open(selected, controller);
                highlighter.ColorTiles(new List<Vector2Int>() { selected.Tile }, HighlightType.Selected);
                break;
            case SelectionTarget.Paused:
                pauseMenu.SetActive(true);
                SoundManager.Instance.PlaySound(Sounds.Pause);
                break;
            case SelectionTarget.Targets:
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
                SoundManager.Instance.PlaySound(Sounds.BackMenu);
                break;
            case SelectionTarget.Targets:
                SetState(SelectionTarget.Move);
                SoundManager.Instance.PlaySound(Sounds.BackMenu);
                break;
            case SelectionTarget.Paused:
                SetState(AnimationsManager.Instance.Animating ? SelectionTarget.None : SelectionTarget.Monster);
                SoundManager.Instance.PlaySound(Sounds.BackMenu);
                break;
        }
    }

    // function of the end turn button
    public void EndTurn() {
        SetState(SelectionTarget.None);
        controller.EndTurn();
    }

    public void SelectMove(int moveSlot, List<Selection> options) {
        selectedMoveSlot = moveSlot;
        Move move = selected.Stats.Moves[selectedMoveSlot];
        filterTargets = move.TargetType == Move.Targets.ZonePlaceable || move.TargetType == Move.Targets.StandableSpot;
        targetOptions = options;
        targetCenters = targetOptions.ConvertAll((Selection tileGroup) => { return Global.DetermineCenter(filterTargets ? tileGroup.Filtered : tileGroup.Unfiltered); });
        SetState(SelectionTarget.Targets);

        List<Vector2Int> targetableTiles = new List<Vector2Int>();
        foreach(Selection option in targetOptions) {
            targetableTiles.AddRange(filterTargets ? option.Filtered : option.Unfiltered); // will add duplicates
        }

        // show health bars on possible targets
        if(move.Type == MoveType.Attack || move.Type == MoveType.Attack || move.Type == MoveType.Heal || move.Type == MoveType.Decay) {
            bool checkAllies = move.Type == MoveType.Heal;
            targetedHealthBars = new List<HealthBarScript>();
            List<Vector2Int> tilesWithMonsters = targetableTiles.FindAll((Vector2Int tile) => { return level.GetMonster(tile) != null; });
            foreach(Vector2Int tile in tilesWithMonsters) {
                Monster healthBarHaver = level.GetMonster(tile);
                if((healthBarHaver.Controller == controller) == checkAllies) {
                    healthBarHaver.healthBar.gameObject.SetActive(true);
                    targetedHealthBars.Add(healthBarHaver.healthBar);
                    if(move.Type == MoveType.Attack) {
                        healthBarHaver.healthBar.PredictDamage(selected, healthBarHaver, ((Attack)move).Damage);
                    }
                }
            }
        }

        LevelHighlighter.Instance.ColorTiles(targetableTiles, HighlightType.Option);
        LevelHighlighter.Instance.ColorTiles(null, HighlightType.Highlight);
    }

    public void BuyMonster(MonsterName type) {
        controller.BuyMonster(type);
        SetState(SelectionTarget.None);
    }
}

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
    [SerializeField] private AutoButton craftButton;
    [SerializeField] private BuyMenu buyMenu;

    public bool UseKBMouse { get; set; }

    private enum SelectionTarget { Monster, Move, Targets, CraftChoice }
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
    }

    void Update() {
        InputManager input = InputManager.Instance;
        Vector2 mousePos = InputManager.Instance.GetMousePosition();

        endTurnButton.disabled = GameManager.Instance.CurrentTurn != controller || AnimationsManager.Instance.Animating || state == SelectionTarget.Targets;

        switch(state) {
            case SelectionTarget.Monster:
                UpdateMonsterSelector(mousePos);
                break;

            case SelectionTarget.Move:
                // buttons update themselves

                if(input.BackPressed()) {
                    state = SelectionTarget.Monster;
                    moveMenu.gameObject.SetActive(false);
                    level.ColorTiles(null, TileHighlighter.State.Highlighted);
                    break;
                }

                TileSelector.SetActive(false); // hide cursor when over menu
                if(!Global.GetObjectArea(moveMenu.Background).Contains(mousePos)) {
                    // if not hovering the move menu, check if selecting a different monster
                    UpdateMonsterSelector(mousePos);
                }
                break;

            case SelectionTarget.CraftChoice:
                // buttons update themselves

                if(input.BackPressed()) {
                    state = SelectionTarget.Monster;
                    buyMenu.gameObject.SetActive(false);
                    break;
                }

                TileSelector.SetActive(false); // hide cursor when over menu
                if(!Global.GetObjectArea(buyMenu.Background).Contains(mousePos)) {
                    // if not hovering the craft menu, check if selecting a different monster
                    UpdateMonsterSelector(mousePos);
                }
                break;

            case SelectionTarget.Targets:
                Vector2 closestMidpoint = tileGroupCenters.Min((Vector2 spot) => { return Vector2.Distance(mousePos, spot); });
                int hoveredTargetIndex = tileGroupCenters.IndexOf(closestMidpoint).Value;
                level.ColorTiles(tileGroups[hoveredTargetIndex], TileHighlighter.State.Hovered);

                if(input.SelectPressed()) {
                    // use the move on the hovered target
                    selected.UseMove(selectedMoveSlot, tileGroups[hoveredTargetIndex]);
                    level.ColorTiles(null, TileHighlighter.State.Hovered);
                    level.ColorTiles(null, TileHighlighter.State.Selectable);
                    state = SelectionTarget.Monster;
                }
                else if(input.BackPressed()) {
                    // go back to move selection of the selected monster
                    state = SelectionTarget.Move;
                    moveMenu.gameObject.SetActive(true);
                    level.ColorTiles(null, TileHighlighter.State.Hovered);
                    level.ColorTiles(null, TileHighlighter.State.Selectable);
                }
                break;
        }
    }

    public void EndTurn() {
        controller.EndTurn();
        moveMenu.gameObject.SetActive(false);
        buyMenu.gameObject.SetActive(false);
        state = SelectionTarget.Monster;
    }

    public void SelectMove(int moveSlot) {
        state = SelectionTarget.Targets;
        moveMenu.gameObject.SetActive(false);
        selectedMoveSlot = moveSlot;
        Move move = selected.Stats.Moves[selectedMoveSlot];
        bool filtered = move.TargetType == Move.Targets.UnaffectedFloor || move.TargetType == Move.Targets.Traversable || move.TargetType == Move.Targets.StandableSpot;
        tileGroups = selected.GetMoveOptions(selectedMoveSlot, filtered);
        tileGroupCenters = DetermineCenters(tileGroups);

        List<Vector2Int> allTiles = new List<Vector2Int>();
        foreach (List<Vector2Int> group in tileGroups)
        {
            allTiles.AddRange(group);
        }
        level.ColorTiles(allTiles, TileHighlighter.State.Selectable);
        level.ColorTiles(null, TileHighlighter.State.Highlighted);
    }

    public void OpenCraftMenu() {
        state = SelectionTarget.CraftChoice;
        moveMenu.gameObject.SetActive(false);
        buyMenu.Open(controller);
    }

    public void BuyMonster(MonsterName type) {
        controller.BuyMonster(type);
        buyMenu.gameObject.SetActive(false);
    }

    private void UpdateMonsterSelector(Vector2 mousePos) {
        TileSelector.SetActive(false);
        if(Global.GetObjectArea(endTurnButton.gameObject).Contains(mousePos) || Global.GetObjectArea(craftButton.gameObject).Contains(mousePos)) {
            return;
        }

        Vector3Int tile = level.Tiles.WorldToCell(mousePos);
        if(!level.IsInGrid((Vector2Int)tile)) {
            return;
        }

        TileSelector.SetActive(true);
        TileSelector.transform.position = level.Tiles.GetCellCenterWorld(tile);

        Monster hovered = level.GetMonster((Vector2Int)tile);
        if(hovered == null) {
            // close move menu when clicking off of it
            if(InputManager.Instance.SelectPressed()) {
                state = SelectionTarget.Monster;
                moveMenu.gameObject.SetActive(false);
                buyMenu.gameObject.SetActive(false);
            }
            return;
        }

        if(gameManager.CurrentTurn == controller && InputManager.Instance.SelectPressed()) {
            moveMenu.GetComponent<MoveMenu>().Open(hovered, controller);
            buyMenu.gameObject.SetActive(false);
            state = SelectionTarget.Move;
            selected = hovered;
        }
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

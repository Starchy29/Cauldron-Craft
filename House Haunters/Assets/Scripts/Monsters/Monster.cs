using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Monster : GridEntity
{
    [SerializeField] public HealthBarScript healthBar;
    [SerializeField] public MoveCounter MoveCounter;

    public MonsterType Stats { get; private set; }
    public int Health { get; private set; }

    public List<StatusAilment> Statuses { get; private set; } = new List<StatusAilment>();
    public List<UniqueStatus> UniqueStatuses { get; private set; } = new List<UniqueStatus>();

    public event Trigger OnTurnStart;
    public event Trigger OnTurnEnd;
    public event Trigger OnDeath;
    public delegate void AttackTrigger(Monster attacker, bool isMelee);
    public event AttackTrigger OnAttacked;

    public int[] Cooldowns {  get; private set; }
    public Shield CurrentShield { get; private set; }
    public int MovesLeft { get; private set; }

    public int MaxMoves { get { return 2 + (HasStatus(StatusEffect.Energy) ? 1 : 0) + (HasStatus(StatusEffect.Drowsiness) ? -1 : 0); } }
    public int CurrentSpeed { get { return Stats.Speed + (HasStatus(StatusEffect.Swiftness) ? StatusAilment.SPEED_BOOST : 0) + (HasStatus(StatusEffect.Slowness) ? -StatusAilment.SPEED_BOOST : 0); } }

    public static PathData[,] pathDistances; // set by level grid in Start()

    protected override void Start() {} // unlike other grid entities, only spawn from code

    public void Setup(MonsterName monsterType, Team controller) {
        controller.Join(this);
        SetOutlineColor(controller.TeamColor);
        spriteRenderer.sprite = PrefabContainer.Instance.monsterToSprite[monsterType];
        Stats = MonstersData.Instance.GetMonsterData(monsterType);
        MoveCounter.Setup(this);

        Health = Stats.Health;
        Cooldowns = new int[Stats.Moves.Length];

        OnTurnStart += RefreshMoves;
        OnTurnStart += ReduceShield;
        OnTurnEnd += DecreaseCooldowns;
        OnTurnEnd += CheckStatuses;

        RefreshMoves();
    }

    public void StartTurn() {
        OnTurnStart();
    }

    public void EndTurn() {
        OnTurnEnd();
    }

    public void Heal(int amount) {
        Health += amount;
        if(Health > Stats.Health) {
            Health = Stats.Health;
        }
        AnimationsManager.Instance.QueueAnimation(new HealthBarAnimator(healthBar, Health));
    }

    public void ReceiveAttack(int damage, Monster attacker, bool isMelee = false) {
        float multiplier = 1f + (attacker.HasStatus(StatusEffect.Strength) ? 0.5f : 0f) + (attacker.HasStatus(StatusEffect.Fear) ? -0.5f : 0f);
        if(HasStatus(StatusEffect.Haunted)) {
            multiplier *= 1.5f;
        }
        if(CurrentShield != null) {
            multiplier *= CurrentShield.DamageMultiplier;
            if(CurrentShield.BlocksOnce) {
                RemoveShield();
            }
        }
        if(multiplier != 1f) {
            damage = Mathf.CeilToInt(damage * multiplier);
        }

        TakeDamage(damage);
        OnAttacked?.Invoke(attacker, isMelee);
    }

    public void TakeDamage(int amount) {
        Health -= amount;
        if(Health < 0) {
            Health = 0;
        }
        AnimationsManager.Instance.QueueAnimation(new HealthBarAnimator(healthBar, Health));

        if(Health == 0) {
            GameManager.Instance.DefeatMonster(this);
            AnimationsManager.Instance.QueueAnimation(new DestructionAnimator(this.gameObject));
            OnDeath?.Invoke();
            foreach(StatusAilment status in Statuses) {
                status.Terminate();
            }
        }
    }

    public bool HasStatus(StatusEffect status) {
        TileAffector tileEffect = LevelGrid.Instance.GetTile(Tile).CurrentEffect;
        return Statuses.Find((StatusAilment condition) => { return condition.effects.Contains(status); }) != null
            || (tileEffect != null && tileEffect.Controller != Controller && tileEffect.AppliedStatus == status);
    }

    public void ApplyStatus(StatusAilment blueprint, Monster user) {
        StatusAilment duplicate = Statuses.Find((StatusAilment existing) => { return existing == blueprint; });
        if(duplicate != null) {
            duplicate.duration = blueprint.duration; // reset duration;
            return;
        }

        GameObject visual = Instantiate(blueprint.visual);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.SetActive(false);
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(visual, true));

        StatusAilment affliction = new StatusAilment(blueprint.effects, blueprint.duration, visual);
        Statuses.Add(affliction);
    }

    public List<List<Vector2Int>> GetMoveOptions(int moveSlot, bool filtered = true) {
        return Stats.Moves[moveSlot].GetOptions(this, filtered);
    }

    // returns all target groups from each tile this monster can move to. Does not include options from staying on the current tile
    public Dictionary<Vector2Int, List<List<Vector2Int>>> GetMoveOptionsAfterWalk(int moveSlot, bool includeUselessTiles, List<List<Vector2Int>> standableSpots = null) {
        if(standableSpots == null) {
            standableSpots = GetMoveOptions(MonsterType.WALK_INDEX);
        }
        
        LevelGrid level = LevelGrid.Instance;
        Dictionary<Vector2Int, List<List<Vector2Int>>> result = new Dictionary<Vector2Int, List<List<Vector2Int>>>();
        Vector2Int startTile = Tile;
        foreach(List<Vector2Int> standableSpot in standableSpots) {
            level.TestEntity(this, standableSpot[0]);
            result[standableSpot[0]] = Stats.Moves[moveSlot].GetOptions(this, !includeUselessTiles, !includeUselessTiles);
        }
        level.TestEntity(this, startTile);

        return result;
    }

    public void UseMove(int moveSlot, List<Vector2Int> tiles) {
        Vector2 selectionMid = Global.DetermineCenter(tiles);
        if(selectionMid.x > transform.position.x) {
            AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(() => { SetSpriteFlip(false); }));
        }
        else if(selectionMid.x < transform.position.x) {
            AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(() => { SetSpriteFlip(true); }));
        }

        Stats.Moves[moveSlot].Use(this, tiles);
        Cooldowns[moveSlot] = Stats.Moves[moveSlot].Cooldown;
        MovesLeft--;
    }

    public bool CanUse(int moveSlot) {
        return Stats.Moves[moveSlot] != null && MovesLeft > 0 && Cooldowns[moveSlot] == 0 && GetMoveOptions(moveSlot).Count > 0;
    }

    public List<int> GetUsableMoveSlots() {
        List<int> moveOptions = new List<int>();
        for(int i = 0; i < Stats.Moves.Length; i++) {
            if(CanUse(i)) {
                moveOptions.Add(i);
            }
        }
        return moveOptions;
    }

    public void ApplyShield(Shield shield) {
        RemoveShield();
        CurrentShield = new Shield(shield.StrengthLevel, shield.Duration, shield.BlocksOnce, Instantiate(shield.Visual));
        CurrentShield.Visual.transform.SetParent(transform, false);
        CurrentShield.Visual.transform.localPosition = Vector3.zero;
        CurrentShield.Visual.SetActive(false);
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(CurrentShield.Visual, true));
    }

    public bool CanMoveTo(Vector2Int tile) {
        return LevelGrid.Instance.GetTile(tile).Walkable && LevelGrid.Instance.GetEntity(tile) == null;
    }

    public void RemoveShield() {
        if(CurrentShield == null) {
            return;
        }

        Destroy(CurrentShield.Visual);
        CurrentShield = null;
    }

    private void RefreshMoves() {
        MovesLeft = MaxMoves;
    }

    private void ReduceShield() {
        if(CurrentShield != null) {
            CurrentShield.Duration--;
            if(CurrentShield.Duration <= 0) {
                RemoveShield();
            }
        }
    }

    private void CheckStatuses() {
        if(HasStatus(StatusEffect.Regeneration)) {
            Heal(2);
        }
        if(HasStatus(StatusEffect.Poison)) {
            TakeDamage(2);
        }

        for(int i = Statuses.Count - 1; i >= 0; i--) {
            Statuses[i].duration--;
            if(Statuses[i].duration <= 0) {
                Statuses[i].Terminate();
                Statuses.RemoveAt(i);
            }
        }
    }

    private void DecreaseCooldowns() {
        for(int i = 0; i < Cooldowns.Length; i++) {
            if(Cooldowns[i] > 0) {
                Cooldowns[i]--;
            }
        }
    }

    public struct PathData {
        public Vector2Int? previous;
        public int travelDistance;
        public int distanceToEnd;
        public int Estimate { get { return travelDistance + distanceToEnd; } }
    }

    // finds the generic walkable path from a start point to an end point
    public static List<Vector2Int> FindPath(Vector2Int startTile, Vector2Int endTile) {
        return FindPath(startTile, endTile, null);
    }

    // asks a monster if it can get to the end tile in move, and finds that path while navigating around obstacle
    public List<Vector2Int> FindPath(Vector2Int endTile) {
        return FindPath(Tile, endTile, this);
    }

    // finds the shortest path to the end tile. Returns null if there is no path.
    // If validating a move, returns null if this cannot get to the end in one move this turn
    private static List<Vector2Int> FindPath(Vector2Int startTile, Vector2Int endTile, Monster traveller) {
        LevelGrid level = LevelGrid.Instance;
        bool validateMove = traveller != null;
        if(validateMove && (!traveller.CanMoveTo(endTile) || Global.CalcTileDistance(startTile, endTile) > traveller.CurrentSpeed)) {
            return null;
        }
        if(!validateMove && !level.GetTile(endTile).Walkable) {
            return null;
        }

        // set up data array for pathfinding
        for(int y = 0; y < LevelGrid.Instance.Height; y++) {
            for(int x = 0; x < LevelGrid.Instance.Width; x++) {
                pathDistances[y, x] = new PathData();
            }
        }
        pathDistances[startTile.y, startTile.x].travelDistance = 0;
        pathDistances[startTile.y, startTile.x].distanceToEnd = Global.CalcTileDistance(startTile, endTile);

        // find the shortest path using A*
        List<Vector2Int> closedList = new List<Vector2Int>();
        List<Vector2Int> openList = new List<Vector2Int>() { startTile };
        while(openList.Count > 0) {
            // find the best option in the open list
            Vector2Int nextTile = openList.Min((Vector2Int tile) => { return pathDistances[tile.y, tile.x].Estimate; });

            // check if this is the end
            if(nextTile == endTile) {
                if(validateMove && pathDistances[nextTile.y, nextTile.x].travelDistance > traveller.CurrentSpeed) {
                    // invalid if too many steps
                    return null;
                }

                List<Vector2Int> path = new List<Vector2Int>();
                while(pathDistances[nextTile.y, nextTile.x].previous != null) {
                    path.Add(nextTile);
                    nextTile = pathDistances[nextTile.y, nextTile.x].previous.Value;
                }
                path.Reverse();
                return path;
            }

            openList.Remove(nextTile);
            closedList.Add(nextTile);

            // prevent moving to a neighbor when the tile is trapped
            TileAffector nextEffect = level.GetTile(nextTile).CurrentEffect;
            if(validateMove && nextTile != startTile && nextEffect != null && nextEffect.Controller != traveller.Controller && nextEffect.StopsMovement) {
                continue;
            }

            // update the neighbors
            foreach(Vector2Int direction in Global.Cardinals) {
                Vector2Int neighbor = nextTile + direction;
                
                if(!level.IsInGrid(neighbor) || !level.GetTile(neighbor).Walkable) {
                    continue;
                }
                GridEntity occupant = level.GetEntity(neighbor);
                if(validateMove && occupant != null && (occupant.Controller != traveller.Controller || !(occupant is Monster))) {
                    continue;
                }

                bool inOpen = openList.Contains(neighbor);
                bool inClosed = closedList.Contains(neighbor);
                int discoveredDistance = pathDistances[nextTile.y, nextTile.x].travelDistance + (validateMove ? level.GetTile(neighbor).GetTravelCost(traveller) : 1);
                if(!inOpen && !inClosed) {
                    // found a new route
                    openList.Add(neighbor);
                    pathDistances[neighbor.y, neighbor.x].previous = nextTile;
                    pathDistances[neighbor.y, neighbor.x].travelDistance = discoveredDistance;
                    pathDistances[neighbor.y, neighbor.x].distanceToEnd = Global.CalcTileDistance(neighbor, endTile);
                }
                else if(inOpen && discoveredDistance < pathDistances[neighbor.y, neighbor.x].travelDistance) {
                    // found a shorter route
                    pathDistances[neighbor.y, neighbor.x].previous = nextTile;
                    pathDistances[neighbor.y, neighbor.x].travelDistance = discoveredDistance;
                }
            }
        }

        return null; // no valid path
    }
}

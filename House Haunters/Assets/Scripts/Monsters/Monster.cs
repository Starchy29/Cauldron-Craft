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
    public delegate void AttackTrigger(Attack attack, Monster attacker);
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

    public void TriggerAttackEffects(Attack attack, Monster attacker) {
        OnAttacked?.Invoke(attack, attacker);
    }

    public void Heal(int amount) {
        Health += amount;
        if(Health > Stats.Health) {
            Health = Stats.Health;
        }
        AnimationsManager.Instance.QueueAnimation(new HealthBarAnimator(healthBar, Health));
    }

    public void TakeDamage(int amount, Monster source = null) {
        if(source != null) {
            float multiplier = 1f + (source.HasStatus(StatusEffect.Strength) ? 0.5f : 0f) + (source.HasStatus(StatusEffect.Fear) ? -0.5f : 0f);
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
                amount = Mathf.CeilToInt(amount * multiplier);
            }
        }

        Health -= amount;
        if(Health < 0) {
            Health = 0;
        }
        AnimationsManager.Instance.QueueAnimation(new HealthBarAnimator(healthBar, Health));

        if(Health == 0) {
            GameManager.Instance.DefeatMonster(this);
            AnimationsManager.Instance.QueueAnimation(new DeathAnimator(this));
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

    public void UseMove(int moveSlot, List<Vector2Int> tiles) {
        Stats.Moves[moveSlot].Use(this, tiles);
        Cooldowns[moveSlot] = Stats.Moves[moveSlot].Cooldown;
        MovesLeft--;

        Vector2 selectionMid = Global.DetermineCenter(tiles);
        if(selectionMid.x > transform.position.x) {
            SetSpriteFlip(false);
        }
        else if(selectionMid.x < transform.position.x) {
            SetSpriteFlip(true);
        }
    }

    public bool CanUse(int moveSlot) {
        return Stats.Moves[moveSlot] != null && MovesLeft > 0 && Cooldowns[moveSlot] == 0 && GetMoveOptions(moveSlot).Count > 0;
    }

    public void ApplyShield(Shield shield) {
        CurrentShield = new Shield(shield.StrengthLevel, shield.Duration, shield.BlocksOnce, Instantiate(shield.Visual));
        CurrentShield.Visual.transform.SetParent(transform, false);
        CurrentShield.Visual.transform.localPosition = Vector3.zero;
    }

    // returns true if the input tile is one that this monster can legally stand on assuming it is unoccupied
    public bool CouldStandOn(Vector2Int tile) {
        WorldTile levelTile = LevelGrid.Instance.GetTile(tile);
        return !levelTile.IsWall && (levelTile.Walkable || Stats.Flying);
    }

    public bool CanMoveTo(Vector2Int tile) {
        return CouldStandOn(tile) && LevelGrid.Instance.GetEntity(tile) == null;
    }

    public void RemoveShield() {
        if(CurrentShield == null) {
            return;
        }

        Destroy(CurrentShield.Visual);
        CurrentShield = null;
    }

    public struct PathData {
        public Vector2Int? previous;
        public int travelDistance;
        public int distanceToEnd;
        public int Estimate { get { return travelDistance + distanceToEnd; } }
    }

    // returns null if this monster cannot get to the tile with one movement
    public List<Vector2Int> FindPath(Vector2Int endTile) {
        LevelGrid level = LevelGrid.Instance;
        if(!CanMoveTo(endTile) || Global.CalcTileDistance(Tile, endTile) > CurrentSpeed) {
            return null;
        }

        // set up data array for pathfinding
        for(int y = 0; y < LevelGrid.Instance.Height; y++) {
            for(int x = 0; x < LevelGrid.Instance.Width; x++) {
                pathDistances[y, x] = new PathData();
            }
        }
        pathDistances[Tile.y, Tile.x].travelDistance = 0;
        pathDistances[Tile.y, Tile.x].distanceToEnd = Global.CalcTileDistance(Tile, endTile);

        // find the shortest path using A*
        List<Vector2Int> closedList = new List<Vector2Int>();
        List<Vector2Int> openList = new List<Vector2Int>() { Tile };
        while(openList.Count > 0) {
            // find the best option in the open list
            Vector2Int nextTile = openList.Min((Vector2Int tile) => { return pathDistances[tile.y, tile.x].Estimate; });

            // check if this is the end
            if(nextTile == endTile) {
                if(pathDistances[nextTile.y, nextTile.x].travelDistance > CurrentSpeed) {
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
            if(nextTile != Tile && nextEffect != null && nextEffect.Controller != Controller && nextEffect.StopsMovement) {
                continue;
            }

            // update the neighbors
            foreach(Vector2Int direction in Global.Cardinals) {
                Vector2Int neighbor = nextTile + direction;
                
                if(!level.IsInGrid(neighbor) || !CouldStandOn(neighbor)) {
                    continue;
                }
                GridEntity occupant = level.GetEntity(neighbor);
                if(occupant != null && occupant.Controller != Controller) {
                    continue;
                }

                bool inOpen = openList.Contains(neighbor);
                bool inClosed = closedList.Contains(neighbor);
                int discoveredDistance = pathDistances[nextTile.y, nextTile.x].travelDistance + level.GetTile(neighbor).GetTravelCost(this);
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
            TakeDamage(2, null);
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
}

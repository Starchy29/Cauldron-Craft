using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Monster : GridEntity
{
    [SerializeField] private MonsterName monsterType;

    public Team Controller { get; set; }
    public MonsterType Stats { get; private set; }
    private int health;
    private int[] cooldowns;
    private Dictionary<StatusEffect, int> effectDurations;

    public Trigger OnTurnStart;
    public Trigger OnTurnEnd;
    public Shield CurrentShield { get; private set; }
    public int MovesLeft { get; private set; }
    public int MaxMoves { get { return 2 + (HasStatus(StatusEffect.Energy) ? 1 : 0) + (HasStatus(StatusEffect.Energy) ? -1 : 0); } }

    public int CurrentSpeed { get { return Stats.Speed + (HasStatus(StatusEffect.Haste) ? 2 : 0) + (HasStatus(StatusEffect.Slowness) ? -2 : 0); } }
    public float DamageMultiplier { get { return 1f + (HasStatus(StatusEffect.Strength)? 0.5f : 0f) + (HasStatus(StatusEffect.Fear)? -0.5f : 0f); } }

    void Start() {
        Stats = MonstersData.Instance.GetMonsterData(monsterType);
        health = Stats.Health;
        cooldowns = new int[Stats.Moves.Length];

        OnTurnStart += RefreshMoves;
        OnTurnEnd += EndTurn;

        StatusEffect[] statuses = (StatusEffect[])Enum.GetValues(typeof(StatusEffect));
        effectDurations = new Dictionary<StatusEffect, int>(statuses.Length);
        foreach(StatusEffect status in statuses) {
            effectDurations[status] = 0;
        }
    }

    void Update() {
        
    }

    public void Heal(int amount) {
        if(HasStatus(StatusEffect.Cursed)) {
            return;
        }

        health += amount;
        if(health > Stats.Health) {
            health = Stats.Health;
        }
    }

    public void TakeDamage(int amount, bool applyMultipliers = true) {
        if(applyMultipliers) {
            float multiplier = 1f;
            if(HasStatus(StatusEffect.Haunted)) {
                multiplier *= 1.5f;
            }
            if(CurrentShield != null) {
                multiplier *= CurrentShield.DamageMultiplier;
                if(CurrentShield.BlocksOnce) {
                    CurrentShield = null;
                }
            }
            if(multiplier != 1f) {
                amount = Mathf.FloorToInt(amount * multiplier);
            }
        }

        health -= amount;
        if(health <= 0) {
            //Die();
        }
    }

    public bool HasStatus(StatusEffect status) {
        TileEffect tileEffect = LevelGrid.Instance.GetTile(Tile).CurrentEffect;
        return effectDurations[status] > 0 || (tileEffect != null && tileEffect.AppliedStatus == status);
    }

    public void ApplyStatus(StatusEffect status, int duration) {
        if(CurrentShield != null && CurrentShield.BlocksStatus) {
            if(CurrentShield.BlocksOnce) {
                CurrentShield = null;
            }
            return;
        }

        if(duration > effectDurations[status]) {
            effectDurations[status] = duration;
        }
    }

    public List<List<Vector2Int>> GetMoveOptions(int moveSlot) {
        return Stats.Moves[moveSlot].GetOptions(this);
    }

    public void UseMove(int moveSlot, List<Vector2Int> tiles) {
        Stats.Moves[moveSlot].Use(this, tiles);
        cooldowns[moveSlot] = Stats.Moves[moveSlot].Cooldown;
        MovesLeft--;
    }

    public bool CanUse(int moveSlot) {
        Move move = Stats.Moves[moveSlot];
        return move != null && MovesLeft > 0 && cooldowns[moveSlot] == 0 && !(HasStatus(StatusEffect.Frozen) && move.Type == Move.MoveType.Movement) 
            && GetMoveOptions(moveSlot).Count > 0;
    }

    public void ApplyShield(Shield shield) {
        CurrentShield = shield;
    }

    public bool CanStandOn(Vector2Int tile) {
        WorldTile levelTile = LevelGrid.Instance.GetTile(tile);
        return !levelTile.IsWall && (levelTile.Walkable || Stats.Flying) && LevelGrid.Instance.GetEntity(tile) == null;
    }

    // returns null if this monster cannot get to the tile with one movement
    public List<Vector2Int> FindPath(Vector2Int endTile) {
        LevelGrid level = LevelGrid.Instance;
        if(!CanStandOn(endTile)) {
            return null;
        }

        // set up data array for pathfinding
        PathData[,] distances = new PathData[LevelGrid.height, LevelGrid.width];
        for(int y = 0; y < LevelGrid.height; y++) {
            for(int x = 0; x < LevelGrid.width; x++) {
                distances[y, x] = new PathData();
            }
        }
        distances[Tile.y, Tile.x].travelDistance = 0;
        distances[Tile.y, Tile.x].distanceToEnd = Global.CalcTileDistance(Tile, endTile);

        // find the shortest path using A*
        List<Vector2Int> closedList = new List<Vector2Int>();
        List<Vector2Int> openList = new List<Vector2Int>() { Tile };
        while(openList.Count > 0) {
            // find the best option in the open list
            Vector2Int nextTile = openList.Min((Vector2Int tile) => { return distances[tile.y, tile.x].Estimate; });

            // check if this is the end
            if(nextTile == endTile) {
                if(distances[nextTile.y, nextTile.x].travelDistance > Stats.Speed) {
                    // invalid if too many steps
                    return null;
                }

                List<Vector2Int> path = new List<Vector2Int>();
                while(distances[nextTile.y, nextTile.x].previous != null) {
                    path.Add(nextTile);
                    nextTile = distances[nextTile.y, nextTile.x].previous.Value;
                }
                path.Reverse();
                return path;
            }

            openList.Remove(nextTile);
            closedList.Add(nextTile);

            // update the neighbors
            foreach(Vector2Int direction in Global.Cardinals) {
                Vector2Int neighbor = nextTile + direction;
                if(!level.IsInGrid(neighbor) || !CanStandOn(neighbor)) {
                    continue; // walls and pits are not navigable
                }

                bool inOpen = openList.Contains(neighbor);
                bool inClosed = closedList.Contains(neighbor);
                int discoveredDistance = distances[nextTile.y, nextTile.x].travelDistance + level.GetTile(neighbor).GetTravelCost(this);
                if(!inOpen && !inClosed) {
                    // found a new route
                    openList.Add(neighbor);
                    distances[neighbor.y, neighbor.x].previous = nextTile;
                    distances[neighbor.y, neighbor.x].travelDistance = discoveredDistance;
                    distances[neighbor.y, neighbor.x].distanceToEnd = Global.CalcTileDistance(neighbor, endTile);
                }
                else if(inOpen && discoveredDistance < distances[neighbor.y, neighbor.x].travelDistance) {
                    // found a shorter route
                    distances[neighbor.y, neighbor.x].previous = nextTile;
                    distances[neighbor.y, neighbor.x].travelDistance = discoveredDistance;
                }
            }
        }

        return null; // no valid path
    }

    private struct PathData {
        public Vector2Int? previous;
        public int travelDistance;
        public int distanceToEnd;
        public int Estimate { get { return travelDistance + distanceToEnd; } }
    }

    private void RefreshMoves() {
        MovesLeft = MaxMoves;
    }

    private void EndTurn() {
        // decrease cooldowns
        for(int i = 0; i < cooldowns.Length; i++) {
            if(cooldowns[i] > 0) {
                cooldowns[i]--;
            }
        }

        // reduce the shield duration
        if(CurrentShield != null) {
            CurrentShield.Duration--;
            if(CurrentShield.Duration <= 0) {
                CurrentShield = null;
            }
        }

        // handle status effects
        if(HasStatus(StatusEffect.Regeneration)) {
            Heal(1);
        }
        if(HasStatus(StatusEffect.Poison)) {
            TakeDamage(1, false);
        }

        foreach(StatusEffect status in Enum.GetValues(typeof(StatusEffect))) {
            if(effectDurations[status] > 0) {
                effectDurations[status]--;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Monster : GridEntity
{
    [SerializeField] public HealthBarScript healthBar;
    [SerializeField] public SpriteRenderer moveIndicator;

    public MonsterType Stats { get; private set; }
    public int Health { get; private set; }

    public List<StatusAilment> Statuses { get; private set; } = new List<StatusAilment>();

    public event Trigger OnTurnStart;
    public event Trigger OnTurnEnd;
    public event Trigger OnDeath;

    public int[] Cooldowns {  get; private set; }
    public bool AbilityAvailable { get; private set; }
    public bool WalkAvailable { get { return Cooldowns[MonsterType.WALK_INDEX] == 0 && CurrentSpeed > 0; } }
    public int CurrentSpeed { get { return Stats.Speed + (HasStatus(StatusEffect.Swift) ? 1 : 0) + (HasStatus(StatusEffect.Slowness) ? -2 : 0); } }
    public GameObject SpriteModel { get { return spriteRenderer.gameObject; } }

    public static PathData[,] pathDistances; // set by level grid in Start()

    protected override void Start() {} // unlike other grid entities, only spawn from code

    public void Setup(MonsterName monsterType, Team controller) {
        controller.Join(this);
        SetOutlineColor(controller.TeamColor);
        spriteRenderer.sprite = PrefabContainer.Instance.monsterToSprite[monsterType];
        Stats = MonstersData.Instance.GetMonsterData(monsterType);

        Health = Stats.Health;
        Cooldowns = new int[Stats.Moves.Length];

        OnTurnStart += RefreshMoves;
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

        AnimationsManager.Instance.QueueSound(Sounds.Heal);
        AnimationsManager.Instance.QueueFunction(() => { 
            GameObject particle = Instantiate(PrefabContainer.Instance.healParticle);
            particle.transform.position = SpriteModel.transform.position;
            particle = Instantiate(PrefabContainer.Instance.healParticle);
            particle.transform.position = SpriteModel.transform.position;
        });
        AnimationsManager.Instance.QueueAnimation(new HealthBarAnimator(healthBar, Health));
    }

    public int DetermineDamage(int startDamage, Monster attacker) {
        float multiplier = 1f 
            + (attacker.HasStatus(StatusEffect.Power) ? 0.5f : 0f) 
            + (attacker.HasStatus(StatusEffect.Fear) ? -0.5f : 0f)
            + (HasStatus(StatusEffect.Haunt) ? 0.5f : 0f)
            + (HasStatus(StatusEffect.Sturdy) ? -0.5f : 0f);
        if(multiplier < 0.25f) {
            multiplier = 0.25f;
        }
        return Mathf.CeilToInt(startDamage * multiplier);
    }

    public void TakeDamage(int amount, Monster attacker = null) {
        if(attacker != null) {
            amount = DetermineDamage(amount, attacker);
        }

        Health -= amount;
        if(Health < 0) {
            Health = 0;
        }

        int numChunks = 1;
        if(amount > 10) {
            numChunks = 3;
        }
        else if(amount > 5) {
            numChunks = 2;
        }

        AnimationsManager.Instance.QueueFunction(() => { 
            for(int i = 0; i < numChunks; i++) {
                SpawnHurtParticle();
            }
        });

        AnimationsManager.Instance.QueueAnimation(new HealthBarAnimator(healthBar, Health));

        if(attacker != null && HasStatus(StatusEffect.Cursed)) {
            attacker.TakeDamage(4);
        }

        if(Health == 0) {
            AnimationsManager.Instance.QueueSound(Sounds.Death);
            AnimationsManager.Instance.QueueFunction(() => { 
                for(int i = 0; i < 15; i++) {
                    SpawnHurtParticle();
                }
            });
            AnimationsManager.Instance.QueueAnimation(new DestructionAnimator(this.gameObject));
            GameManager.Instance.DefeatMonster(this);
            OnDeath?.Invoke();
            foreach(StatusAilment status in Statuses) {
                status.Terminate();
            }
        }
    }

    public bool HasStatus(StatusEffect status) {
        return Statuses.Exists((StatusAilment condition) => { return condition.effect == status; });
    }

    public GameObject ApplyStatus(StatusAilment blueprint) {
        StatusAilment duplicate = Statuses.Find((StatusAilment existing) => { return existing.blueprintMadeFrom == blueprint; });
        if(duplicate != null) {
            duplicate.duration = blueprint.duration; // reset duration;
            return duplicate.visual;
        }

        GameObject visual = Instantiate(blueprint.visual);
        visual.transform.SetParent(spriteRenderer.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.SetActive(false);
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(visual, true));

        StatusAilment affliction = new StatusAilment(blueprint.effect, blueprint.duration, visual);
        affliction.blueprintMadeFrom = blueprint;
        Statuses.Add(affliction);
        return visual;
    }

    public List<Selection> GetMoveOptions(int moveSlot, bool ignoreUseless = true) {
        return Stats.Moves[moveSlot].GetOptions(this, ignoreUseless);
    }

    // returns all target groups from each tile this monster can move to. Does not include options from staying on the current tile
    public Dictionary<Vector2Int, List<Selection>> GetMoveOptionsAfterWalk(int moveSlot, bool includeAllTargetArea, List<Vector2Int> standableSpots = null) {
        if(standableSpots == null) {
            standableSpots = GetMoveOptions(MonsterType.WALK_INDEX).ConvertAll((Selection tileContainer) => tileContainer.Filtered[0]);
        }
        
        LevelGrid level = LevelGrid.Instance;
        Dictionary<Vector2Int, List<Selection>> result = new Dictionary<Vector2Int, List<Selection>>();
        if(Stats.Moves[moveSlot].CantWalkFirst) {
            // no options after walk
            return result;
        }

        Vector2Int startTile = Tile;
        foreach(Vector2Int standableSpot in standableSpots) {
            level.TestEntity(this, standableSpot);
            result[standableSpot] = Stats.Moves[moveSlot].GetOptions(this, !includeAllTargetArea);
        }

        if(Tile != startTile) {
            level.TestEntity(this, startTile);
        }

        return result;
    }

    public void UseMove(int moveSlot, Selection targets) {
        Move move = Stats.Moves[moveSlot];

        Vector2 selectionMid = Global.DetermineCenter(targets.Filtered);
        AnimationsManager.Instance.QueueFunction(() => {
            if(selectionMid.x > transform.position.x) {
                SetSpriteFlip(false);
            }
            else if(selectionMid.x < transform.position.x) {
                SetSpriteFlip(true);
            }
        });

        Stats.Moves[moveSlot].Use(this, targets);
        Cooldowns[moveSlot] = Stats.Moves[moveSlot].Cooldown;
        
        if(moveSlot != MonsterType.WALK_INDEX) {
            AbilityAvailable = false;
        }
    }

    public bool CanUse(int moveSlot) {
        if(Stats.Moves[moveSlot].CantWalkFirst && !WalkAvailable) {
            return false;
        }

        return (moveSlot == MonsterType.WALK_INDEX || AbilityAvailable) && Cooldowns[moveSlot] == 0 && GetMoveOptions(moveSlot).Count > 0;
    }

    private void RefreshMoves() {
        AbilityAvailable = true;
    }

    private void CheckStatuses() {
        if(HasStatus(StatusEffect.Poison)) {
            AnimationsManager.Instance.QueueAnimation(new CameraAnimator(transform.position));
            AnimationsManager.Instance.QueueSound(Sounds.Bubbles);
            AnimationsManager.Instance.QueueFunction(() => {
                GameObject particle = Instantiate(PrefabContainer.Instance.poisonParticle);
                particle.transform.position = SpriteModel.transform.position;
                particle = Instantiate(PrefabContainer.Instance.poisonParticle);
                particle.transform.position = SpriteModel.transform.position;
            });
            TakeDamage(5);
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

    private void SpawnHurtParticle() {
        GameObject particle = Instantiate(PrefabContainer.Instance.HurtParticle);
        particle.transform.position = SpriteModel.transform.position;
        particle.GetComponent<HurtParticle>().Randomize(SpriteModel.GetComponent<SpriteRenderer>().sprite);
    }

    // finds the shortest path to the end tile. Returns null if there is no path. Does not matter if the end spot is occupied
    // If validating a move, returns null if this cannot get to the end in one move this turn
    private static List<Vector2Int> FindPath(Vector2Int startTile, Vector2Int endTile, Monster traveller) {
        LevelGrid level = LevelGrid.Instance;
        bool validateMove = traveller != null;
        if(validateMove && (!level.GetTile(endTile).Walkable || Global.CalcTileDistance(startTile, endTile) > traveller.CurrentSpeed)) {
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

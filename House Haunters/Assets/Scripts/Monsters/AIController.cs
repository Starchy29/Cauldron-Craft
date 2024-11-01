using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIController
{
    private Team controlTarget;

    private static Dictionary<Monster, List<Vector2Int>> walkOptions = new Dictionary<Monster, List<Vector2Int>>(); // cache to prevent repeated pathfinding
    private static Dictionary<Monster, Vector2Int> pathedPositions = new Dictionary<Monster, Vector2Int>();
    private static Vector2 idealZoneMiddle;

    private enum TeamRole {
        Offense,
        Defense,
        Support
    }
    private static Dictionary<MonsterName, TeamRole> monsterRoles = new Dictionary<MonsterName, TeamRole> {
        { MonsterName.LostSoul, TeamRole.Support },
        { MonsterName.Golem, TeamRole.Support },
        { MonsterName.Fungus, TeamRole.Support },
        { MonsterName.Jackolantern, TeamRole.Support },
        { MonsterName.Cactus, TeamRole.Defense },
        { MonsterName.Fossil, TeamRole.Defense },
        { MonsterName.Sludge, TeamRole.Defense },
        { MonsterName.Automaton, TeamRole.Defense },
        { MonsterName.Amalgamation, TeamRole.Defense },
        { MonsterName.Flytrap, TeamRole.Offense },
        { MonsterName.Demon, TeamRole.Offense },
        { MonsterName.Beast, TeamRole.Offense },
        { MonsterName.Phantom, TeamRole.Offense },
    };

    private GamePlan plan;
    Dictionary<Ingredient, int> allyNeeds = new Dictionary<Ingredient, int>();

    public AIController(Team team) {
        this.controlTarget = team;
        plan = new GamePlan(new List<ResourcePile>(GameObject.FindObjectsByType<ResourcePile>(FindObjectsSortMode.None)));
    }

    // called by Team.cs after the start team is spawned
    public void ChooseStartPlan() {
        // start attacking 2 random resources
        Monster supporter = controlTarget.Teammates.Find((Monster teammate) => monsterRoles[teammate.Stats.Name] == TeamRole.Support);
        if(supporter == null) {
            supporter = controlTarget.Teammates[UnityEngine.Random.Range(0, controlTarget.Teammates.Count)];
        }
        List<Monster> attackers = controlTarget.Teammates.FindAll((Monster teammate) => teammate != supporter);

        ResourcePile notAttacked = GameManager.Instance.AllResources[UnityEngine.Random.Range(0, GameManager.Instance.AllResources.Count)];
        List<ResourcePile> attackTargets = GameManager.Instance.AllResources.FindAll((ResourcePile resource) => resource != notAttacked);

        for(int i = 0; i < attackTargets.Count; i++) {
            plan.Assign(attackers[i], attackTargets[i]);
        }
        plan.Assign(supporter, attackTargets[UnityEngine.Random.Range(0, attackTargets.Count)]);
    }

    public void RemoveMonster(Monster defeated) {
        plan.Remove(defeated);
    }

    public void AddMonster(Monster spawned) {
        plan.Assign(spawned, null); // start assigned to nothing, get assignment later
    }

    // enacts every move in the turn immediately. The AnimationsManager makes the visuals play out in the correct order
    public void TakeTurn() {
        FindWalkableTiles();

        // find center of conflict for zone placement
        Vector2 allyCenter = controlTarget.Teammates
            .ConvertAll(monster => (Vector2)monster.transform.position)
            .Collapse((Vector2 cur, Vector2 next) => cur + next)
            / controlTarget.Teammates.Count;

        Vector2 enemyCenter = GameManager.Instance.OpponentOf(controlTarget).Teammates
            .ConvertAll(monster => (Vector2)monster.transform.position)
            .Collapse((Vector2 cur, Vector2 next) => cur + next)
            / GameManager.Instance.OpponentOf(controlTarget).Teammates.Count;

        idealZoneMiddle = .25f * allyCenter + .75f * enemyCenter;

        // update the game plan
        Dictionary<ResourcePile, ResourceData> resourceData = EvaluateResources();
        List<ResourcePile> targets = new List<ResourcePile>(plan.GetTargetResources()); // moifies collection by unassigning, so need to copy
        foreach(ResourcePile resource in targets) {
            // unassign defending monsters if possible
            ResourceData info = resourceData[resource];
            if(resource.Controller == null || resource.Contested) {
                continue;
            }

            List<Monster> assignees = plan.GetAssignedAt(resource);

            if(info.priority < 0.1f || assignees.Find((Monster monster) => monsterRoles[monster.Stats.Name] != TeamRole.Support) == null) {
                // abandon all
                List<Monster> assignedHere = plan.GetAssignedAt(resource);
                for(int i = assignedHere.Count - 1; i >= 0; i--) {
                    plan.Assign(assignedHere[i], null);
                }
            }
            else if(assignees.Count > 1 && info.threatValue < 0.5f) {
                // leave one behind
                Monster stayer = assignees.Find((Monster ally) => monsterRoles[ally.Stats.Name] == TeamRole.Defense);
                if(stayer == null) {
                    stayer = assignees.Find((Monster ally) => monsterRoles[ally.Stats.Name] == TeamRole.Offense);
                }
                if(stayer == null) {
                    stayer = assignees[UnityEngine.Random.Range(0, assignees.Count)];
                }
                List<Monster> assignedHere = plan.GetAssignedAt(resource);
                for(int i = assignedHere.Count - 1; i >= 0; i--) {
                    if(assignedHere[i] == stayer) {
                        continue;
                    }
                    plan.Assign(assignedHere[i], null);
                }
            }
        }

        // choose whether to launch an attack on a new resource or support an existing resource
        List<Monster> unassigned = plan.GetUnassigned();
        if(unassigned.Count > 0) {
            // consider shoring up a defense or an existing attack
            ResourcePile chosenTarget = null;
            List<ResourcePile> activeTargets = plan.GetTargetResources();
            if(activeTargets.Count > 0) {
                chosenTarget = activeTargets.Min((ResourcePile resource) => resourceData[resource].Advantage);
                if(activeTargets.Count < 3 
                    && unassigned.FindAll((Monster monster) => monsterRoles[monster.Stats.Name] != TeamRole.Support).Count > 1
                    && (resourceData[chosenTarget].priority < 0.3f || resourceData[chosenTarget].Advantage > 0)
                ) {
                    chosenTarget = null; // choose to attack instead
                }
            }

            // choose a new resource to attack
            if(chosenTarget == null) {
                List<ResourcePile> attackOptions = GameManager.Instance.AllResources.FindAll((ResourcePile target) => !activeTargets.Contains(target));
                chosenTarget = attackOptions.Max((ResourcePile option) => resourceData[option].priority);
            }

            for(int i = unassigned.Count - 1; i >= 0; i--) {
                plan.Assign(unassigned[i], chosenTarget);
            }
        }

        // order abilities
        Dictionary<Monster, int> movePriorities = new Dictionary<Monster, int>();
        foreach(Monster teammate in controlTarget.Teammates) {
            // -2: targets an ally
            // -1 targets an ally and targeted by an ally
            // 0: no priority
            // 1: targeted by an ally
            movePriorities[teammate] = 0;
        }
        
        List<TurnOption> monsterPlans = controlTarget.Teammates.ConvertAll((Monster monster) => ChooseAction(monster));
        foreach(TurnOption plan in monsterPlans) {
            if(!plan.UsesAbility) {
                continue;
            }

            Move plannedMove = plan.user.Stats.Moves[plan.abilitySlot];
            if(plannedMove is StatusMove && (plannedMove as StatusMove).Condition.effect == StatusEffect.Haunt) {
                movePriorities[plan.user] = -2; // always haunt the enemy first
            }
            else if(plannedMove.TargetType == Move.Targets.Allies) {
                movePriorities[plan.user] = movePriorities[plan.user] == 1 || movePriorities[plan.user] == -1 ? -1 : -2;
                foreach(Vector2Int tile in plan.abilityTargets.Filtered) {
                    if(tile == plan.walkDestination) {
                        continue; // targeting yourself has no dependencies
                    }
                    Monster ally = LevelGrid.Instance.GetMonster(tile);
                    movePriorities[ally] = movePriorities[ally] == -2 || movePriorities[plan.user] == -1 ? -1 : 1;
                }
            }
        }

        // choose abilities
        List<Monster> orderedTeammates = new List<Monster>(controlTarget.Teammates);
        orderedTeammates.Sort((Monster cur, Monster next) => movePriorities[cur] - movePriorities[next]);
        foreach(Monster teammate in orderedTeammates) {
            TurnOption choice = ChooseAction(teammate);

            if(choice.ordering != TurnOption.MoveOrdering.None) {
                Vector2 focus = choice.user.transform.position;
                if(choice.UsesAbility) {
                    Vector2 targetPos = Global.DetermineCenter(choice.abilityTargets.Filtered);
                    focus = (focus + targetPos) / 2f;
                }
                AnimationsManager.Instance.QueueAnimation(new CameraAnimator(focus));
            }

            foreach(TurnOption.MoveOrdering action in choice.GetSequence()) {
                if(action == TurnOption.MoveOrdering.WalkOnly) {
                    choice.user.UseMove(MonsterType.WALK_INDEX, new Selection(choice.walkDestination));
                    AnimationsManager.Instance.QueueAnimation(new PauseAnimator(0.2f));
                }
                else if(action == TurnOption.MoveOrdering.AbilityOnly) {
                    choice.user.UseMove(choice.abilitySlot, choice.abilityTargets);
                    AnimationsManager.Instance.QueueAnimation(new PauseAnimator(0.5f));
                }
            }

            if(choice.user.Stats.Moves[choice.abilitySlot].Name == "Vine Grasp") {
                // since an enemy moved, paths must be updated
                FindWalkableTiles();
            }
        }

        AttemptCraft();

        // end turn after animations play out
        AnimationsManager.Instance.QueueFunction(() => { controlTarget.EndTurn(); });
    }

    // caches each tile each teammate can walk to this turn to avoid repeated pathfinding
    private void FindWalkableTiles() {
        pathedPositions.Clear();
        walkOptions.Clear();
        foreach(Monster monster in controlTarget.Teammates) {
            pathedPositions[monster] = monster.Tile;
            if(monster.WalkAvailable) {
                walkOptions[monster] = monster.GetMoveOptions(MonsterType.WALK_INDEX, false)
                    .ConvertAll((Selection tileContainter) => tileContainter.Unfiltered[0]);
            } else {
                walkOptions[monster] = new List<Vector2Int>();
            }
        }
    }

    // determines the best sequence of walk and ability to use this turn
    private TurnOption ChooseAction(Monster monster) {
        List<TurnOption> options = new List<TurnOption>();

        // consider if doing nothing is optimal
        options.Add(new TurnOption { 
            user = monster,
            ordering = TurnOption.MoveOrdering.None,
            abilityValue = 0f,
            positionValue = 0f
        });

        // find the best walk destination
        ResourcePile assignment = plan.GetAssignmentOf(monster);
        List<Vector2Int> walkableTiles = walkOptions[monster].FindAll((Vector2Int tile) => LevelGrid.Instance.IsOpenTile(tile));
        Vector2Int idealEndTile = walkableTiles.Count > 0 ? walkableTiles.Max((Vector2Int tile) => DeterminePositionValue(monster, tile, assignment)) : monster.Tile;
        float startPosValue = DeterminePositionValue(monster, monster.Tile, assignment);
        float idealEndValue = DeterminePositionValue(monster, idealEndTile, assignment);

        // consider simply walking
        if(walkableTiles.Count > 0) {
            options.Add(new TurnOption { 
                user = monster,
                ordering = TurnOption.MoveOrdering.WalkOnly,
                walkDestination = idealEndTile,
                abilityValue = 0f,
                positionValue = idealEndValue + 0.01f // prefer staying still over an equally valuable other tile
            });
        }

        // for each ability, find the best option
        for(int i = 0; i < monster.Stats.Moves.Length; i++) {
            if(i == MonsterType.WALK_INDEX || monster.Cooldowns[i] > 0) {
                continue;
            }

            // consider moving then using the ability
            if(walkableTiles.Count > 0) {
                TurnOption walkFirst = ChooseWalkedOption(monster, i, startPosValue, walkableTiles);
                if(walkFirst.ordering != TurnOption.MoveOrdering.None) {
                    options.Add(walkFirst);
                }
            }

            // consider using the ability then moving
            TurnOption abilityFirst = ChooseStillOption(monster, i);
            if(abilityFirst.ordering == TurnOption.MoveOrdering.None) {
                // there might be no targets
                continue;
            }

            if(walkableTiles.Count == 0 || monster.Stats.Moves[i].Type == MoveType.Shift) {
                // if a monster is shifted, pathfinding is inaccurate
                abilityFirst.ordering = TurnOption.MoveOrdering.AbilityOnly;
                abilityFirst.positionValue = startPosValue;
                options.Add(abilityFirst);
                continue;
            }

            abilityFirst.ordering = idealEndTile == monster.Tile ? TurnOption.MoveOrdering.AbilityOnly : TurnOption.MoveOrdering.AbilityThenWalk;
            abilityFirst.walkDestination = idealEndTile;
            abilityFirst.positionValue = idealEndValue;
            options.Add(abilityFirst);
        }
       
        return options.Max((TurnOption option) => option.Effectiveness);
    }

    // best way of using an ability without moving
    private TurnOption ChooseStillOption(Monster monster, int moveSlot) {
        List<Selection> targetGroups = monster.GetMoveOptions(moveSlot);
        if(targetGroups.Count == 0) {
            return new TurnOption {
                user = monster,
                ordering = TurnOption.MoveOrdering.None
            };
        }

        List<Selection> bestTargets = targetGroups.AllTiedMax((Selection targetGroup) => DetermineOptionValue(monster, moveSlot, targetGroup.Filtered, monster.Tile));
        Selection chosenTarget = bestTargets[UnityEngine.Random.Range(0, bestTargets.Count)];
        return new TurnOption {
            user = monster,
            ordering = TurnOption.MoveOrdering.AbilityOnly,
            abilitySlot = moveSlot,
            abilityTargets = chosenTarget,
            abilityValue = DetermineOptionValue(monster, moveSlot, chosenTarget.Filtered, monster.Tile)
        };
    }

    // best way of moving to another tile then using an ability
    private TurnOption ChooseWalkedOption(Monster monster, int moveSlot, float startTileValue, List<Vector2Int> walkableTiles) {
        Dictionary<Vector2Int, List<Selection>> positionTargetOptions = monster.GetMoveOptionsAfterWalk(moveSlot, false, walkableTiles);

        List<TurnOption> allOptions = new List<TurnOption>();
        foreach(KeyValuePair<Vector2Int, List<Selection>> positionTargets in positionTargetOptions) {
            Vector2Int walkDestination = positionTargets.Key;
            List<Selection> targetGroups = positionTargets.Value;

            float posVal = DeterminePositionValue(monster, walkDestination, plan.GetAssignmentOf(monster));
            foreach(Selection targetGroup in targetGroups) {
                if(targetGroup.Filtered.Count == 0) {
                    continue;
                }

                allOptions.Add(new TurnOption {
                    user = monster,
                    ordering = TurnOption.MoveOrdering.WalkThenAbility,
                    walkDestination = walkDestination,
                    abilitySlot = moveSlot,
                    abilityTargets = targetGroup,
                    abilityValue = DetermineOptionValue(monster, moveSlot, targetGroup.Filtered, walkDestination),
                    positionValue = posVal
                });
            }
        }

        if(allOptions.Count == 0) {
            return new TurnOption {
                user = monster,
                ordering = TurnOption.MoveOrdering.None
            };
        }

        return allOptions.Max((TurnOption option) => option.Effectiveness);
    }

    // returns a value that represents how valuable the usage of the input move on the input targets would be
    // userPosition is the tile that the user has theoretically moved to, which may be different from its current tile
    private float DetermineOptionValue(Monster user, int moveSlot, List<Vector2Int> targets, Vector2Int userPosition) {
        LevelGrid level = LevelGrid.Instance;
        Move move = user.Stats.Moves[moveSlot];

        if(move.Type == MoveType.Heal) {
            // prioritize healing lower health allies
            float value = -0.1f;
            foreach(Vector2Int tile in targets) {
                Monster ally = tile == userPosition ? user : level.GetMonster(tile);
                value += 0.7f * (1f - (float)ally.Health / ally.Stats.Health);
            }
            return value;
        }

        if(move is Attack) {
            int totalDamage = 0;
            foreach(Monster hit in targets.ConvertAll((Vector2Int tile) => level.GetMonster(tile))) {
                totalDamage += hit.DetermineDamage(((Attack)move).Damage, user);
            }
            return totalDamage / 10.0f;
        }

        if(move is StatusMove) {
            StatusAilment effect = ((StatusMove)move).Condition;
            Monster hit = targets[0] == userPosition ? user : level.GetMonster(targets[0]);
            if(hit.HasStatus(effect.effect)) {
                return -0.2f; // don't use a status on a monster that already has that status
            }

            if(targets.Count > 1) {
                return -0.1f + 0.3f * targets.Count;
            }

            if(move.TargetType == Move.Targets.Enemies) {
                if(effect.effect == StatusEffect.Fear && !(hit.Stats.Moves[MonsterType.PRIMARY_INDEX] is Attack)) {
                    return -1f; // dont weaken an enemy that has no attacks
                }

                return 0.8f * (hit.Health / (float)hit.Stats.Health);
            } else {
                if(effect.effect == StatusEffect.Power && !(hit.Stats.Moves[MonsterType.PRIMARY_INDEX] is Attack)) {
                    return -1f; // dont strengthen an ally that has no attacks
                }

                float value = -0.2f;
                foreach(Monster enemy in GameManager.Instance.OpponentOf(controlTarget).Teammates) {
                    value += 0.4f * Mathf.Max(0f, 1f - Global.CalcTileDistance(hit.Tile, enemy.Tile) / 6f);
                }
                return value;
            }
        }

        if(move is ZoneMove) {
            if(GameManager.Instance.OpponentOf(controlTarget).Teammates.Count == 0) {
                return -1f;
            }

            float value = -0.4f;
            foreach(Vector2Int tile in targets) {
                if(level.GetTile(tile).CurrentEffect != null) {
                    // discourage replacing an existing tile effect
                    value -= 0.2f;
                    continue;
                }

                float distance = Vector2.Distance(level.Tiles.GetCellCenterWorld((Vector3Int)tile), idealZoneMiddle);
                value += 0.6f * (1f - distance / 4f);
            }
            return value;
        }

        if(move.Type == MoveType.Shift) {
            switch(user.Stats.Name) {
                case MonsterName.Phantom:
                    // dash
                    Vector2Int dir = targets[0] - userPosition;
                    if(dir.x == 0) {
                        dir = new Vector2Int(0, dir.y > 0 ? 1 : -1);
                    } else {
                        dir = new Vector2Int(dir.x > 0 ? 1 : -1, 0);
                    }

                    float enemiesHit = 0;
                    for(Vector2Int tile = userPosition + dir; tile != targets[0]; tile += dir) {
                        Monster occupant = level.GetMonster(tile);
                        if(occupant != null && occupant.Controller != user.Controller) {
                            enemiesHit++;
                        }
                    }
                    
                    if(enemiesHit == 0) {
                        return -0.1f;
                    }
                    
                    return 0.4f * DeterminePositionValue(user, targets[0], plan.GetAssignmentOf(user)) + 0.6f * enemiesHit;

                case MonsterName.Flytrap:
                    // pull
                    return 0.3f + 0.1f * (Global.CalcTileDistance(userPosition, targets[0]) - 1);
            }
        }

        return 0.3f;
    }

    // returns a value that represents how well this position approaches the capture point
    private float DeterminePositionValue(Monster monster, Vector2Int position, ResourcePile goal) {
        float distanceValue = 0f;
        float captureBonus = 0f;

        if(monsterRoles[monster.Stats.Name] == TeamRole.Support) {
            // supporters should not advance on capture points. Instead they should move to the middle of their teammates attacking the same resource
            if(plan.GetAssignmentOf(monster).IsInCaptureRange(position)) {
                captureBonus = 0.5f;
            }
            
            int numAttackers = 0;
            Vector2 teamMiddle = Vector2.zero;
            List<Monster> comrades = plan.GetAssignedAt(plan.GetAssignmentOf(monster));
            foreach(Monster comrade in comrades) {
                if(monsterRoles[comrade.Stats.Name] != TeamRole.Support) {
                    numAttackers++;
                    teamMiddle += (Vector2)LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)comrade.Tile);
                }
            }

            if(numAttackers == 0) {
                return 0;
            }

            teamMiddle /= numAttackers;
            Vector2Int targetTile = (Vector2Int)LevelGrid.Instance.Tiles.WorldToCell(teamMiddle);
            if(!LevelGrid.Instance.GetTile(targetTile).Walkable) {
                // if the middle of the pack is an invalid tile, move towards a single ally
                targetTile = comrades.Find((Monster ally) => monsterRoles[ally.Stats.Name] != TeamRole.Support).Tile;
            }
            distanceValue = 1f - 0.02f * Monster.FindPath(position, targetTile).Count;
            return distanceValue + captureBonus;
        }

        if(goal.IsInCaptureRange(position)) {
            distanceValue = 1f;

            // slighlty value being towards where the opponent is likely to attack
            Vector2Int enemySpawn = GameManager.Instance.OpponentOf(controlTarget).Spawnpoint.Tile;
            int resourceDist = Global.CalcTileDistance(goal.Tile, enemySpawn);
            int testDist = Global.CalcTileDistance(position, enemySpawn);
            distanceValue += (resourceDist - testDist) / 200f;

            // weight this spot higher if this monster would be the only capturer
            int otherCapturers = controlTarget.Teammates.FindAll((Monster teammate) => teammate != monster && goal.IsInCaptureRange(teammate.Tile)).Count;
            captureBonus = otherCapturers == 0 ? 1f : 0.2f;
        } else {
            List<Vector2Int> path = Monster.FindPath(position, goal.Tile);
            int tilesOnPoint = 0;
            for(int i = path.Count - 1; i >= 0; i--) {
                if(goal.IsInCaptureRange(path[i])) {
                    tilesOnPoint++;
                } else {
                    break;
                }
            }

            int tilesFromPoint = path.Count - tilesOnPoint;
            distanceValue = 1f - tilesFromPoint * 0.02f;
        }

        TileAffector terrain = LevelGrid.Instance.GetTile(position).CurrentEffect;
        float terrainPenalty = 0f;
        if(terrain != null && terrain.Controller != controlTarget && terrain.HasNegativeEffect) {
            terrainPenalty = -0.1f;
        }

        return distanceValue + captureBonus + terrainPenalty;
    }

    private float CalculateInfluence(Monster monster, ResourcePile resource) {
        const int MAX_INFLUENCE_RANGE = 5;
        
        if(Global.CalcTileDistance(monster.Tile, resource.Tile) > MAX_INFLUENCE_RANGE + 2) { // influence range may be 2 greater than the actual distance
            return 0;
        }

        // find the distance to the capture zone
        int distance = Monster.FindPath(monster.Tile, resource.Tile).Count - 1;
        if(monster.Tile.x != resource.Tile.x && monster.Tile.y != resource.Tile.y) {
            // make corners worth the same as orthogonally adjacent
            distance--;
        }

        if(distance > MAX_INFLUENCE_RANGE) {
            return 0f;
        }

        float influence = (MAX_INFLUENCE_RANGE + 1 - distance) / (MAX_INFLUENCE_RANGE + 1f);
        return 0.5f + 0.5f * influence; // range 0.5-1
    }

    private Dictionary<ResourcePile, ResourceData> EvaluateResources() {
        Dictionary<ResourcePile, ResourceData> resourceData = new Dictionary<ResourcePile, ResourceData>();

        // count how many resources are needed to craft the remaining monster types
        float allyTotal = 0;
        float enemyTotal = 0;
        allyNeeds.Clear();
        Dictionary<Ingredient, int> enemyNeeds = new Dictionary<Ingredient, int>();
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            allyNeeds[ingredient] = 0;
            enemyNeeds[ingredient] = 0;
        }

        Team opponent = GameManager.Instance.OpponentOf(controlTarget);
        foreach(MonsterName type in Enum.GetValues(typeof(MonsterName))) {
            bool allyCrafted = controlTarget.CraftedMonsters[type];
            bool enemyCrafted = opponent.CraftedMonsters[type];
            if(allyCrafted && enemyCrafted) {
                continue;
            }

            foreach(Ingredient ingredient in MonstersData.Instance.GetMonsterData(type).Recipe) {
                if(!allyCrafted) {
                    allyNeeds[ingredient]++;
                    allyTotal++;
                }
                if(!enemyCrafted) {
                    enemyNeeds[ingredient]++;
                    enemyTotal++;
                }
            }
        }

        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            allyNeeds[ingredient] -= controlTarget.Resources[ingredient];
            enemyNeeds[ingredient] -= opponent.Resources[ingredient];
        }
        const float ENEMY_WEIGHT = 0.25f;

        // generate data for each resource
        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            ResourceData data = new ResourceData();

            // determine how much influence each team has on this resource
            foreach(Monster ally in controlTarget.Teammates) {
                data.controlValue += CalculateInfluence(ally, resource);
            }

            foreach(Monster enemy in GameManager.Instance.OpponentOf(controlTarget).Teammates) {
                data.threatValue += CalculateInfluence(enemy, resource);
            }

            // save the importance based on how much each team needs this resource
            data.priority = (1f - ENEMY_WEIGHT) * (allyNeeds[resource.Type] / allyTotal) + ENEMY_WEIGHT * (enemyNeeds[resource.Type] / enemyTotal);

            resourceData[resource] = data;
        }

        return resourceData;
    }

    private void AttemptCraft() {
        if(controlTarget.Spawnpoint.CookState != Cauldron.State.Ready) {
            return;
        }

        List<MonsterName> buyOptions = new List<MonsterName>();
        foreach(MonsterName monsterType in Enum.GetValues(typeof(MonsterName))) {
            if(controlTarget.CanAfford(monsterType)) {
                buyOptions.Add(monsterType);
            }
        }

        if(buyOptions.Count == 0) {
            return;
        }

        List<MonsterName> newCrafts = buyOptions.FindAll((MonsterName type) => !controlTarget.CraftedMonsters[type]);
        if(newCrafts.Count > 0) {
            buyOptions = newCrafts;
        } else {
            Dictionary<Ingredient, int> surplus = new Dictionary<Ingredient, int>();
            int totalSurplus = 0;
            foreach(Ingredient resource in Enum.GetValues(typeof(Ingredient))) {
                surplus[resource] = Mathf.Max(0, controlTarget.Resources[resource] - allyNeeds[resource]);
                totalSurplus += surplus[resource];
            }

            if(totalSurplus >= 3) {
                // find all monsters that can be crafted with the extra resources
                buyOptions = buyOptions.FindAll((MonsterName monster) => { 
                    List<Ingredient> recipe = MonstersData.Instance.GetMonsterData(monster).Recipe;
                    foreach(Ingredient resource in Enum.GetValues(typeof(Ingredient))) {
                        if(surplus[resource] < recipe.FindAll((Ingredient cost) => cost == resource).Count) {
                            return false;
                        }
                    }
                    return true;
                });

                if(buyOptions.Count == 0) {
                    return;
                }
            }
            else if(controlTarget.Teammates.Count > GameManager.Instance.OpponentOf(controlTarget).Teammates.Count) {
                // only spend needed resources on a duplicate if falling behind the opponent
                return;
            }
        }

        controlTarget.BuyMonster(buyOptions[UnityEngine.Random.Range(0, buyOptions.Count)]);
    }

    private struct ResourceData {
        public float priority;
        public float threatValue; // enemy influence
        public float controlValue; // ally influence
        public float Advantage { get { return controlValue - threatValue; } }
    }

    private struct TurnOption {
        public enum MoveOrdering {
            None,
            WalkOnly,
            AbilityOnly,
            WalkThenAbility,
            AbilityThenWalk
        }

        public Monster user;
        public MoveOrdering ordering;
        public Vector2Int walkDestination;
        public int abilitySlot;
        public Selection abilityTargets;
        public float abilityValue; // how valuable the used ability with the given targets is
        public float positionValue; // how close the end position is to the target position

        public float Effectiveness { get { return abilityValue + positionValue; } }

        private static List<MoveOrdering> abilityOrders = new List<MoveOrdering> { MoveOrdering.AbilityOnly, MoveOrdering.WalkThenAbility, MoveOrdering.AbilityThenWalk };
        public bool UsesAbility { get { return abilityOrders.Contains(ordering); } }

        public MoveOrdering[] GetSequence() {
            switch(ordering) {
                default:
                case MoveOrdering.None:
                    return new MoveOrdering[0];
                case MoveOrdering.WalkOnly:
                    return new MoveOrdering[] { MoveOrdering.WalkOnly };
                case MoveOrdering.AbilityOnly:
                    return new MoveOrdering[] { MoveOrdering.AbilityOnly }; ;
                case MoveOrdering.WalkThenAbility:
                    return new MoveOrdering[] { MoveOrdering.WalkOnly, MoveOrdering.AbilityOnly }; ;
                case MoveOrdering.AbilityThenWalk:
                    return new MoveOrdering[] { MoveOrdering.AbilityOnly, MoveOrdering.WalkOnly }; ;
            }
        }
    }
}

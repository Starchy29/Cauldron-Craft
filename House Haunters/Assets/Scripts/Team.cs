using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// represents one team of monsters
public class Team
{
    public Color TeamColor { get; private set; }
    public List<Monster> Teammates { get; private set; }
    public Dictionary<Ingredient, int> Resources { get; private set; }
    public AIController AI { get; private set; }
    public Cauldron Spawnpoint { get; set; }
    public ResourceTracker ResourceDisplay { get; set; }
    public Dictionary<MonsterName, bool> CraftedMonsters { get; private set; }
    public bool OnLeft { get { return Spawnpoint != null && Spawnpoint.Tile.x < LevelGrid.Instance.Width / 2; } }

    public event Trigger OnTurnEnd;
    public event Trigger OnTurnStart;

    public Team(Color color, bool isAI, Ingredient? startBatch = null) {
        Array monsterTypes = Enum.GetValues(typeof(MonsterName));
        CraftedMonsters = new Dictionary<MonsterName, bool>(monsterTypes.Length);
        foreach(MonsterName monster in monsterTypes) {
            CraftedMonsters[monster] = false;
        }

        TeamColor = color;
        Teammates = new List<Monster>();
        Resources = new Dictionary<Ingredient, int>(Enum.GetValues(typeof(Ingredient)).Length);
        foreach(Ingredient type in Enum.GetValues(typeof(Ingredient))) {
            Resources[type] = 0;
        }
        if(startBatch.HasValue) {
            Resources[startBatch.Value] = 3;
        }

        if(isAI) {
            AI = new AIController(this);
        }
    }

    public void AddResource(Ingredient type) {
        Resources[type]++;
        ResourceDisplay.UpdateDisplay();
    }

    public bool CanCraft() {
        if(Spawnpoint.CookState != Cauldron.State.Ready) {
            return false;
        }

        int totalIngredients = 0;
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            totalIngredients += Resources[ingredient];
        }

        return totalIngredients >= 3;
    }

    public bool CanBuy(MonsterName monsterType) {
        MonsterType monster = MonstersData.Instance.GetMonsterData(monsterType);

        Dictionary<Ingredient, int> requirements = new Dictionary<Ingredient, int>();
        foreach(Ingredient ingredient in monster.Recipe) {
            if(!requirements.ContainsKey(ingredient)) {
                requirements[ingredient] = 0;
            }
            requirements[ingredient]++;
        }

        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            if(requirements.ContainsKey(ingredient) && Resources[ingredient] < requirements[ingredient]) {
                return false;
            }
        }

        return true;
    }

    public void BuyMonster(MonsterName type) {
        if(!CanBuy(type)) {
            return;
        }

        MonsterType data = MonstersData.Instance.GetMonsterData(type);
        AnimationsManager animator = AnimationsManager.Instance;
        foreach(Ingredient ingredient in data.Recipe) {
            Resources[ingredient]--;
        }
        animator.QueueAnimation(new IngredientAnimator(Spawnpoint, data.Recipe));
        ResourceDisplay.UpdateDisplay();

        Spawnpoint.StartCook(type);
        CraftedMonsters[type] = true;

        // check for victory
        foreach(MonsterName monster in Enum.GetValues(typeof(MonsterName))) {
            if(!CraftedMonsters[monster]) {
                return;
            }
        }
        GameOverviewDisplayer.Instance.ShowWinner(this);
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(99999f));
    }
    
    public void Join(Monster monster) {
        Teammates.Add(monster);
        monster.Controller = this;
    }

    public void Remove(Monster monster) {
        Teammates.Remove(monster);
        monster.Controller = null;
    }

    public void StartTurn() {
        foreach(Monster teammate in Teammates) {
            teammate.StartTurn();
        }

        Spawnpoint.StartTurn();
        OnTurnStart?.Invoke();

        if(AI == null) {
            MenuManager.Instance.StartPlayerTurn(this);
        } else {
            AI.PlanTurn();
        }
    }

    public void EndTurn() {
        if(GameManager.Instance.CurrentTurn != this) {
            return;
        }

        for(int i = Teammates.Count - 1; i >= 0; i--) { // loop backwards because ending turn could kill the monster and modify the collection
            Teammates[i].EndTurn();
        }

        OnTurnEnd?.Invoke();

        GameManager.Instance.PassTurn(this);
    }
}

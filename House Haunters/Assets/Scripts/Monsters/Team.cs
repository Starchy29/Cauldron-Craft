using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// represents one team of monsters
public class Team
{
    public String Name { get; private set; }
    public Color TeamColor { get; private set; }
    public List<Monster> Teammates { get; private set; }
    public Dictionary<Ingredient, int> Resources { get; private set; }
    public AIController AI { get; private set; }
    public Cauldron Spawnpoint { get; set; }
    public bool OnLeft { get { return Spawnpoint != null && Spawnpoint.Tile.x < LevelGrid.Instance.Width / 2; } }
    public int TotalIngredients { get {
        int totalIngredients = 0;
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            totalIngredients += Resources[ingredient];
        }
        return totalIngredients;
    } }

    public Dictionary<MonsterName, bool> CraftedMonsters { get; private set; }
    private int totalCrafted;
    private static int CRAFT_GOAL = Enum.GetValues(typeof(MonsterName)).Length;

    public event Trigger OnTurnEnd;
    public event Trigger OnTurnStart;

    public Team(String name, Color color, bool isAI) {
        Name = name;
        TeamColor = color;
        Teammates = new List<Monster>();
        Resources = new Dictionary<Ingredient, int>(Enum.GetValues(typeof(Ingredient)).Length);

        CraftedMonsters = new Dictionary<MonsterName, bool>();
        foreach(MonsterName monster in Enum.GetValues(typeof(MonsterName))) {
            CraftedMonsters[monster] = false;
        }

        foreach(Ingredient type in Enum.GetValues(typeof(Ingredient))) {
            Resources[type] = 3;
        }

        if(isAI) {
            AI = new AIController(this);
        }
    }

    public bool CanCraft() {
        return Spawnpoint.CookState == Cauldron.State.Ready && TotalIngredients >= 3;
    }

    public bool CanAfford(MonsterName monsterType) {
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
        if(!CanAfford(type)) {
            return;
        }

        MonsterType data = MonstersData.Instance.GetMonsterData(type);
        AnimationsManager animator = AnimationsManager.Instance;
        foreach(Ingredient ingredient in data.Recipe) {
            Resources[ingredient]--;
        }
        animator.QueueAnimation(new IngredientAnimator(Spawnpoint, data.Recipe));

        Spawnpoint.StartCook(type);

        // check for victory
        if(!CraftedMonsters[type]) {
            CraftedMonsters[type] = true;
            totalCrafted++;
            if(totalCrafted >= CRAFT_GOAL) {
                AnimationsManager.Instance.QueueAnimation(new PauseAnimator(1f));
                Spawnpoint.FinishCook();
                GameOverviewDisplayer.Instance.ShowWinner(this);
            }
        }
    }
    
    public void Join(Monster monster) {
        Teammates.Add(monster);
        monster.Controller = this;
    }

    public void StartTurn() {
        foreach(Monster teammate in Teammates) {
            teammate.StartTurn();
        }

        OnTurnStart?.Invoke();

        if(AI == null) {
            MenuManager.Instance.StartPlayerTurn(this);
        } else {
            AI.TakeTurn();
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

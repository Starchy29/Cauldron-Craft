using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public struct TeamPreset {
    public string name;
    public Color teamColor;
    public Ingredient startResource;
    public MonsterName[] teamComp;

    public static bool operator ==(TeamPreset self, TeamPreset other) {
        return self.startResource == other.startResource;
    }

    public static bool operator !=(TeamPreset self, TeamPreset other) {
        return self.startResource != other.startResource;
    }
}

// represents one team of monsters
public class Team
{
    public static TeamPreset Alchemists = new TeamPreset {
        name = "Alchemists",
        teamColor = new Color(0.1f, 0.5f, 0.9f),
        startResource = Ingredient.Mineral,
        teamComp = new MonsterName[3] { MonsterName.Sludge, MonsterName.Amalgamation, MonsterName.Golem }
    };

    public static TeamPreset Witchcrafters = new TeamPreset {
        name = "Witchcrafters",
        teamColor = new Color(0.5f, 0.8f, 0.1f),
        startResource = Ingredient.Flora,
        teamComp = new MonsterName[3] { MonsterName.Flytrap,  MonsterName.Beast, MonsterName.LostSoul }
    };

    public static TeamPreset Occultists = new TeamPreset {
        name = "Occultists",
        teamColor = new Color(0.9f, 0.3f, 0.1f),
        startResource = Ingredient.Decay,
        teamComp = new MonsterName[3] { MonsterName.Demon, MonsterName.Fossil, MonsterName.Jackolantern }
    };

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
    public int TotalCrafted { get; private set; }
    private static int CRAFT_GOAL = Enum.GetValues(typeof(MonsterName)).Length;

    private MonsterName[] startTeam;

    public event Trigger OnTurnEnd;
    public event Trigger OnTurnStart;

    public Team(TeamPreset preset, bool isAI) {
        Name = preset.name;
        TeamColor = preset.teamColor;
        startTeam = preset.teamComp;
        Teammates = new List<Monster>();
        Resources = new Dictionary<Ingredient, int>(Enum.GetValues(typeof(Ingredient)).Length);

        CraftedMonsters = new Dictionary<MonsterName, bool>();
        foreach(MonsterName monster in Enum.GetValues(typeof(MonsterName))) {
            CraftedMonsters[monster] = false;
        }

        foreach(Ingredient type in Enum.GetValues(typeof(Ingredient))) {
            Resources[type] = 0;
        }
        Resources[preset.startResource] = 2;

        if(isAI) {
            AI = new AIController(this);
        }
    }

    struct TileWithDistance {
        public Vector2Int tile;
        public int distance;
    }

    public void SpawnStartTeam() {
        AnimationsManager animator = AnimationsManager.Instance;
        LevelGrid level = LevelGrid.Instance;

        animator.QueueAnimation(new CameraAnimator(Spawnpoint.transform.position));
        GameOverviewDisplayer.Instance.ShowTurnStart(this);
        foreach(MonsterName starter in startTeam) {
            animator.QueueAnimation(new IngredientAnimator(Spawnpoint, MonstersData.Instance.GetMonsterData(starter).Recipe));
            Spawnpoint.StartCook(starter);
            CraftedMonsters[starter] = true;
            TotalCrafted++;
            animator.QueueAnimation(new PauseAnimator(1f));
            Spawnpoint.FinishCook(true);
        }

        if(AI != null) {
            AI.ChooseStartPlan();
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
        animator.QueueAnimation(new CameraAnimator(Spawnpoint.transform.position));
        foreach(Ingredient ingredient in data.Recipe) {
            Resources[ingredient]--;
        }
        animator.QueueAnimation(new IngredientAnimator(Spawnpoint, data.Recipe));

        Spawnpoint.StartCook(type);
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(1f));

        // check for victory
        if(!CraftedMonsters[type]) {
            CraftedMonsters[type] = true;
            TotalCrafted++;
            if(TotalCrafted >= CRAFT_GOAL) {
                AnimationsManager.Instance.QueueFunction(() => { SoundManager.Instance.StopSong(false); });
                AnimationsManager.Instance.QueueAnimation(new PauseAnimator(1f));
                Spawnpoint.FinishCook();
                AnimationsManager.Instance.QueueAnimation(new PauseAnimator(1f));
                GameOverviewDisplayer.Instance.ShowWinner(this);
            }
        }
    }
    
    public void Join(Monster monster) {
        Teammates.Add(monster);
        monster.Controller = this;
        if(AI != null) {
            AI.AddMonster(monster);
        }
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

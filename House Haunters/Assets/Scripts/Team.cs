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
    public bool IsAI { get; private set; }
    public Cauldron Spawnpoint { get; set; }

    public event Trigger OnTurnEnd;
    public event Trigger OnTurnStart;

    public Team(Color color, bool isAI) {
        TeamColor = color;
        IsAI = isAI;
        Teammates = new List<Monster>();
        Resources = new Dictionary<Ingredient, int>(Enum.GetValues(typeof(Ingredient)).Length);
        foreach(Ingredient type in Enum.GetValues(typeof(Ingredient))) {
            Resources[type] = 0;
        }
    }

    public void AddResource(Ingredient type) {
        Resources[type]++;
    }

    public bool CanBuy(MonsterName monsterType) {
        MonsterType monster = MonstersData.Instance.GetMonsterData(monsterType);
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            if(Resources[ingredient] < monster.Recipe[ingredient]) {
                return false;
            }
        }

        return true;
    }

    public void BuyMonster(MonsterName type) {
        if(!CanBuy(type)) {
            return;
        }

        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            MonsterType data = MonstersData.Instance.GetMonsterData(type);
            Resources[ingredient] -= data.Recipe[ingredient];
        }

        Spawnpoint.StartCook(type);
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

        OnTurnStart?.Invoke();
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

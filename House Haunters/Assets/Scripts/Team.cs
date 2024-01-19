using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// represents one team of monsters
public class Team
{
    public List<Monster> Teammates { get; private set; }
    public Dictionary<Ingredient, int> Resources { get; private set; }

    public Team() {
        Teammates = new List<Monster>();
        Resources = new Dictionary<Ingredient, int>(Enum.GetValues(typeof(Ingredient)).Length);
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
            teammate.OnTurnStart();
        }
    }

    public void EndTurn() {
        if(GameManager.Instance.CurrentTurn != this) {
            return;
        }

        foreach(Monster teammate in Teammates) {
            teammate.OnTurnEnd();
        }

        GameManager.Instance.PassTurn(this);
    }
}

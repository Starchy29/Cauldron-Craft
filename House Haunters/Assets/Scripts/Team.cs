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
    public event Trigger OnTurnEnd;
    public event Trigger OnTurnStart;

    public Team(Color color) {
        TeamColor = color;
        Teammates = new List<Monster>();
        Resources = new Dictionary<Ingredient, int>(Enum.GetValues(typeof(Ingredient)).Length);
        foreach(Ingredient type in Enum.GetValues(typeof(Ingredient))) {
            Resources[type] = 0;
        }
    }

    public void AddResource(Ingredient type) {
        Resources[type]++;
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

        foreach(Monster teammate in Teammates) {
            teammate.EndTurn();
        }

        OnTurnEnd?.Invoke();

        GameManager.Instance.PassTurn(this);
    }
}

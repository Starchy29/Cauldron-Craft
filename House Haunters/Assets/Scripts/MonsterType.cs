using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Ingredient {
    Decay,
    Plant,
    Mineral,
    Crawler
}

public class MonsterType
{
    private Ingredient[] recipe;
    public int Health { get; private set; }
    public int MoveLength { get; private set; }

    public MonsterType(Ingredient ingredient1, Ingredient ingredient2, Ingredient ingredient3, int health, int moveLength) {
        recipe = new Ingredient[3] { ingredient1, ingredient2, ingredient3 };
        Health = health;
        MoveLength = moveLength;
    }
}

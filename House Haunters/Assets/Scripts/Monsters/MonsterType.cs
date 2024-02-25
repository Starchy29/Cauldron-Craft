using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterType
{
    private Ingredient[] recipe;

    public int Health { get; private set; }
    public int Speed { get; private set; }
    public Move[] Moves { get; private set; }
    public bool Flying { get; private set; }

    public MonsterType(Ingredient ingredient1, Ingredient ingredient2, Ingredient ingredient3, int health, int speed, List<Move> specialMoves) {
        recipe = new Ingredient[3] { ingredient1, ingredient2, ingredient3 };
        Health = health;
        Speed = speed;

        Moves = new Move[specialMoves.Count + 1];
        Moves[0] = new MovementAbility();
        for(int i = 1; i < Moves.Length; i++) {
            Moves[i] = specialMoves[i-1];
        }
    }
}

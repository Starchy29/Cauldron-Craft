using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterType
{
    public Dictionary<Ingredient, int> Recipe { get; private set; }
    public int Health { get; private set; }
    public int Speed { get; private set; }
    public Move[] Moves { get; private set; }
    public bool Flying { get; private set; }

    public MonsterType(Ingredient ingredient1, Ingredient ingredient2, Ingredient ingredient3, int health, int speed, List<Move> specialMoves) {
        Recipe = new Dictionary<Ingredient, int>() {
            { Ingredient.Decay, 0 },
            { Ingredient.Flora, 0 },
            { Ingredient.Swarm, 0 },
            { Ingredient.Mineral, 0 }
        };

        Recipe[ingredient1]++;
        Recipe[ingredient2]++;
        Recipe[ingredient3]++;

        Health = health;
        Speed = speed;

        Moves = new Move[specialMoves.Count + 1];
        Moves[0] = new MovementAbility();
        for(int i = 1; i < Moves.Length; i++) {
            Moves[i] = specialMoves[i-1];
        }
    }
}

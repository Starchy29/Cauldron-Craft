using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterType
{
    public const int WALK_INDEX = 0;
    public const int PRIMARY_INDEX = 2;

    public List<Ingredient> Recipe { get; private set; }
    public int Health { get; private set; }
    public int Speed { get; private set; }
    public Move[] Moves { get; private set; }

    private static Dictionary<int, string> speedToName = new Dictionary<int, string> {
        { 2, "Shift" },
        { 3, "Crawl" },
        { 4, "Walk" },
        { 5, "Sprint" }
    };

    public MonsterType(List<Ingredient> recipe, int health, int speed, Move primary, Move special) {
        Recipe = recipe;

        Health = health;
        Speed = speed;

        Moves = new Move[3];
        Moves[0] = new MovementAbility(speedToName[speed]);
        Moves[1] = special;
        Moves[2] = primary;
    }
}

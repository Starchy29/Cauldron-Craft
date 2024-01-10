using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// represents one team of monsters
public class Team
{
    private List<Monster> teammates;
    
    public void Join(Monster monster) {
        teammates.Add(monster);
        monster.Controller = this;
    }

    public void Remove(Monster monster) {
        teammates.Remove(monster);
        monster.Controller = null;
    }
}

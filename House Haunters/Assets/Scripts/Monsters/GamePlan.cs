using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// helper container for AI decision-making
public struct GamePlan
{
    private Dictionary<Monster, ResourcePile> monsterAssignments;
    private Dictionary<ResourcePile, List<Monster>> assignedAllies;
    private List<Monster> unassigned;

    public GamePlan(List<ResourcePile> allResources) {
        assignedAllies = new Dictionary<ResourcePile, List<Monster>>();
        foreach(ResourcePile resource in allResources) {
            assignedAllies[resource] = new List<Monster>();
        }

        monsterAssignments = new Dictionary<Monster, ResourcePile>();
        unassigned = new List<Monster>();
    }

    public void Assign(Monster ally, ResourcePile resource) {
        if(unassigned.Contains(ally)) {
            unassigned.Remove(ally);
        }
        else if(monsterAssignments.ContainsKey(ally) && monsterAssignments[ally] != null) {
            assignedAllies[monsterAssignments[ally]].Remove(ally);
        }

        monsterAssignments[ally] = resource;
        if(resource == null) {
            unassigned.Add(ally);
        } else {
            assignedAllies[resource].Add(ally);
        }
    }

    public void Remove(Monster ally) {
        if(monsterAssignments[ally] == null) {
            unassigned.Remove(ally);
        } else {
            assignedAllies[monsterAssignments[ally]].Remove(ally);
        }
        monsterAssignments.Remove(ally);
    }

    public List<Monster> GetAssignedAt(ResourcePile resource) {
        return assignedAllies[resource];
    }

    public ResourcePile GetAssignmentOf(Monster ally) {
        return monsterAssignments[ally];
    }

    public List<Monster> GetUnassigned() {
        return unassigned;
    }
}

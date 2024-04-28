using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Ingredient
{
    Decay,
    Flora,
    Mineral,
    Swarm
}

public class ResourcePile : Capturable
{
    [SerializeField] private Ingredient type;
    public Ingredient Type { get { return type; } }

    protected override void Start() {
        base.Start();
        GameManager.Instance.OnTurnEnd += GrantResource;
        GameManager.Instance.AllResources.Add(this);
    }

    private void GrantResource(Team turnEnder, Team nextTurn) {
        // give a resource to the controller
        if(turnEnder == Controller) {
            Controller.AddResource(type);
        }
    }
}

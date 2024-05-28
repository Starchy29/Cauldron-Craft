using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusIcon : MonoBehaviour
{
    public string statusName;
    public string description;
    public int duration;

    public StatusIcon SetData(string name, string description, Sprite icon) {
        GetComponent<SpriteRenderer>().sprite = icon;
        statusName = name;
        this.description = description;
        return this;
    }
}

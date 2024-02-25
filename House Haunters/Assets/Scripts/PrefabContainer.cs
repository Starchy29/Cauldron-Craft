using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabContainer : MonoBehaviour
{
    public static PrefabContainer Instance { get; private set; }

    void Awake() {
        Instance = this;
    }

    public Sprite soulSprite;
    public Sprite demonSprite;
    public Sprite tempMonsterSprite;

    public GameObject BaseMonsterPrefab;
    public GameObject TempMonsterProjectile;
    public GameObject ExampleZone;
    public GameObject ExampleShield;
}

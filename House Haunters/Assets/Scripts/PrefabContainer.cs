using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabContainer : MonoBehaviour
{
    public static PrefabContainer Instance { get; private set; }

    void Awake() {
        Instance = this;
    }

    public GameObject TempMonsterPrefab;
    public GameObject TempMonsterProjectile;
}

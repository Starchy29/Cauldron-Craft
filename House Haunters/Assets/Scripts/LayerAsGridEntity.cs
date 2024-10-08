using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerAsGridEntity : MonoBehaviour
{
    void Start() {
        GetComponent<SpriteRenderer>().sortingOrder = (int)(-100 * transform.position.y);
    }
}

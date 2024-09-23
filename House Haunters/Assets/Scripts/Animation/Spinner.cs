using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private float degreesPerSec;
    private float rotation;
    
    void Update()
    {
        rotation += Time.deltaTime * degreesPerSec;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }
}

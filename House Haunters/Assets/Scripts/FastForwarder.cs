using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastForwarder : MonoBehaviour
{
    private GameObject container;

    // Start is called before the first frame update
    void Start()
    {
        container = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        container.SetActive(AnimationsManager.Instance.Animating && InputManager.Instance.SkipHeld() && !MenuManager.Instance.Paused);
    }
}

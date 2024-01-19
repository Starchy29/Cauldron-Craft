using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    private List<Button> buttons;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < transform.childCount; i++) {
            Button button = transform.GetChild(i).GetComponent<Button>();
            if(button != null) {
                buttons.Add(button);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

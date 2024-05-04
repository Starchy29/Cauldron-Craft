using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class DisappearanceAnimator : IMoveAnimator
{
    public bool Completed { get; set; }

    private GameObject disappearer;
    private float delay;

    public DisappearanceAnimator(GameObject disappearer, float delay = 0f) {
        this.disappearer = disappearer;
        this.delay = delay;
    }

    public void Start() {
        
    }

    public void Update(float deltaTime) {
        delay -= Time.deltaTime;
        if(delay <= 0f) {
            Completed = true;
            disappearer.SetActive(false);
        }
    }
}

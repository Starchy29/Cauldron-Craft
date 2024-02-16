using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    [SerializeField] private Texture2D[] sprites;
    [SerializeField] private AnimationMode mode;
    [SerializeField] private int framesPerSprite;

    private SpriteAnimation spriteAnimation;

    void Start() {
        spriteAnimation = new SpriteAnimation(sprites, mode, framesPerSprite);
    }

    void Update() {
        spriteAnimation.Update(Time.deltaTime);
        if(spriteAnimation.Finished) {
            Destroy(gameObject);
        }
    }
}

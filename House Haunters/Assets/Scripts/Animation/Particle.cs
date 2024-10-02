using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private AnimationMode mode;
    [SerializeField] private int framesPerSprite;
    [SerializeField] private bool destroyAtEnd;
    private SpriteRenderer renderer;

    private SpriteAnimation spriteAnimation;

    void Start() {
        if(sprites.Length > 0) {
            spriteAnimation = new SpriteAnimation(sprites, mode, framesPerSprite);
        }
        renderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if(spriteAnimation == null) {
            return;
        }

        spriteAnimation.Update(Time.deltaTime);
        renderer.sprite = spriteAnimation.CurrentSprite;
        if(spriteAnimation.Finished && destroyAtEnd) {
            Destroy(gameObject);
        }
    }
}

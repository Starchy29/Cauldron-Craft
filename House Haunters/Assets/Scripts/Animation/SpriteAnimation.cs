using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationMode {
    Forward,
    Rebound,
    Loop,
    Oscillate
}

public class SpriteAnimation
{
    private Texture2D[] sprites;
    private int framesPerSprite;
    private AnimationMode mode;

    private int spriteIndex;
    private float timer;
    private bool reversed;

    public Texture2D CurrentSprite { get { return sprites[spriteIndex]; } }
    public bool Finished { get; private set; }
    
    public SpriteAnimation(Texture2D[] sprites, AnimationMode mode, int framesPerSprite) {
        this.sprites = sprites;
        this.mode = mode;
        this.framesPerSprite = framesPerSprite;
    }

    public void Update(float deltaTime) {
        timer -= deltaTime;
        if(timer <= 0) {
            timer += 1f/60f * framesPerSprite;

            spriteIndex += reversed ? -1 : 1;

            if(spriteIndex < 0) {
                switch(mode) {
                    case AnimationMode.Rebound:
                        Finished = true;
                        spriteIndex = 0;
                        break;

                    case AnimationMode.Oscillate:
                        reversed = false;
                        spriteIndex = 1;
                        break;
                }
            }
            else if(spriteIndex >= sprites.Length) {
                switch(mode) {
                    case AnimationMode.Forward:
                        Finished = true;
                        spriteIndex = sprites.Length - 1;
                        break;

                    case AnimationMode.Loop:
                        spriteIndex = 0;
                        break;

                    case AnimationMode.Rebound:
                    case AnimationMode.Oscillate:
                        spriteIndex = sprites.Length - 2;
                        reversed = true;
                        break;
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtParticle : MonoBehaviour {
    [SerializeField] private Sprite testSprite;
    private void Start()
    {
        Randomize(testSprite);
    }

    private const int WIDTH = 16;

    public void Randomize(Sprite monsterImage) {
        Texture2D referenceImage = monsterImage.texture;
        Texture2D texture = new Texture2D(WIDTH, WIDTH);
        Vector2 referenceMid = new Vector2(referenceImage.width / 2f, referenceImage.height / 2f);
        Vector2 chunkMid = new Vector2(WIDTH / 2f, WIDTH / 2f);
        for(int x = 0; x < WIDTH; x++) {
            for(int y = 0; y < WIDTH; y++) {
                texture.SetPixel(x, y, Color.red);
                continue;

                Vector2 pixelVec = new Vector2(x, y);
                float distFromCenter = Vector2.Distance(chunkMid, pixelVec);
                if(distFromCenter > WIDTH / 2f) {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }
                
                Color chosenHue = referenceImage.GetPixel((int)referenceMid.x - 8 + x, (int)referenceMid.y - 8 + y);
                texture.SetPixel(x, y, new Color(pixelVec.x / WIDTH, pixelVec.y / WIDTH, 0, 1));
            }
        }

        texture.filterMode = FilterMode.Point;

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, WIDTH, WIDTH), new Vector2(0.5f, 0.5f), WIDTH);
        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = "VFX";
        renderer.sharedMaterial = new Material(Shader.Find("Unlit/SimpleShader"));
    }
}

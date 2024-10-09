using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtParticle : MonoBehaviour {
    private const int WIDTH = 7;

    public void Randomize(Sprite monsterImage) {
        Texture2D referenceImage = monsterImage.texture;
        Texture2D texture = new Texture2D(WIDTH, WIDTH);
        Vector2 referenceMid = new Vector2(referenceImage.width / 2f, referenceImage.height / 2f);
        Vector2Int chunkMid = new Vector2Int(WIDTH / 2, WIDTH / 2);
        for(int x = 0; x < WIDTH; x++) {
            for(int y = 0; y < WIDTH; y++) {
                int distFromCenter = Mathf.Abs(x - chunkMid.x) + Mathf.Abs(y - chunkMid.y);
                if(distFromCenter > 4 || distFromCenter == 4 && Random.value < 0.5f) {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }
                
                Vector2 fromMiddle = new Vector2(Random.value / 2f, Random.value / 2f);
                fromMiddle *= fromMiddle;
                fromMiddle *= new Vector2(referenceImage.width - 7, referenceImage.width - 3); 
                Vector2 chosenPixel = referenceMid + fromMiddle;
                Color chosenHue = referenceImage.GetPixel((int)chosenPixel.x, (int)chosenPixel.y);
                texture.SetPixel(x, y, chosenHue);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, WIDTH, WIDTH), new Vector2(0.5f, 0.5f), WIDTH + 1);
        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = "VFX";
        renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
    }
}

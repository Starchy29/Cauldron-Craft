using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Unity.Collections;

// attach to an empty gameobject, then set the radius to the desired amount in range 0-1
public class CaptureVFX : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start() {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = "VFX";
        spriteRenderer.sharedMaterial = new Material(Shader.Find("Unlit/WaveParticleShader"));
        SetRadius(0f);
    }

    public void SetColor(Color color) {
        spriteRenderer.color = color;
    }

    public void SetRadius(float radius) {
        // See WaveParticleShader, uses TEXCOORD1 for the radius value
        Vector2[] fakeUVs = new Vector2[4] {
            new Vector2(radius, 0f),
            new Vector2(radius, 0f),
            new Vector2(radius, 0f),
            new Vector2(radius, 0f)
        };
        SpriteDataAccessExtensions.SetVertexAttribute(spriteRenderer.sprite, UnityEngine.Rendering.VertexAttribute.TexCoord1,
            new NativeArray<Vector2>(fakeUVs, Allocator.Temp));
    }
}

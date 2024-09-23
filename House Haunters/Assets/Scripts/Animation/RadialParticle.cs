using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Unity.Collections;

// attach to an empty gameobject, then set the radius to the desired amount in range 0-1
public class RadialParticle : MonoBehaviour
{
    public enum Type {
        Controlled,
        Rebound,
        Linear
    }

    [SerializeField] private string ShaderName;
    [SerializeField] private Type type;
    [SerializeField] private float duration;

    private float t;

    private SpriteRenderer spriteRenderer;

    void Start() {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

        if(spriteRenderer == null) {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = "VFX";
        spriteRenderer.sharedMaterial = new Material(Shader.Find("Unlit/" + ShaderName));

        if(type == Type.Rebound) {
            transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);
        }

        SetRadius(0f);
    }

    private void Update() {
        switch(type) {
            case Type.Rebound:
                t += Time.deltaTime / duration;
                if(t > 1f) {
                    Destroy(gameObject);
                    return;
                }
                
                SetRadius(0.5f - Mathf.Abs(t * t - 0.5f));
                break;
        }
    }

    public void SetColor(Color color) {
        if(spriteRenderer == null) {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Unity.Collections;

// attach to an empty gameobject, then set the radius to the desired amount in range 0-1
public class CaptureVFX : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    //GraphicsBuffer constBuffer;

    void Start() {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = "VFX";
        spriteRenderer.sharedMaterial = new Material(Shader.Find("Unlit/WaveParticleShader"));

        //constBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Constant, 4, sizeof(float));
        //constBuffer.SetData(new float[4] { 1f, 0f, 0f, 1f });
        //spriteRenderer.sharedMaterial.SetConstantBuffer("radius", constBuffer, 0, 4*sizeof(float));
        //constBuffer.Release();

        //spriteRenderer.sharedMaterial.SetBuffer
        SetRadius(0f);
    }

    public void SetColor(Color color) {
        spriteRenderer.color = color;
    }

    public void SetRadius(float radius) {
        //constBuffer.SetData(new float[4] { 0.2f, 0.5f, 0.8f, 1.0f });
        //spriteRenderer.sharedMaterial.SetConstantBuffer("test", constBuffer, 0, 4 * sizeof(float));
        //return;
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
